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

		public List<HircItem> HircItems
		{
			get
			{
				HircChunk hircChunk = (HircChunk)wwiseRootObjects.Single(wro => wro.Tag == "HIRC");
				return hircChunk.HircItems;
			}
		}

		private SoundBank(List<WwiseRootObject> wwiseRootObjects)
		{
			this.wwiseRootObjects = wwiseRootObjects;
		}

		public static SoundBank CreateFromBnkFile(string inputBnkFilePath)
		{
			using FileStream fileStream = File.OpenRead(inputBnkFilePath);
			using BinaryReader binaryReader = new BinaryReader(fileStream);

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

		public static SoundBank CreateFromJsonFile(string inputJsonFilePath)
		{
			string fileText = File.ReadAllText(inputJsonFilePath);

			List<WwiseRootObject>? wwiseRootObjects = JsonSerializer.Deserialize<List<WwiseRootObject>>(fileText);
			if (wwiseRootObjects == null)
			{
				throw new Exception("Unable to parse Wwise objects from JSON file.");
			}

			return new SoundBank(wwiseRootObjects);
		}
		public void WriteToBnkFile(string outputBnkFilePath)
		{
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

			for (int i = 0; i < wwiseRootObjects.Count; i++)
			{
				wwiseRootObjects[i].WriteToBinary(binaryWriter);
			}

			byte[] serializedBytes = memoryStream.ToArray();
			File.WriteAllBytes(outputBnkFilePath, serializedBytes);
		}

		public void WriteToJsonFile(string outputJsonFilePath)
		{
			WwiseShortIdUtility.AddNames(File.ReadAllLines("TWA_Names.txt").ToList());

			string bnkJson = JsonSerializer.Serialize(
				wwiseRootObjects,
				new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }
			);

			File.WriteAllText(outputJsonFilePath, bnkJson);
		}
	}
}
