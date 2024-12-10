using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditorCore.WwiseObjects.HircItems.Common;
using System.Data;

namespace SoundbankEditorCore.WwiseObjects.HircItems
{
	public class CAkMusicRanSeqCntr : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.MusicRandomSequenceContainer;

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
		public MusicNodeParams MusicNodeParams { get; set; } = new MusicNodeParams();
		public List<AkMusicTransitionRule> Rules { get; set; } = new List<AkMusicTransitionRule>();
		public List<AkMusicRanSeqPlaylistItem> PlaylistItems { get; set; } = new List<AkMusicRanSeqPlaylistItem>();

		public CAkMusicRanSeqCntr()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkMusicRanSeqCntr(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			MusicNodeParams = new MusicNodeParams(binaryReader);
			uint ruleCount = binaryReader.ReadUInt32();
			for (int i = 0; i < ruleCount; i++)
			{
				Rules.Add(new AkMusicTransitionRule(binaryReader));
			}
			uint playlistItemCount = binaryReader.ReadUInt32(); // This seems to contain the count of all nested playlist items.
			//for (int i = 0; i < playlistItemCount; i++)
			//{
			//	PlaylistItems.Add(new AkMusicRanSeqPlaylistItem(binaryReader));
			//}
			PlaylistItems.Add(new AkMusicRanSeqPlaylistItem(binaryReader)); // This doesn't seem to be a list, but a tree.

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkMusicRanSeqCntr '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 17 +
				MusicNodeParams.ComputeTotalSize() +
				(uint)Rules.Sum(mm => mm.ComputeTotalSize()) +
				(uint)PlaylistItems.Sum(mm => mm.ComputeTotalSize());
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkMusicRanSeqCntr '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate MusicNodeParams
			knownValidationErrors.AddRange(MusicNodeParams.GetKnownValidationErrors(soundbank).Select(s => $"CAkMusicRanSeqCntr's '{UlID}' {s}"));

			return knownValidationErrors;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			uint expectedSize = ComputeTotalSize() - 5;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.Write(UlID);

			MusicNodeParams.WriteToBinary(binaryWriter);
			binaryWriter.Write((uint)Rules.Count);
			for (int i = 0; i < Rules.Count; i++)
			{
				Rules[i].WriteToBinary(binaryWriter);
			}
			binaryWriter.Write((uint)PlaylistItems[0].ComputeTreeCount());
			PlaylistItems[0].WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkMusicRanSeqCntr '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class AkMusicTransitionRule : WwiseObject
	{
		public List<uint> SourceIds { get; set; } = new List<uint>();
		public List<uint> DstIds { get; set; } = new List<uint>();
		public AkMusicTransSrcRule AkMusicTransSrcRule { get; set; } = new AkMusicTransSrcRule();
		public AkMusicTransDstRule AkMusicTransDstRule { get; set; } = new AkMusicTransDstRule();
		public byte AllocTransObjectFlag { get; set; }

		public AkMusicTransitionRule() { }

		public AkMusicTransitionRule(BinaryReader binaryReader)
		{
			uint sourceCount = binaryReader.ReadUInt32();
			for (int i = 0; i < sourceCount; i++)
			{
				SourceIds.Add(binaryReader.ReadUInt32());
			}
			uint dstCount = binaryReader.ReadUInt32();
			for (int i = 0; i < sourceCount; i++)
			{
				DstIds.Add(binaryReader.ReadUInt32());
			}
			AkMusicTransSrcRule = new AkMusicTransSrcRule(binaryReader);
			AkMusicTransDstRule = new AkMusicTransDstRule(binaryReader);
			AllocTransObjectFlag = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 9 +
				(uint)(SourceIds.Count * 4) +
				(uint)(DstIds.Count * 4) +
				AkMusicTransSrcRule.ComputeTotalSize() +
				AkMusicTransDstRule.ComputeTotalSize();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((uint)SourceIds.Count);
			for (int i = 0; i < SourceIds.Count; i++)
			{
				binaryWriter.Write(SourceIds[i]);
			}
			binaryWriter.Write((uint)DstIds.Count);
			for (int i = 0; i < DstIds.Count; i++)
			{
				binaryWriter.Write(DstIds[i]);
			}
			AkMusicTransSrcRule.WriteToBinary(binaryWriter);
			AkMusicTransDstRule.WriteToBinary(binaryWriter);
			binaryWriter.Write(AllocTransObjectFlag);
		}
	}

	public class AkMusicTransSrcRule : WwiseObject
	{
		public float TransitionTime { get; set; }
		public uint FadeCurve { get; set; }
		public float FadeOffset { get; set; }
		public uint SyncType { get; set; }
		public uint CueFilterHash { get; set; }
		public byte PlayPostExit { get; set; }

		public AkMusicTransSrcRule() { }

		public AkMusicTransSrcRule(BinaryReader binaryReader)
		{
			TransitionTime = binaryReader.ReadSingle();
			FadeCurve = binaryReader.ReadUInt32();
			FadeOffset = binaryReader.ReadSingle();
			SyncType = binaryReader.ReadUInt32();
			CueFilterHash = binaryReader.ReadUInt32();
			PlayPostExit = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 21;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(TransitionTime);
			binaryWriter.Write(FadeCurve);
			binaryWriter.Write(FadeOffset);
			binaryWriter.Write(SyncType);
			binaryWriter.Write(CueFilterHash);
			binaryWriter.Write(PlayPostExit);
		}
	}

	public class AkMusicTransDstRule : WwiseObject
	{
		public float TransitionTime { get; set; }
		public uint FadeCurve { get; set; }
		public float FadeOffset { get; set; }
		public uint CueFilterHash { get; set; }
		public uint JumpToId { get; set; }
		public ushort EntryType { get; set; }
		public byte PlayPreEntry { get; set; }
		public byte DestMatchSourceCueName { get; set; }

		public AkMusicTransDstRule() { }

		public AkMusicTransDstRule(BinaryReader binaryReader)
		{
			TransitionTime = binaryReader.ReadSingle();
			FadeCurve = binaryReader.ReadUInt32();
			FadeOffset = binaryReader.ReadSingle();
			CueFilterHash = binaryReader.ReadUInt32();
			JumpToId = binaryReader.ReadUInt32();
			EntryType = binaryReader.ReadUInt16();
			PlayPreEntry = binaryReader.ReadByte();
			DestMatchSourceCueName = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 24;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(TransitionTime);
			binaryWriter.Write(FadeCurve);
			binaryWriter.Write(FadeOffset);
			binaryWriter.Write(CueFilterHash);
			binaryWriter.Write(JumpToId);
			binaryWriter.Write(EntryType);
			binaryWriter.Write(PlayPreEntry);
			binaryWriter.Write(DestMatchSourceCueName);
		}
	}

	public class AkMusicRanSeqPlaylistItem : WwiseObject
	{
		public uint SegmentId { get; set; }
		public int PlaylistItemId { get; set; }
		public uint RsType { get; set; }
		public short Loop { get; set; }
		public short LoopMin { get; set; }
		public short LoopMax { get; set; }
		public uint Weight { get; set; }
		public ushort AvoidRepeatCount { get; set; }
		public byte IsUsingWeight { get; set; }
		public byte IsShuffle { get; set; }
		public List<AkMusicRanSeqPlaylistItem> PlaylistItems { get; set; } = new List<AkMusicRanSeqPlaylistItem>();

		public AkMusicRanSeqPlaylistItem() { }

		public AkMusicRanSeqPlaylistItem(BinaryReader binaryReader)
		{
			SegmentId = binaryReader.ReadUInt32();
			PlaylistItemId = binaryReader.ReadInt32();
			uint childrenCount = binaryReader.ReadUInt32();
			RsType = binaryReader.ReadUInt32();
			Loop = binaryReader.ReadInt16();
			LoopMin = binaryReader.ReadInt16();
			LoopMax = binaryReader.ReadInt16();
			Weight = binaryReader.ReadUInt32();
			AvoidRepeatCount = binaryReader.ReadUInt16();
			IsUsingWeight = binaryReader.ReadByte();
			IsShuffle = binaryReader.ReadByte();
			for (int i = 0; i < childrenCount; i++)
			{
				PlaylistItems.Add(new AkMusicRanSeqPlaylistItem(binaryReader));
			}
		}

		public uint ComputeTotalSize()
		{
			return 30 + (uint)PlaylistItems.Sum(pi => pi.ComputeTotalSize());
		}

		public int ComputeTreeCount()
		{
			return 1 + PlaylistItems.Sum(mm => mm.ComputeTreeCount());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(SegmentId);
			binaryWriter.Write(PlaylistItemId);
			binaryWriter.Write((uint)PlaylistItems.Count);
			binaryWriter.Write(RsType);
			binaryWriter.Write(Loop);
			binaryWriter.Write(LoopMin);
			binaryWriter.Write(LoopMax);
			binaryWriter.Write(Weight);
			binaryWriter.Write(AvoidRepeatCount);
			binaryWriter.Write(IsUsingWeight);
			binaryWriter.Write(IsShuffle);
			for (int i = 0; i < PlaylistItems.Count; i++)
			{
				PlaylistItems[i].WriteToBinary(binaryWriter);
			}
		}
	}
}
