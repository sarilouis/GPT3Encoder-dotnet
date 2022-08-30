// See https://aka.ms/new-console-template for more information
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