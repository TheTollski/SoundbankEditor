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

			// Validate MusicNodeParams
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
