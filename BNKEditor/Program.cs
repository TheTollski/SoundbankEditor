// See https://aka.ms/new-console-template for more information
using BNKEditor;
using BNKEditor.WwiseObjects;
using System.Diagnostics;
using System.IO;
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

int lastDotIndex = args[0].LastIndexOf(".");
if (lastDotIndex == -1)
{
	Console.WriteLine($"Converting name to Wwise ShortId...");
	Console.WriteLine($"ShortId of '{args[0]}': {WwiseShortIdUtility.ConvertToShortId(args[0])}");
	Console.WriteLine($"ShortId30Bit of '{args[0]}': {WwiseShortIdUtility.ConvertToShortId30Bit(args[0])}");

	return;
}

string fileExtension = args[0].Substring(lastDotIndex + 1);

if (fileExtension == "bnk")
{
	Console.WriteLine($"Converting BNK to JSON...");

	using FileStream fileStream = File.OpenRead(args[0]);
	using BinaryReader binaryReader = new BinaryReader(fileStream);

	BnkReader bnkReader = new BnkReader();
	List<WwiseRootObject> wwiseRootObjects = bnkReader.Parse(binaryReader);

	string bnkJson = JsonSerializer.Serialize(
		wwiseRootObjects,
		new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
	);

	string path = $"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}_temp.json";

	Console.WriteLine($"Writing JSON to file: '{path}'");

	File.WriteAllText(path, bnkJson);

	Console.WriteLine("JSON file saved.");

	ProcessStartInfo psi = new ProcessStartInfo();
	psi.FileName = path;
	psi.UseShellExecute = true;
	Process.Start(psi);

	return;
}

if (fileExtension == "json")
{
	Console.WriteLine($"Converting JSON to BNK...");

	string fileText = File.ReadAllText(args[0]);

	List<WwiseRootObject>? wwiseRootObjects = JsonSerializer.Deserialize<List<WwiseRootObject>>(fileText);
	if (wwiseRootObjects == null)
	{
		Console.WriteLine($"Unable to parse Wwise objects from JSON file. Exiting...");
		return;
	}

	using MemoryStream memoryStream = new MemoryStream();
	using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

	for (int i = 0; i < wwiseRootObjects.Count; i++)
	{
		wwiseRootObjects[i].WriteToBinary(binaryWriter);
	}

	byte[] serializedBytes = memoryStream.ToArray();

	string path = $"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}_temp.bnk";

	Console.WriteLine($"Writing to BNK file: '{path}'");

	File.WriteAllBytes(path, serializedBytes);

	Console.WriteLine("BNK file saved.");

	return;
}

Console.WriteLine($"Unsupported file extension '{fileExtension}'. Exiting...");