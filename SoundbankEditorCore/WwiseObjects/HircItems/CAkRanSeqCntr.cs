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
	public class CAkRanSeqCntr : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.RandomSequenceContainer;

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
		public ushort LoopCount { get; set; }
		public ushort LoopModMin { get; set; }
		public ushort LoopModMax { get; set; }
		public float TransitionTime { get; set; }
		public float TransitionTimeModMin { get; set; }
		public float TransitionTimeModMax { get; set; }
		public ushort AvoidRepeatCount { get; set; }
		public byte TransitionMode { get; set; }
		public byte RandomMode { get; set; }
		public byte Mode { get; set; }
		public byte ByBitVector { get; set; }
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> ChildIds { get; set; } = new List<uint>();
		public CAkPlayList CAkPlayList { get; set; } = new CAkPlayList();

		public CAkRanSeqCntr()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkRanSeqCntr(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			NodeBaseParams = new NodeBaseParams(binaryReader);
			LoopCount = binaryReader.ReadUInt16();
			LoopModMin = binaryReader.ReadUInt16();
			LoopModMax = binaryReader.ReadUInt16();
			TransitionTime = binaryReader.ReadSingle();
			TransitionTimeModMin = binaryReader.ReadSingle();
			TransitionTimeModMax = binaryReader.ReadSingle();
			AvoidRepeatCount = binaryReader.ReadUInt16();
			TransitionMode = binaryReader.ReadByte();
			RandomMode = binaryReader.ReadByte();
			Mode = binaryReader.ReadByte();
			ByBitVector = binaryReader.ReadByte();
			uint childCount = binaryReader.ReadUInt32();
			for (int i = 0; i < childCount; i++)
			{
				ChildIds.Add(binaryReader.ReadUInt32());
			}
			CAkPlayList = new CAkPlayList(binaryReader);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkRanSeqCntr '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 37 + NodeBaseParams.ComputeTotalSize() + (uint)(ChildIds.Count * 4) + CAkPlayList.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkRanSeqCntr '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate NodeBaseParams
			knownValidationErrors.AddRange(NodeBaseParams.GetKnownValidationErrors(soundbank).Select(s => $"CAkRanSeqCntr's '{UlID}' NodeBaseParams.{s}"));

			// Validate ChildIds
			ChildIds.ForEach(id =>
			{
				if (!soundbank.HircItems.Any(hi => hi.UlID == id))
				{
					knownValidationErrors.Add($"CAkRanSeqCntr '{UlID}' has a ChildId that is '{id}', but no HIRC item in the soundbank has that ID.");
				}
				if (!CAkPlayList.PlaylistItems.Any(pi => pi.PlayId == id))
				{
					knownValidationErrors.Add($"CAkRanSeqCntr '{UlID}' has a ChildId that is '{id}', but no playlist item in the CAkRanSeqCntr's CAkPlayList has that ID.");
				}
			});

			// Validate CAkPlayList
			if (CAkPlayList.PlaylistItems.Count == 0)
			{
				knownValidationErrors.Add($"CAkRanSeqCntr's '{UlID}' CAkPlayList has no playlist items.");
			}
			CAkPlayList.PlaylistItems.ForEach(pi =>
			{
				if (!soundbank.HircItems.Any(hi => hi.UlID == pi.PlayId))
				{
					knownValidationErrors.Add($"CAkRanSeqCntr's '{UlID}' CAkPlayList has a playlist item with ID '{pi.PlayId}', but no HIRC item in the soundbank has that ID.");
				}
				if (!ChildIds.Any(id => id == pi.PlayId))
				{
					knownValidationErrors.Add($"CAkRanSeqCntr's '{UlID}' CAkPlayList has a playlist item with ID '{pi.PlayId}', but no child in the CAkRanSeqCntr has that ID.");
				}
			});

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
			binaryWriter.Write(LoopCount);
			binaryWriter.Write(LoopModMin);
			binaryWriter.Write(LoopModMax);
			binaryWriter.Write(TransitionTime);
			binaryWriter.Write(TransitionTimeModMin);
			binaryWriter.Write(TransitionTimeModMax);
			binaryWriter.Write(AvoidRepeatCount);
			binaryWriter.Write(TransitionMode);
			binaryWriter.Write(RandomMode);
			binaryWriter.Write(Mode);
			binaryWriter.Write(ByBitVector);
			binaryWriter.Write((uint)ChildIds.Count);
			for (int i = 0; i < ChildIds.Count; i++)
			{
				binaryWriter.Write(ChildIds[i]);
			}
			CAkPlayList.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkRanSeqCntr '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class CAkPlayList : WwiseObject
	{
		public List<AkPlaylistItem> PlaylistItems { get; set; } = new List<AkPlaylistItem>();

		public CAkPlayList() { }

		public CAkPlayList(BinaryReader binaryReader)
		{
			ushort playListItemCount = binaryReader.ReadUInt16();
			for (int i = 0; i < playListItemCount; i++)
			{
				PlaylistItems.Add(new AkPlaylistItem(binaryReader));
			}
		}

		public uint ComputeTotalSize()
		{
			return 2 + (uint)PlaylistItems.Sum(p => p.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((ushort)PlaylistItems.Count);
			for (int i = 0; i < PlaylistItems.Count; i++)
			{
				PlaylistItems[i].WriteToBinary(binaryWriter);
			}
		}
	}

	public class AkPlaylistItem : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint PlayId { get; set; }
		public int Weight { get; set; }

		public AkPlaylistItem() { }

		public AkPlaylistItem(BinaryReader binaryReader)
		{
			PlayId = binaryReader.ReadUInt32();
			Weight = binaryReader.ReadInt32();
		}

		public uint ComputeTotalSize()
		{
			return 8;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(PlayId);
			binaryWriter.Write(Weight);
		}
	}
}
