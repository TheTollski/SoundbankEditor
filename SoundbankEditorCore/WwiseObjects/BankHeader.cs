using SoundbankEditor.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects
{
	public class BankHeader : WwiseRootObject
	{
		public string? Tag { get; set; }
		public uint BankGeneratorVersion { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint SoundBankID { get; set; }
		public uint LanguageID { get; set; }
		public uint FeedbackInBank { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint ProjectID { get; set; }
		public byte[]? Padding { get; set; }

		public BankHeader()
		{
		}

		public BankHeader(BinaryReader binaryReader)
		{
			Tag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4));
			uint chunkSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;

			BankGeneratorVersion = binaryReader.ReadUInt32();
			SoundBankID = binaryReader.ReadUInt32();
			LanguageID = binaryReader.ReadUInt32();
			FeedbackInBank = binaryReader.ReadUInt32();
			ProjectID = binaryReader.ReadUInt32();

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < chunkSize)
			{
				Padding = binaryReader.ReadBytes((int)chunkSize - bytesReadFromThisObject);
			}
		}

		public uint ComputeTotalSize()
		{
			uint size = 28;
			if (Padding != null) size += (uint)Padding.Length;
			return size;
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

			binaryWriter.Write(BankGeneratorVersion);
			binaryWriter.Write(SoundBankID);
			binaryWriter.Write(LanguageID);
			binaryWriter.Write(FeedbackInBank);
			binaryWriter.Write(ProjectID);
			if (Padding != null)
			{
				binaryWriter.Write(Padding);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected BKHD chunk size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
