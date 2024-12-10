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
	public class CAkMusicSwitchCntr : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.MusicSwitchContainer;

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
		public byte IsContinuePlayback { get; set; }
		public uint TreeDepth { get; set; }
		public ArgumentList Arguments { get; set; } = new ArgumentList();
		public byte Mode { get; set; }
		public AkDecisionTree AkDecisionTree { get; set; } = new AkDecisionTree();

		public CAkMusicSwitchCntr()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkMusicSwitchCntr(BinaryReader binaryReader)
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
			IsContinuePlayback = binaryReader.ReadByte();
			TreeDepth = binaryReader.ReadUInt32();
			Arguments = new ArgumentList(binaryReader, TreeDepth);
			uint treeDataSize = binaryReader.ReadUInt32();
			Mode = binaryReader.ReadByte();
			AkDecisionTree = new AkDecisionTree(binaryReader, TreeDepth, treeDataSize);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkMusicSwitchCntr '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 23 +
				MusicNodeParams.ComputeTotalSize() +
				(uint)Rules.Sum(mm => mm.ComputeTotalSize()) +
				Arguments.ComputeTotalSize() +
				AkDecisionTree.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkMusicSwitchCntr '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate MusicNodeParams
			knownValidationErrors.AddRange(MusicNodeParams.GetKnownValidationErrors(soundbank).Select(s => $"CAkMusicSwitchCntr's '{UlID}' {s}"));

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
			binaryWriter.Write(IsContinuePlayback);
			binaryWriter.Write(TreeDepth);
			Arguments.WriteToBinary(binaryWriter);
			binaryWriter.Write(AkDecisionTree.ComputeTotalSize());
			binaryWriter.Write(Mode);
			AkDecisionTree.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkMusicSwitchCntr '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
