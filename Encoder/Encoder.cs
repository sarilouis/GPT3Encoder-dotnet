using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GPT3Encoder;

public class Encoder
{
    private IReadOnlyDictionary<string, int> _encoder;
    private IReadOnlyDictionary<int, string> _decoder;
    private IReadOnlyDictionary<int, string> _byteEncoder;
    private IReadOnlyDictionary<string, int> _byteDecoder;
    private IReadOnlyDictionary<(string, string), int> _bpeRanks;
    private Dictionary<string, string> _cache;

    public Encoder()
    {
        try
        {
            _encoder = (Dictionary<string, int>)JsonSerializer.Deserialize(File.ReadAllText(@"Assets/encoder.json"), typeof(Dictionary<string, int>));
        }
        catch (Exception e)
        {
            throw new Exception("Failed to initialize Encoder using 'encoder.json' file.", e);
        }
        _byteEncoder = bytesToUnicode();
        _decoder = _encoder.ToDictionary(x => (int)x.Value, x => x.Key);
        _byteDecoder = _byteEncoder.ToDictionary(x => x.Value, x => (int)x.Key);

        Dictionary<(string, string), int> ranks = new();
        var lines = File.ReadAllText(@"Assets/vocab.bpe", Encoding.UTF8).Trim().Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Skip(1);
        var i = 0;
        foreach (string s in lines)
        {
            var t = s.Split(" ").ToArray();
            ranks[(t[0], t[1])] = i++;
        }
        _bpeRanks = ranks;

        _cache = new();
    }

    public IEnumerable<int> Encode(string text)
    {
        IEnumerable<int> bpeTokens = new int[] { };
        Regex re = new Regex(@"/'s|'t|'re|'ve|'m|'l l|'d| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+/gu");

        foreach (var token in re.Matches(text).Select(m => m.Value))
        {
            var tmp = string.Join("", Encoding.UTF8.GetBytes(token).Select(b => _byteEncoder[b]));
            var newTokens = bpe(tmp).Split(" ").Select(x => _encoder[x]);
            bpeTokens = bpeTokens.Concat(newTokens);
        }

        return bpeTokens;
    }

    public string Decode(IEnumerable<int> tokens)
    {
        return Encoding.UTF8.GetString(string.Join("", tokens.Select(x => _decoder[x])).ToCharArray().Select(x => (byte)_byteDecoder[x.ToString()]).ToArray());
    }

    private IEnumerable<(string, string)> GetPairs(IEnumerable<string> word)
    {
        var prev = word.First();
        foreach (var s in word.Skip(1))
        {
            yield return (prev, s);
            prev = s;
        }
    }

    private Dictionary<int, string> bytesToUnicode()
    {
        var bs = Enumerable.Range((int)'!', (int)'~' - (int)'!' + 1).
            Concat(Enumerable.Range((int)'¡', (int)'¬' - (int)'¡' + 1)).
            Concat(Enumerable.Range((int)'®', (int)'ÿ' - (int)'®' + 1));

        var cs = bs.Select(x => x);
        var n = 0;
        for (var b = 0; b < 256; b++)
        {
            if (!bs.Contains(b))
            {
                bs = bs.Append(b);
                cs = cs.Append(256 + n);
                n = n + 1;
            }
        }

        var tmp = cs.Select(x => ((char)x).ToString());

        Dictionary<int, string> result = new Dictionary<int, string>();
        for (int i = 0; i < bs.Count(); i++)
            result[bs.ElementAt(i)] = tmp.ElementAt(i);
        return result;
    }

    private string bpe(string token)
    {
        if (_cache.ContainsKey(token))
            return _cache[token];

        var word = token.ToCharArray().Select(c => c.ToString()).ToArray();
        var pairs = GetPairs(word);

        if (pairs.Count() == 0)
            return token;

        while (true)
        {
            var minPairs = new Dictionary<int, (string, string)>();
            int rank;
            pairs.ToList().ForEach(p => minPairs[_bpeRanks.TryGetValue(p, out rank) ? rank : int.MaxValue] = p);

            var bigram = minPairs[minPairs.Min(p => p.Key)];
            if (!_bpeRanks.ContainsKey(bigram))
                break;

            var first = bigram.Item1;
            var second = bigram.Item2;

            IEnumerable<string> newWord = new string[] { };
            int i = 0;

            while (i < word.Length)
            {
                var j = Array.IndexOf(word, first, i);
                if (j == -1)
                {
                    newWord = newWord.Concat(word.Skip(i));
                    break;
                }
                newWord = newWord.Concat(word.Skip(i).Take(j - i));

                i = j;

                if (word[i] == first && i < word.Length - 1 && word[i + 1] == second)
                {
                    newWord = newWord.Append(first + second);
                    i = i + 2;
                }
                else
                {
                    newWord = newWord.Append(word[i]);
                    i++;
                }
            }

            word = newWord.ToArray();
            if (word.Length == 1)
                break;
            else
                pairs = GetPairs(word);
        }

        var tmp = string.Join(" ", word);
        _cache[token] = tmp;

        return tmp;
    }
}