﻿using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using SoundbankEditorCore.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CakDialogueEvent : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.Dialogue_Event;

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
		public byte Probability { get; set; }
		public uint TreeDepth { get; set; }
		public ArgumentList Arguments { get; set; } = new ArgumentList();
		public byte Mode { get; set; }
		public AkDecisionTree AkDecisionTree { get; set; }

		public CakDialogueEvent()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CakDialogueEvent(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			Probability = binaryReader.ReadByte();
			TreeDepth = binaryReader.ReadUInt32();
			Arguments = new ArgumentList(binaryReader, TreeDepth);
			uint treeDataSize = binaryReader.ReadUInt32();
			Mode = binaryReader.ReadByte();
			AkDecisionTree = new AkDecisionTree(binaryReader, TreeDepth, treeDataSize);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CakDialogueEvent '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 19 + Arguments.ComputeTotalSize() + AkDecisionTree.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CakDialogueEvent '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate 
			//if (ActionIds.Count == 0)
			//{
			//	knownValidationErrors.Add($"CAkEvent '{UlID}' has no action IDs.");
			//}
			//ActionIds.ForEach(actionId =>
			//{
			//	if (!soundbank.HircItems.Any(hi => hi.UlID == actionId))
			//	{
			//		knownValidationErrors.Add($"CAkEvent '{UlID}' has an ActionId that is '{actionId}', but no HIRC item in the soundbank has that ID.");
			//	}
			//});

			return knownValidationErrors;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			uint expectedSize = ComputeTotalSize() - 5;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.Write(UlID);

			binaryWriter.Write(Probability);
			binaryWriter.Write(TreeDepth);
			Arguments.WriteToBinary(binaryWriter);
			binaryWriter.Write(AkDecisionTree.ComputeTotalSize());
			binaryWriter.Write(Mode);
			AkDecisionTree.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CakDialogueEvent '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
