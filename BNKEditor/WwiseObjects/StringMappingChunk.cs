using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects
{
	public class StringMappingChunk : WwiseRootObject
	{
		public WwiseRootObjectHeader Header { get; set; } = new WwiseRootObjectHeader();
		public uint UiType { get; set; }
		public uint UiSize { get; set; }
		public List<AKBKHashHeader> BankIDToFileName { get; set; } = new List<AKBKHashHeader>();

		public StringMappingChunk() { }

		public StringMappingChunk(BinaryReader binaryReader)
		{
			Header = new WwiseRootObjectHeader
			{
				DwTag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4)),
				DwChunkSize = binaryReader.ReadUInt32(),
			};

			long position = binaryReader.BaseStream.Position;

			UiType = binaryReader.ReadUInt32();
			UiSize = binaryReader.ReadUInt32();

			while (binaryReader.BaseStream.Position < position + Header.DwChunkSize)
			{
				BankIDToFileName.Add(new AKBKHashHeader(binaryReader));
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			Header.WriteToBinary(binaryWriter);
			binaryWriter.Write(UiType);
			binaryWriter.Write(UiSize);
			for (int i = 0; i < BankIDToFileName.Count; i++)
			{
				BankIDToFileName[i].WriteToBinary(binaryWriter);
			}
		}
	}

	public class AKBKHashHeader : WwiseObject
	{
		public uint BankId { get; set; }
		public byte StringSize { get; set; }
		public string FileName { get; set; } = string.Empty;

		public AKBKHashHeader() { }

		public AKBKHashHeader(BinaryReader binaryReader)
		{
			BankId = binaryReader.ReadUInt32();
			StringSize = binaryReader.ReadByte();
			FileName = Encoding.UTF8.GetString(binaryReader.ReadBytes(StringSize));
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(BankId);
			binaryWriter.Write(StringSize);
			for (int i = 0; i < StringSize; i++)
			{
				binaryWriter.Write(FileName[i]);
			}
		}
	}
}
