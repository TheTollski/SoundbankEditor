using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core
{
	public class SoundData
	{
		public List<DatEvent> Events { get; set; }
		public List<DatEnumWithStrings> Enums1 { get; set; } // These appears to be the different Groups that are available for switch containers to use.
		public List<DatEnumWithStrings> Enums2 { get; set; }
		public List<DatEnumWithUints> Enums3 { get; set; }
		public List<string> Settings { get; set; }
		public List<DatUnknownParent> Unknowns { get; set; }

		[JsonConstructor]
		public SoundData(
			List<DatEvent> events,
			List<DatEnumWithStrings> enums1,
			List<DatEnumWithStrings> enums2,
			List<DatEnumWithUints> enums3,
			List<string> settings,
			List<DatUnknownParent> unknowns
		)
		{
			Events = events;
			Enums1 = enums1;
			Enums2 = enums2;
			Enums3 = enums3;
			Settings = settings;
			Unknowns = unknowns;
		}

		public void AddNames()
		{
			var names = new List<string>();
			names.AddRange(Events.Select(e => e.Key));
			names.AddRange(Enums1.Select(e => e.Key));
			names.AddRange(Enums1.SelectMany(e => e.Values));
			names.AddRange(Enums2.Select(e => e.Key));
			names.AddRange(Enums2.SelectMany(e => e.Values));
			names.AddRange(Enums3.Select(e => e.Key));
			names.AddRange(Settings);

			WwiseShortIdUtility.AddNames(names);
		}

		public static SoundData CreateFromDatFile(string inputDatFilePath)
		{
			using FileStream fileStream = File.OpenRead(inputDatFilePath);
			using BinaryReader binaryReader = new BinaryReader(fileStream);

			uint eventCount = binaryReader.ReadUInt32();
			var events = new List<DatEvent>();
			for (int i = 0; i < eventCount; i++)
			{
				uint keySize = binaryReader.ReadUInt32();
				string key = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)keySize));
				uint value = binaryReader.ReadUInt32();

				events.Add(new DatEvent { Key = key, Value = value});
			}

			uint enums1Count = binaryReader.ReadUInt32();
			var enums1 = new List<DatEnumWithStrings>();
			for (int i = 0; i < enums1Count; i++)
			{
				uint keySize = binaryReader.ReadUInt32();
				string key = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)keySize));
				
				uint valueCount = binaryReader.ReadUInt32();
				var values = new List<string>();
				for (int j = 0; j < valueCount; j++)
				{
					uint stringSize = binaryReader.ReadUInt32();
					string value = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize));

					values.Add(value);
				}

				enums1.Add(new DatEnumWithStrings { Key = key, Values = values});
			}

			uint enums2Count = binaryReader.ReadUInt32();
			var enums2 = new List<DatEnumWithStrings>();
			for (int i = 0; i < enums2Count; i++)
			{
				uint keySize = binaryReader.ReadUInt32();
				string key = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)keySize));

				uint valueCount = binaryReader.ReadUInt32();
				var values = new List<string>();
				for (int j = 0; j < valueCount; j++)
				{
					uint stringSize = binaryReader.ReadUInt32();
					string value = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)stringSize));

					values.Add(value);
				}

				enums2.Add(new DatEnumWithStrings { Key = key, Values = values });
			}

			uint enums3Count = binaryReader.ReadUInt32();
			var enums3 = new List<DatEnumWithUints>();
			for (int i = 0; i < enums3Count; i++)
			{
				uint keySize = binaryReader.ReadUInt32();
				string key = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)keySize));

				uint valueCount = binaryReader.ReadUInt32();
				var values = new List<uint>();
				for (int j = 0; j < valueCount; j++)
				{
					values.Add(binaryReader.ReadUInt32());
				}

				enums3.Add(new DatEnumWithUints { Key = key, Values = values });
			}

			uint settingCount = binaryReader.ReadUInt32();
			var settings = new List<string>();
			for (int i = 0; i < settingCount; i++)
			{
				uint settingSize = binaryReader.ReadUInt32();
				string setting = Encoding.UTF8.GetString(binaryReader.ReadBytes((int)settingSize));

				settings.Add(setting);
			}

			uint unknownCount = binaryReader.ReadUInt32();
			var unknowns = new List<DatUnknownParent>();
			for (int i = 0; i < unknownCount; i++)
			{
				var unknownParent = new DatUnknownParent();
				unknownParent.Key = binaryReader.ReadUInt32();
				
				uint unknownParentValueCount = binaryReader.ReadUInt32();
				unknownParent.Values = new List<DatUnknownChild>();
				for (int j = 0; j < unknownParentValueCount; j++)
				{
					var unknownChild = new DatUnknownChild();
					unknownChild.Key = binaryReader.ReadUInt32();

					uint unknownChildValueCount = binaryReader.ReadUInt32();
					unknownChild.Values = new List<uint>();
					for (int k = 0; k < unknownChildValueCount; k++)
					{
						unknownChild.Values.Add(binaryReader.ReadUInt32());
					}

					unknownParent.Values.Add(unknownChild);
				}

				unknowns.Add(unknownParent);
			}

			return new SoundData(events, enums1, enums2, enums3, settings, unknowns);
		}

		public static SoundData CreateFromJsonFile(string inputJsonFilePath)
		{
			string fileText = File.ReadAllText(inputJsonFilePath);

			SoundData? soundData = JsonSerializer.Deserialize<SoundData>(fileText);
			if (soundData == null)
			{
				throw new Exception("Unable to parse sound data from JSON file.");
			}

			return soundData;
		}

		private void WriteStringToBinary(BinaryWriter binaryWriter, string s)
		{
			binaryWriter.Write((uint)s.Length);
			for (int i = 0; i < s.Length; i++)
			{
				binaryWriter.Write(s[i]);
			}
		}

		public void WriteToDatFile(string outputDatFilePath)
		{
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

			binaryWriter.Write((uint)Events.Count);
			for (int i = 0; i < Events.Count; i++)
			{
				WriteStringToBinary(binaryWriter, Events[i].Key);
				binaryWriter.Write(Events[i].Value);
			}

			binaryWriter.Write((uint)Enums1.Count);
			for (int i = 0; i < Enums1.Count; i++)
			{
				WriteStringToBinary(binaryWriter, Enums1[i].Key);
				binaryWriter.Write((uint)Enums1[i].Values.Count);
				for (int j = 0; j < Enums1[i].Values.Count; j++)
				{
					WriteStringToBinary(binaryWriter, Enums1[i].Values[j]);
				}
			}

			binaryWriter.Write((uint)Enums2.Count);
			for (int i = 0; i < Enums2.Count; i++)
			{
				WriteStringToBinary(binaryWriter, Enums2[i].Key);
				binaryWriter.Write((uint)Enums2[i].Values.Count);
				for (int j = 0; j < Enums2[i].Values.Count; j++)
				{
					WriteStringToBinary(binaryWriter, Enums2[i].Values[j]);
				}
			}

			binaryWriter.Write((uint)Enums3.Count);
			for (int i = 0; i < Enums3.Count; i++)
			{
				WriteStringToBinary(binaryWriter, Enums3[i].Key);
				binaryWriter.Write((uint)Enums3[i].Values.Count);
				for (int j = 0; j < Enums3[i].Values.Count; j++)
				{
					binaryWriter.Write(Enums3[i].Values[j]);
				}
			}

			binaryWriter.Write((uint)Settings.Count);
			for (int i = 0; i < Settings.Count; i++)
			{
				WriteStringToBinary(binaryWriter, Settings[i]);
			}

			binaryWriter.Write((uint)Unknowns.Count);
			for (int i = 0; i < Unknowns.Count; i++)
			{
				binaryWriter.Write(Unknowns[i].Key);
				binaryWriter.Write((uint)Unknowns[i].Values.Count);
				for (int j = 0; j < Unknowns[i].Values.Count; j++)
				{
					binaryWriter.Write(Unknowns[i].Values[j].Key);
					binaryWriter.Write((uint)Unknowns[i].Values[j].Values.Count);
					for (int k = 0; k < Unknowns[i].Values[j].Values.Count; k++)
					{
						binaryWriter.Write(Unknowns[i].Values[j].Values[k]);
					}
				}
			}

			byte[] serializedBytes = memoryStream.ToArray();
			File.WriteAllBytes(outputDatFilePath, serializedBytes);
		}

		public void WriteToJsonFile(string outputJsonFilePath)
		{
			AddNames();

			string json = JsonSerializer.Serialize(
				this,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }
			);

			File.WriteAllText(outputJsonFilePath, json);
		}
	}

	public class DatEvent
	{
		public string Key { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint Value { get; set; }
	}

	public class DatEnumWithStrings
	{
		public string Key { get; set; }
		public List<string> Values { get; set; }
	}

	public class DatEnumWithUints
	{
		public string Key { get; set; }
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> Values { get; set; }
	}

	public class DatUnknownParent
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint Key { get; set; }
		public List<DatUnknownChild> Values { get; set; }
	}

	public class DatUnknownChild
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint Key { get; set; }
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> Values { get; set; }
	}
}
