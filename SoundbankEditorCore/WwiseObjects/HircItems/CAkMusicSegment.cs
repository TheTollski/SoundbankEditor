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

namespace SoundbankEditorCore.WwiseObjects.HircItems
{
	public class CAkMusicSegment : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.Music_Segment;

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
		public double Duration { get; set; }
		public List<AkMusicMarker> AkMusicMarkers { get; set; } = new List<AkMusicMarker>();

		public CAkMusicSegment()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkMusicSegment(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			MusicNodeParams = new MusicNodeParams(binaryReader);
			Duration = binaryReader.ReadDouble();
			uint musicMarkerCount = binaryReader.ReadUInt32();
			for (int i = 0; i < musicMarkerCount; i++)
			{
				AkMusicMarkers.Add(new AkMusicMarker(binaryReader));
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkMusicSegment '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 21 + MusicNodeParams.ComputeTotalSize() + (uint)AkMusicMarkers.Sum(mm => mm.ComputeTotalSize());
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkMusicSegment '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate NodeBaseParams
			knownValidationErrors.AddRange(MusicNodeParams.GetKnownValidationErrors(soundbank).Select(s => $"CAkMusicSegment's '{UlID}' {s}"));

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
			binaryWriter.Write(Duration);
			binaryWriter.Write((uint)AkMusicMarkers.Count);
			for (int i = 0; i < AkMusicMarkers.Count; i++)
			{
				AkMusicMarkers[i].WriteToBinary(binaryWriter);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkMusicSegment '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

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

	public class AkMusicMarker : WwiseObject
	{
		public uint Id { get; set; }
		public double Position { get; set; }
		public string? Name { get; set; }

		public AkMusicMarker() { }

		public AkMusicMarker(BinaryReader binaryReader)
		{
			Id = binaryReader.ReadUInt32();
			Position = binaryReader.ReadDouble();
			uint stringSize = binaryReader.ReadUInt32();
			if (stringSize > 0)
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				byte[] nameBytes = binaryReader.ReadBytes((int)stringSize);
				Name = Encoding.GetEncoding(1252).GetString(nameBytes);
			}
		}

		public uint ComputeTotalSize()
		{
			return 16 + (uint)(Name?.Length ?? 0);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(Id);
			binaryWriter.Write(Position);
			if (Name != null)
			{
				binaryWriter.Write(Name.Length);

				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				byte[] nameBytes = Encoding.GetEncoding(1252).GetBytes(Name);
				binaryWriter.Write(nameBytes);
			}
			else
			{
				binaryWriter.Write((uint)0);
			}
		}
	}
}
