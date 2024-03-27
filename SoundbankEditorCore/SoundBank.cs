using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoundbankEditor.Core
{
	public class SoundBank
	{
		private List<WwiseRootObject> wwiseRootObjects;

		public uint GeneratorVersion
		{
			get
			{
				BankHeader bankHeader = (BankHeader)wwiseRootObjects.Single(wro => wro.Tag == Enum.GetName(WwiseRootObjectType.BKHD));
				return bankHeader.BankGeneratorVersion;
			}
		}

		public List<HircItem> HircItems
		{
			get
			{
				HircChunk hircChunk = (HircChunk)wwiseRootObjects.Single(wro => wro.Tag == Enum.GetName(WwiseRootObjectType.HIRC));
				return hircChunk.HircItems;
			}
		}

		private SoundBank(List<WwiseRootObject> wwiseRootObjects)
		{
			this.wwiseRootObjects = wwiseRootObjects;
		}

		public static SoundBank CreateFromBinary(BinaryReader binaryReader)
		{
			var wwiseRootObjects = new List<WwiseRootObject>();
			while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
			{
				string tag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4));
				uint chunkSize = binaryReader.ReadUInt32();
				binaryReader.BaseStream.Position -= 8;

				if (tag == Enum.GetName(WwiseRootObjectType.BKHD))
				{
					wwiseRootObjects.Add(new BankHeader(binaryReader));
				}
				else if (tag == Enum.GetName(WwiseRootObjectType.HIRC))
				{
					wwiseRootObjects.Add(new HircChunk(binaryReader));
				}
				else if (tag == Enum.GetName(WwiseRootObjectType.STID))
				{
					wwiseRootObjects.Add(new StringMappingChunk(binaryReader));
				}
				else
				{
					throw new Exception($"Unknown object tag '{tag}', skipping {chunkSize} bytes.");
				}
			}

			return new SoundBank(wwiseRootObjects);
		}

		public static SoundBank CreateFromBnkFile(string inputBnkFilePath)
		{
			using FileStream fileStream = File.OpenRead(inputBnkFilePath);
			using BinaryReader binaryReader = new BinaryReader(fileStream);

			return CreateFromBinary(binaryReader);
		}

		public static SoundBank CreateFromJson(string json)
		{
			List<WwiseRootObject>? wwiseRootObjects = JsonSerializer.Deserialize<List<WwiseRootObject>>(json);
			if (wwiseRootObjects == null)
			{
				throw new Exception("Unable to parse Wwise objects from JSON file.");
			}

			return new SoundBank(wwiseRootObjects);
		}

		public static SoundBank CreateFromJsonFile(string inputJsonFilePath)
		{
			string fileText = File.ReadAllText(inputJsonFilePath);
			return CreateFromJson(fileText);
		}

		public string ToJson()
		{
			return JsonSerializer.Serialize(
				wwiseRootObjects,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
			);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			for (int i = 0; i < wwiseRootObjects.Count; i++)
			{
				wwiseRootObjects[i].WriteToBinary(binaryWriter);
			}
		}

		public void WriteToBnkFile(string outputBnkFilePath)
		{
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			
			WriteToBinary(binaryWriter);

			byte[] serializedBytes = memoryStream.ToArray();
			File.WriteAllBytes(outputBnkFilePath, serializedBytes);
		}

		public void WriteToJsonFile(string outputJsonFilePath)
		{
			File.WriteAllText(outputJsonFilePath, ToJson());
		}
	}
}
