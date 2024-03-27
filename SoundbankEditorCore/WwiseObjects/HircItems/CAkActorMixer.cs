using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CAkActorMixer : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.ActorMixer;

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
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> ChildIds { get; set; } = new List<uint>();

		public CAkActorMixer()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkActorMixer(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			NodeBaseParams = new NodeBaseParams(binaryReader);
			uint childCount = binaryReader.ReadUInt32();
			for (int i = 0; i < childCount; i++)
			{
				ChildIds.Add(binaryReader.ReadUInt32());
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkActorMixer '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 13 + NodeBaseParams.ComputeTotalSize() + (uint)(ChildIds.Count * 4);
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkActorMixer '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate NodeBaseParams
			knownValidationErrors.AddRange(NodeBaseParams.GetKnownValidationErrors(soundbank).Select(s => $"CAkActorMixer's '{UlID}' NodeBaseParams.{s}"));

			return knownValidationErrors;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			uint expectedSize = ComputeTotalSize() - 5;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.Write(UlID);

			NodeBaseParams.WriteToBinary(binaryWriter);
			binaryWriter.Write((uint)ChildIds.Count);
			for (int i = 0; i < ChildIds.Count; i++)
			{
				binaryWriter.Write(ChildIds[i]);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkActorMixer '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
