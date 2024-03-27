using SoundbankEditor.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects
{
	public class BankHeader : WwiseRootObject
	{
		public WwiseRootObjectHeader Header { get; set; } = new WwiseRootObjectHeader();
		public uint DwBankGeneratorVersion { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint DwSoundBankID { get; set; }
		public uint DwLanguageID { get; set; }
		public uint BFeedbackInBank { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint DwProjectID { get; set; }

		public byte[]? Padding { get; set; }

		public BankHeader()
		{
		}

		public BankHeader(BinaryReader binaryReader)
		{
			Header = new WwiseRootObjectHeader
			{
				DwTag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4)),
				DwChunkSize = binaryReader.ReadUInt32(),
			};

			long position = binaryReader.BaseStream.Position;

			DwBankGeneratorVersion = binaryReader.ReadUInt32();
			DwSoundBankID = binaryReader.ReadUInt32();
			DwLanguageID = binaryReader.ReadUInt32();
			BFeedbackInBank = binaryReader.ReadUInt32();
			DwProjectID = binaryReader.ReadUInt32();

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < Header.DwChunkSize)
			{
				Padding = binaryReader.ReadBytes((int)Header.DwChunkSize - bytesReadFromThisObject);
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			Header.WriteToBinary(binaryWriter);

			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(DwBankGeneratorVersion);
			binaryWriter.Write(DwSoundBankID);
			binaryWriter.Write(DwLanguageID);
			binaryWriter.Write(BFeedbackInBank);
			binaryWriter.Write(DwProjectID);
			if (Padding != null)
			{
				binaryWriter.Write(Padding);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != Header.DwChunkSize)
			{
				throw new Exception($"Expected BKHD chunk size to be {Header.DwChunkSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
