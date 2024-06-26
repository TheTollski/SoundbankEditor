﻿using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CAkSound : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.Sound;

		private HircType _hircType;
		public HircType EHircType
		{
			get
			{
				return _hircType;
			}
			set
			{
				if (value != EXPECTED_HIRC_TYPE)
				{
					throw new Exception($"HIRC item of type '{GetType().Name}' cannot have {nameof(EHircType)} of '{(byte)value}', it must be '{(byte)EXPECTED_HIRC_TYPE}'.");
				}

				_hircType = value;
			}
		}

		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint UlID { get; set; }
		public AkBankSourceData AkBankSourceData { get; set; } = new AkBankSourceData();
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();

		public CAkSound()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkSound(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			AkBankSourceData = new AkBankSourceData(binaryReader);
			NodeBaseParams = new NodeBaseParams(binaryReader);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkSound '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 9 + AkBankSourceData.ComputeTotalSize() + NodeBaseParams.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkSound '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			return knownValidationErrors;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			uint expectedSize = ComputeTotalSize() - 5;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.Write(UlID);

			AkBankSourceData.WriteToBinary(binaryWriter);
			NodeBaseParams.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkSound '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

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
		public byte SourceBits {  get; set; }

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
