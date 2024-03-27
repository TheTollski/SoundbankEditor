using BNKEditor.WwiseObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BNKEditor
{
	public class BnkReader
	{
		public BnkReader() { }

		public List<WwiseRootObject> Parse(BinaryReader binaryReader)
		{
			var wwiseObjects = new List<WwiseRootObject>();
			while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
			{
				var header = new WwiseRootObjectHeader
				{
					DwTag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4)),
					DwChunkSize = binaryReader.ReadUInt32(),
				};
				binaryReader.BaseStream.Position -= 8;

				if (header.DwTag == Enum.GetName(WwiseRootObjectType.BKHD))
				{
					wwiseObjects.Add(new BankHeader(binaryReader));
				}
				else if (header.DwTag == Enum.GetName(WwiseRootObjectType.HIRC))
				{
					wwiseObjects.Add(new HircChunk(binaryReader));
				}
				else if (header.DwTag == Enum.GetName(WwiseRootObjectType.STID))
				{
					wwiseObjects.Add(new StringMappingChunk(binaryReader));
				}
				else
				{
					Console.WriteLine($"Unknown object tag '{header.DwTag}', skipping {header.DwChunkSize} bytes.");
					binaryReader.ReadBytes((int)header.DwChunkSize);
				}
			}

			return wwiseObjects;
		}
	}
}
