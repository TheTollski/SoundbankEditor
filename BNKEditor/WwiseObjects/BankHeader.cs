using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects
{
	public class BankHeader : WwiseRootObject
	{
		public WwiseRootObjectHeader Header { get; set; }
		public uint DwBankGeneratorVersion { get; set; }
		public uint DwSoundBankID { get; set; }     // Name of the file
		public uint DwLanguageID { get; set; }      // Enum 11 - English
		public uint BFeedbackInBank { get; set; }
		public uint DwProjectID { get; set; }

		public byte[]? Padding { get; set; }

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

			binaryWriter.Write(DwBankGeneratorVersion);
			binaryWriter.Write(DwSoundBankID);
			binaryWriter.Write(DwLanguageID);
			binaryWriter.Write(BFeedbackInBank);
			binaryWriter.Write(DwProjectID);
			if (Padding != null)
			{
				binaryWriter.Write(Padding);
			}
		}
	}
}
