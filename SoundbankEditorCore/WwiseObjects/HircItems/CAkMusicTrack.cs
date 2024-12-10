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
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using SoundbankEditor.Core.WwiseObjects;
using SoundbankEditorCore.WwiseObjects.HircItems.Common;

namespace SoundbankEditorCore.WwiseObjects.HircItems
{
	public class CAkMusicTrack : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.MusicTrack;

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
		public byte Overrides { get; set; }
		public List<AkBankSourceData> Sources { get; set; } = new List<AkBankSourceData>();
		public List<AkTrackSrcInfo> TrackSourceInfos { get; set; } = new List<AkTrackSrcInfo>();
		public uint NumSubTrack {  get; set; }
		public List<AkClipAutomation> ClipAutomations { get; set; } = new List<AkClipAutomation>();
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();
		public byte TrackType { get; set; }
		public int LookAheadTime { get; set; }

		public CAkMusicTrack()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkMusicTrack(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			Overrides = binaryReader.ReadByte();
			uint sourceCount = binaryReader.ReadUInt32();
			for (int i = 0; i < sourceCount; i++)
			{
				Sources.Add(new AkBankSourceData(binaryReader));
			}
			uint trackSrcInfoCount = binaryReader.ReadUInt32();
			for (int i = 0; i < trackSrcInfoCount; i++)
			{
				TrackSourceInfos.Add(new AkTrackSrcInfo(binaryReader));
			}
			NumSubTrack = binaryReader.ReadUInt32();
			uint clipAutomationItemCount = binaryReader.ReadUInt32();
			if (clipAutomationItemCount > 0)
			{
				ClipAutomations.Add(new AkClipAutomation(binaryReader));
			}
			NodeBaseParams = new NodeBaseParams(binaryReader);
			TrackType = binaryReader.ReadByte();
			LookAheadTime = binaryReader.ReadInt32();

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkMusicTrack '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 31 +
				(uint)Sources.Sum(s => s.ComputeTotalSize()) +
				(uint)TrackSourceInfos.Sum(tsi => tsi.ComputeTotalSize()) +
				(uint)ClipAutomations.Sum(ca => ca.ComputeTotalSize()) +
				NodeBaseParams.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkMusicTrack '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate NodeBaseParams
			knownValidationErrors.AddRange(NodeBaseParams.GetKnownValidationErrors(soundbank).Select(s => $"CAkMusicSegment's '{UlID}' {s}"));

			return knownValidationErrors;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			uint expectedSize = ComputeTotalSize() - 5;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.Write(UlID);

			binaryWriter.Write(Overrides);
			binaryWriter.Write((uint)Sources.Count);
			for (int i = 0; i < Sources.Count; i++)
			{
				Sources[i].WriteToBinary(binaryWriter);
			}
			binaryWriter.Write((uint)TrackSourceInfos.Count);
			for (int i = 0; i < TrackSourceInfos.Count; i++)
			{
				TrackSourceInfos[i].WriteToBinary(binaryWriter);
			}
			binaryWriter.Write(NumSubTrack);
			binaryWriter.Write((uint)ClipAutomations.Count);
			for (int i = 0; i < ClipAutomations.Count; i++)
			{
				ClipAutomations[i].WriteToBinary(binaryWriter);
			}
			NodeBaseParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(TrackType);
			binaryWriter.Write(LookAheadTime);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkMusicTrack '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class AkTrackSrcInfo : WwiseObject
	{
		public uint TrackId { get; set; }
		public uint SourceId { get; set; }
		public double PlayAt { get; set; }
		public double BeginTrimOffset { get; set; }
		public double EndTrimOffset { get; set; }
		public double SrcDuration { get; set; }

		public AkTrackSrcInfo() { }

		public AkTrackSrcInfo(BinaryReader binaryReader)
		{
			TrackId = binaryReader.ReadUInt32();
			SourceId = binaryReader.ReadUInt32();
			PlayAt = binaryReader.ReadDouble();
			BeginTrimOffset = binaryReader.ReadDouble();
			EndTrimOffset = binaryReader.ReadDouble();
			SrcDuration = binaryReader.ReadDouble();
		}

		public uint ComputeTotalSize()
		{
			return 40;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(TrackId);
			binaryWriter.Write(SourceId);
			binaryWriter.Write(PlayAt);
			binaryWriter.Write(BeginTrimOffset);
			binaryWriter.Write(EndTrimOffset);
			binaryWriter.Write(SrcDuration);
		}
	}

	public class AkClipAutomation : WwiseObject
	{
		public uint ClipIndex { get; set; }
		public uint AutoType { get; set; }
		public List<AkRTPCGraphPoint> GraphPoints { get; set; } = new List<AkRTPCGraphPoint>();

		public AkClipAutomation() { }

		public AkClipAutomation(BinaryReader binaryReader)
		{
			ClipIndex = binaryReader.ReadUInt32();
			AutoType = binaryReader.ReadUInt32();
			uint graphPointCount = binaryReader.ReadUInt32();
			for (int i = 0; i < graphPointCount; i++)
			{
				GraphPoints.Add(new AkRTPCGraphPoint(binaryReader));
			}
		}

		public uint ComputeTotalSize()
		{
			return 12 + (uint)GraphPoints.Sum(gp => gp.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ClipIndex);
			binaryWriter.Write(AutoType);
			binaryWriter.Write((uint)GraphPoints.Count);
			for (int i = 0; i < GraphPoints.Count; i++)
			{
				GraphPoints[i].WriteToBinary(binaryWriter);
			}
		}
	}
}
