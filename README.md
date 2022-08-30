# GPT3Encoder
.NET BPE Encoder Decoder for GPT-2 / GPT-3

## About
GPT-2 and GPT-3 use byte pair encoding (BPE) to turn text into a series of integers to feed into the model. This is a C# implementation of [OpenAI](https://github.com/openai/)'s original [python encoder/decoder](https://github.com/openai/gpt-2/blob/master/src/encoder.py). Also inspired by [Latitude](https://github.com/latitudegames)'s [Javascript GPT-3-Encoder](https://github.com/latitudegames/GPT-3-Encoder).

## Usage

```c#
var encoder = new GPT3Encoder.Encoder();

// Encode
var encodedString = encoder.Encode("Arbitrarily sampled sentence to encode. Should result in 14 tokens.");
Console.WriteLine("Encoded string:");
Console.WriteLine($"[{string.Join(", ", encodedString)}]");
Console.WriteLine($"\nToken count: {encodedString.Count()}");

// Decode
var decodedString = encoder.Decode(encodedString);
Console.WriteLine("\nDecoded string:");
Console.WriteLine(decodedString);
Console.WriteLine($"\n|{"Token",-10}|String");
Console.WriteLine($"|{"-----",-10}|------");
encodedString.ToList().ForEach(token => Console.WriteLine($"|{token,-10}|{encoder.Decode(new int[] { token })}"));
```
