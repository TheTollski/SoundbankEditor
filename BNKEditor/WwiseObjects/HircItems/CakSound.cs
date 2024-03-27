using BNKEditor.Utility;
using BNKEditor.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CAkSound : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint UlID { get; set; }
		public AkBankSourceData AkBankSourceData { get; set; } = new AkBankSourceData();
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();

		public CAkSound() { }

		public CAkSound(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			AkBankSourceData = new AkBankSourceData(binaryReader);
			NodeBaseParams = new NodeBaseParams(binaryReader);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CakSound '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);

			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(UlID);
			AkBankSourceData.WriteToBinary(binaryWriter);
			NodeBaseParams.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != DwSectionSize)
			{
				throw new Exception($"Expected CAkSound '{UlID}' section size to be {DwSectionSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class AkBankSourceData : WwiseObject
	{
		public uint UlPluginId { get; set; }
		public byte StreamType { get; set; }
		public AkMediaInformation AkMediaInformation { get; set; } = new AkMediaInformation();

		public AkBankSourceData() { }

		public AkBankSourceData(BinaryReader binaryReader)
		{
			UlPluginId = binaryReader.ReadUInt32();
			StreamType = binaryReader.ReadByte();
			AkMediaInformation = new AkMediaInformation(binaryReader);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(UlPluginId);
			binaryWriter.Write(StreamType);
			AkMediaInformation.WriteToBinary(binaryWriter);
		}
	}

	public class AkMediaInformation : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint SourceId { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint FileId { get; set; }
		public uint InMemoryMediaSize { get; set; }
		public byte SourceBits {  get; set; }

		public AkMediaInformation() { }

		public AkMediaInformation(BinaryReader binaryReader)
		{
			SourceId = binaryReader.ReadUInt32();
			FileId = binaryReader.ReadUInt32();
			InMemoryMediaSize = binaryReader.ReadUInt32();
			SourceBits = binaryReader.ReadByte();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(SourceId);
			binaryWriter.Write(FileId);
			binaryWriter.Write(InMemoryMediaSize);
			binaryWriter.Write(SourceBits);
		}
	}
}
