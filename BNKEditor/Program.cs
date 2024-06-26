﻿// See https://aka.ms/new-console-template for more information
using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects;
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

	WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList());
	SoundBank soundBank = SoundBank.CreateFromBnkFile(args[0]);

	string outputJsonFilePath = $"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}_temp.json";
	soundBank.WriteToJsonFile(outputJsonFilePath);

	Console.WriteLine($"JSON file saved: '{outputJsonFilePath}'");

	ProcessStartInfo psi = new ProcessStartInfo();
	psi.FileName = outputJsonFilePath;
	psi.UseShellExecute = true;
	Process.Start(psi);

	return;
}

if (fileExtension == "json")
{
	if (File.ReadAllText(args[0]).Contains("\"Events\""))
	{
		Console.WriteLine($"Converting JSON to DAT...");

		SoundData soundData = SoundData.CreateFromJsonFile(args[0]);

		string outputDatFilePath = $"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}_temp.dat";
		soundData.WriteToDatFile(outputDatFilePath);

		Console.WriteLine($"DAT file saved: '{outputDatFilePath}'");

		return;
	}

	Console.WriteLine($"Converting JSON to BNK...");

	SoundBank soundBank = SoundBank.CreateFromJsonFile(args[0]);

	string outputBnkFilePath = $"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}_temp.bnk";
	soundBank.WriteToBnkFile(outputBnkFilePath);

	Console.WriteLine($"BNK file saved: '{outputBnkFilePath}'");

	return;
}

if (fileExtension == "dat")
{
	Console.WriteLine($"Converting DAT to JSON...");

	SoundData soundData = SoundData.CreateFromDatFile(args[0]);

	string outputJsonFilePath = $"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}_temp.json";
	soundData.WriteToJsonFile(outputJsonFilePath);

	Console.WriteLine($"JSON file saved: '{outputJsonFilePath}'");

	ProcessStartInfo psi = new ProcessStartInfo();
	psi.FileName = outputJsonFilePath;
	psi.UseShellExecute = true;
	Process.Start(psi);

	return;
}

Console.WriteLine($"Unsupported file extension '{fileExtension}'. Exiting...");