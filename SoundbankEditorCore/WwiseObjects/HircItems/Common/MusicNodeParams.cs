using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundbankEditorCore.WwiseObjects.HircItems.Common
{
	public class MusicNodeParams : WwiseObject
	{
		public byte Flags { get; set; }
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();
		public List<uint> ChildIds { get; set; } = new List<uint>();
		public AkMeterInfo AkMeterInfo { get; set; } = new AkMeterInfo();
		public byte MeterInfoFlag { get; set; }
		public List<object> Stingers { get; set; } = new List<object>();

		public MusicNodeParams() { }

		public MusicNodeParams(BinaryReader binaryReader)
		{
			Flags = binaryReader.ReadByte();
			NodeBaseParams = new NodeBaseParams(binaryReader);
			uint childCount = binaryReader.ReadUInt32();
			for (int i = 0; i < childCount; i++)
			{
				ChildIds.Add(binaryReader.ReadUInt32());
			}
			AkMeterInfo = new AkMeterInfo(binaryReader);
			MeterInfoFlag = binaryReader.ReadByte();
			uint stingersCount = binaryReader.ReadUInt32();
			if (stingersCount > 0)
			{
				throw new Exception("MusicNodeParams.Stingers is not supported.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 10 + NodeBaseParams.ComputeTotalSize() + (uint)(ChildIds.Count * 4) + AkMeterInfo.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate NodeBaseParams
			knownValidationErrors.AddRange(NodeBaseParams.GetKnownValidationErrors(soundbank).Select(s => $"MusicNodeParams's NodeBaseParams {s}"));

			// Validate ChildIds
			ChildIds.ForEach(id =>
			{
				if (!soundbank.HircItems.Any(hi => hi.UlID == id))
				{
					knownValidationErrors.Add($"has a ChildId that is '{id}', but no HIRC item in the soundbank has that ID.");
				}
				//if (!CAkPlayList.PlaylistItems.Any(pi => pi.PlayId == id))
				//{
				//	knownValidationErrors.Add($"has a ChildId that is '{id}', but no playlist item in the CAkRanSeqCntr's CAkPlayList has that ID.");
				//}
			});

			return knownValidationErrors;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(Flags);
			NodeBaseParams.WriteToBinary(binaryWriter);
			binaryWriter.Write((uint)ChildIds.Count);
			for (int i = 0; i < ChildIds.Count; i++)
			{
				binaryWriter.Write(ChildIds[i]);
			}
			AkMeterInfo.WriteToBinary(binaryWriter);
			binaryWriter.Write(MeterInfoFlag);
			binaryWriter.Write((uint)Stingers.Count);
		}
	}

	public class AkMeterInfo : WwiseObject
	{
		public double GridPeriod { get; set; }
		public double GridOffset { get; set; }
		public float Tempo { get; set; }
		public byte TimeSigNumBeatsBar { get; set; }
		public byte TimeSigBeatValue { get; set; }

		public AkMeterInfo() { }

		public AkMeterInfo(BinaryReader binaryReader)
		{
			GridPeriod = binaryReader.ReadDouble();
			GridOffset = binaryReader.ReadDouble();
			Tempo = binaryReader.ReadSingle();
			TimeSigNumBeatsBar = binaryReader.ReadByte();
			TimeSigBeatValue = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 22;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(GridPeriod);
			binaryWriter.Write(GridOffset);
			binaryWriter.Write(Tempo);
			binaryWriter.Write(TimeSigNumBeatsBar);
			binaryWriter.Write(TimeSigBeatValue);
		}
	}
}
