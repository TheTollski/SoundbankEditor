// See https://aka.ms/new-console-template for more information
using BNKEditor;
using BNKEditor.WwiseObjects;
using System.Diagnostics;
using System.Text.Json;

Console.WriteLine($"args: {string.Join(',', args)}");

if (args.Length < 1)
{
	Console.WriteLine($"A path must be provided. Exiting...");
	return;
}

if (args.Length > 1)
{
	Console.WriteLine($"Only 1 argument is supported. Exiting...");
	return;
}

using FileStream fileStream = File.OpenRead(args[0]);
using BinaryReader binaryReader = new BinaryReader(fileStream);

BnkReader bnkReader = new BnkReader();
List<WwiseRootObject> wwiseRootObjects = bnkReader.Parse(binaryReader);

string bnkJson = JsonSerializer.Serialize(
	wwiseRootObjects,
	new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
);

Console.Write("Writing JSON to file.");

string path = "temp.json";
File.WriteAllText(path, bnkJson);

Console.Write("JSON written to file.");

ProcessStartInfo psi = new ProcessStartInfo();
psi.FileName = path;
psi.UseShellExecute = true;
Process.Start(psi);