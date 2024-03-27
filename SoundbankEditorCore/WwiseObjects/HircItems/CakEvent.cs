using SoundbankEditor.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CAkEvent : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.Event;

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
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> ActionIds { get; set; } = new List<uint>();

		public CAkEvent()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkEvent(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			uint actionCount = binaryReader.ReadUInt32();
			for (int i = 0; i < actionCount; i++)
			{
				ActionIds.Add(binaryReader.ReadUInt32());
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkEvent '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 13 + (uint)(ActionIds.Count * 4);
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkEvent '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate 
			if (ActionIds.Count == 0)
			{
				knownValidationErrors.Add($"CAkEvent '{UlID}' has no action IDs.");
			}
			ActionIds.ForEach(actionId =>
			{
				if (!soundbank.HircItems.Any(hi => hi.UlID == actionId))
				{
					knownValidationErrors.Add($"CAkEvent '{UlID}' has an ActionId that is '{actionId}', but no HIRC item in the soundbank has that ID.");
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

			binaryWriter.Write((uint)ActionIds.Count);
			for (int i = 0; i < ActionIds.Count; i++)
			{
				binaryWriter.Write(ActionIds[i]);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkEvent '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
