using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditorCore.WwiseObjects.HircItems.Common
{
	public class AkBankSourceData : WwiseObject
	{
		public uint UlPluginId { get; set; }
		public byte StreamType { get; set; }
		public uint? Size { get; set; }

		public AkMediaInformation AkMediaInformation { get; set; } = new AkMediaInformation();

		public AkBankSourceData() { }

		public AkBankSourceData(BinaryReader binaryReader)
		{
			UlPluginId = binaryReader.ReadUInt32();
			StreamType = binaryReader.ReadByte();
			AkMediaInformation = new AkMediaInformation(binaryReader, StreamType);

			uint pluginType = UlPluginId & 0x000F;
			if (pluginType == 0x02)
			{
				Size = binaryReader.ReadUInt32();
			}
		}

		public uint ComputeTotalSize()
		{
			uint size = 5 + AkMediaInformation.ComputeTotalSize();
			if (Size != null) size += 4;
			return size;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(UlPluginId);
			binaryWriter.Write(StreamType);
			AkMediaInformation.WriteToBinary(binaryWriter);
			if (Size != null) binaryWriter.Write(Size.Value);
		}
	}

	public class AkMediaInformation : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint SourceId { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint FileId { get; set; }
		public uint? FileOffset { get; set; }
		public uint InMemoryMediaSize { get; set; }
		public byte SourceBits { get; set; }

		public AkMediaInformation() { }

		public AkMediaInformation(BinaryReader binaryReader, byte streamType)
		{
			SourceId = binaryReader.ReadUInt32();
			FileId = binaryReader.ReadUInt32();
			if (streamType == 0)
			{
				FileOffset = binaryReader.ReadUInt32();
			}
			InMemoryMediaSize = binaryReader.ReadUInt32();
			SourceBits = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return (uint)(13 + (FileOffset != null ? 4 : 0));
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(SourceId);
			binaryWriter.Write(FileId);
			if (FileOffset != null) binaryWriter.Write(FileOffset.Value);
			binaryWriter.Write(InMemoryMediaSize);
			binaryWriter.Write(SourceBits);
		}
	}
}
