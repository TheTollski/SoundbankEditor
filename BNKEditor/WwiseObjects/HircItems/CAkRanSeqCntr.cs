using BNKEditor.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CAkRanSeqCntr : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
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
		public uint ChildCount { get; set; }
		public List<uint> ChildIds { get; set; } = new List<uint>();
		public CAkPlayList CAkPlayList { get; set; } = new CAkPlayList();

		public CAkRanSeqCntr() { }

		public CAkRanSeqCntr(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

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
			ChildCount = binaryReader.ReadUInt32();
			for (int i = 0; i < ChildCount; i++)
			{
				ChildIds.Add(binaryReader.ReadUInt32());
			}
			CAkPlayList = new CAkPlayList(binaryReader);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CAkRanSeqCntr '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);
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
			binaryWriter.Write(ChildCount);
			for (int i = 0; i < ChildIds.Count; i++)
			{
				binaryWriter.Write(ChildIds[i]);
			}
			CAkPlayList.WriteToBinary(binaryWriter);
		}
	}

	public class CAkPlayList : WwiseObject
	{
		public ushort PlayListItemCount { get; set; }
		public List<AkPlaylistItem> PlaylistItems { get; set; } = new List<AkPlaylistItem>();

		public CAkPlayList() { }

		public CAkPlayList(BinaryReader binaryReader)
		{
			PlayListItemCount = binaryReader.ReadUInt16();
			for (int i = 0; i < PlayListItemCount; i++)
			{
				PlaylistItems.Add(new AkPlaylistItem(binaryReader));
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(PlayListItemCount);
			for (int i = 0; i < PlaylistItems.Count; i++)
			{
				PlaylistItems[i].WriteToBinary(binaryWriter);
			}
		}
	}

	public class AkPlaylistItem : WwiseObject
	{
		public uint PlayId { get; set; }
		public int Weight { get; set; }

		public AkPlaylistItem() { }

		public AkPlaylistItem(BinaryReader binaryReader)
		{
			PlayId = binaryReader.ReadUInt32();
			Weight = binaryReader.ReadInt32();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(PlayId);
			binaryWriter.Write(Weight);
		}
	}
}
