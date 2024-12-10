using SoundbankEditor.Core.WwiseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.TimeZoneInfo;

namespace SoundbankEditorCore.WwiseObjects.HircItems.Common
{
	public class AkMusicTransitionRule : WwiseObject
	{
		public List<uint> SourceIds { get; set; } = new List<uint>();
		public List<uint> DstIds { get; set; } = new List<uint>();
		public AkMusicTransSrcRule AkMusicTransSrcRule { get; set; } = new AkMusicTransSrcRule();
		public AkMusicTransDstRule AkMusicTransDstRule { get; set; } = new AkMusicTransDstRule();
		public byte AllocTransObjectFlag { get; set; }
		public AkMusicTransitionObject? AkMusicTransitionObject { get; set; }

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
			if (AllocTransObjectFlag == 1)
			{
				AkMusicTransitionObject = new AkMusicTransitionObject(binaryReader);
			}
		}

		public uint ComputeTotalSize()
		{
			return 9 +
				(uint)(SourceIds.Count * 4) +
				(uint)(DstIds.Count * 4) +
				AkMusicTransSrcRule.ComputeTotalSize() +
				AkMusicTransDstRule.ComputeTotalSize() +
				(AkMusicTransitionObject != null ? AkMusicTransitionObject.ComputeTotalSize() : 0);
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
			AkMusicTransitionObject?.WriteToBinary(binaryWriter);
		}
	}

	public class AkMusicTransSrcRule : WwiseObject
	{
		public int TransitionTime { get; set; }
		public uint FadeCurve { get; set; }
		public int FadeOffset { get; set; }
		public uint SyncType { get; set; }
		public uint CueFilterHash { get; set; }
		public byte PlayPostExit { get; set; }

		public AkMusicTransSrcRule() { }

		public AkMusicTransSrcRule(BinaryReader binaryReader)
		{
			TransitionTime = binaryReader.ReadInt32();
			FadeCurve = binaryReader.ReadUInt32();
			FadeOffset = binaryReader.ReadInt32();
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
		public int TransitionTime { get; set; }
		public uint FadeCurve { get; set; }
		public int FadeOffset { get; set; }
		public uint CueFilterHash { get; set; }
		public uint JumpToId { get; set; }
		public ushort EntryType { get; set; }
		public byte PlayPreEntry { get; set; }
		public byte DestMatchSourceCueName { get; set; }

		public AkMusicTransDstRule() { }

		public AkMusicTransDstRule(BinaryReader binaryReader)
		{
			TransitionTime = binaryReader.ReadInt32();
			FadeCurve = binaryReader.ReadUInt32();
			FadeOffset = binaryReader.ReadInt32();
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

	public class AkMusicTransitionObject : WwiseObject
	{
		public uint SegmentId { get; set; }
		public FadeParams FadeInParams { get; set; } = new FadeParams();
		public FadeParams FadeOutParams { get; set; } = new FadeParams();
		public byte PlayPreEntry { get; set; }
		public byte PlayPostExit { get; set; }

		public AkMusicTransitionObject() { }

		public AkMusicTransitionObject(BinaryReader binaryReader)
		{
			SegmentId = binaryReader.ReadUInt32();
			FadeInParams = new FadeParams(binaryReader);
			FadeOutParams = new FadeParams(binaryReader);
			PlayPreEntry = binaryReader.ReadByte();
			PlayPostExit = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 6 + FadeInParams.ComputeTotalSize() + FadeOutParams.ComputeTotalSize();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(SegmentId);
			FadeInParams.WriteToBinary(binaryWriter);
			FadeOutParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(PlayPreEntry);
			binaryWriter.Write(PlayPostExit);
		}
	}

	public class FadeParams : WwiseObject
	{
		public int TransitionTime { get; set; }
		public uint FadeCurve { get; set; }
		public int FadeOffset { get; set; }

		public FadeParams() { }

		public FadeParams(BinaryReader binaryReader)
		{
			TransitionTime = binaryReader.ReadInt32();
			FadeCurve = binaryReader.ReadUInt32();
			FadeOffset = binaryReader.ReadInt32();
		}

		public uint ComputeTotalSize()
		{
			return 12;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(TransitionTime);
			binaryWriter.Write(FadeCurve);
			binaryWriter.Write(FadeOffset);
		}
	}
}
