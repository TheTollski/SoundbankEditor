using SoundbankEditor.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects
{
	public class StringMappingChunk : WwiseRootObject
	{
		public string? Tag { get; set; }
		public uint UiType { get; set; }
		public uint UiSize { get; set; }
		public List<AKBKHashHeader> BankIDToFileName { get; set; } = new List<AKBKHashHeader>();

		public StringMappingChunk() { }

		public StringMappingChunk(BinaryReader binaryReader)
		{
			Tag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4));
			uint chunkSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;

			UiType = binaryReader.ReadUInt32();
			UiSize = binaryReader.ReadUInt32();

			while (binaryReader.BaseStream.Position < position + chunkSize)
			{
				BankIDToFileName.Add(new AKBKHashHeader(binaryReader));
			}
		}

		public uint ComputeTotalSize()
		{
			return 16 + (uint)BankIDToFileName.Sum(b => b.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(Tag[0]);
			binaryWriter.Write(Tag[1]);
			binaryWriter.Write(Tag[2]);
			binaryWriter.Write(Tag[3]);
			uint expectedSize = ComputeTotalSize() - 8;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(UiType);
			binaryWriter.Write(UiSize);
			for (int i = 0; i < BankIDToFileName.Count; i++)
			{
				BankIDToFileName[i].WriteToBinary(binaryWriter);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new Exception($"Expected STID chunk size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class AKBKHashHeader : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint BankId { get; set; }
		public string FileName { get; set; } = string.Empty;

		public AKBKHashHeader() { }

		public AKBKHashHeader(BinaryReader binaryReader)
		{
			BankId = binaryReader.ReadUInt32();
			byte stringSize = binaryReader.ReadByte();

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			byte[] fileNameBytes = binaryReader.ReadBytes(stringSize);
			FileName = Encoding.GetEncoding(1252).GetString(fileNameBytes);
		}

		public uint ComputeTotalSize()
		{
			return 5 + (uint)FileName.Length;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(BankId);
			binaryWriter.Write((byte)FileName.Length);

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			byte[] fileNameBytes = Encoding.GetEncoding(1252).GetBytes(FileName);
			binaryWriter.Write(fileNameBytes);
		}
	}
}
