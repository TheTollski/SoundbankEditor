using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CAkSwitchCntr : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.SwitchContainer;

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
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();
		public byte GroupType { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint GroupId { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint DefaultSwitch { get; set; }
		public byte IsContinuousValidation { get; set; }
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> ChildIds { get; set; } = new List<uint>();
		public List<CAkSwitchPackage> SwitchPackages { get; set; } = new List<CAkSwitchPackage>();
		public List<AkSwitchNodeParams> SwitchParams { get; set; } = new List<AkSwitchNodeParams>();

		public CAkSwitchCntr()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkSwitchCntr(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			NodeBaseParams = new NodeBaseParams(binaryReader);
			GroupType = binaryReader.ReadByte();
			GroupId = binaryReader.ReadUInt32();
			DefaultSwitch = binaryReader.ReadUInt32();
			IsContinuousValidation = binaryReader.ReadByte();
			uint childCount = binaryReader.ReadUInt32();
			for (int i = 0; i < childCount; i++)
			{
				ChildIds.Add(binaryReader.ReadUInt32());
			}
			uint switchPackageCount = binaryReader.ReadUInt32();
			for (int i = 0; i < switchPackageCount; i++)
			{
				SwitchPackages.Add(new CAkSwitchPackage(binaryReader));
			}
			uint switchParamsCount = binaryReader.ReadUInt32();
			for (int i = 0; i < switchParamsCount; i++)
			{
				SwitchParams.Add(new AkSwitchNodeParams(binaryReader));
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkSwitchCntr '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 31
				+ NodeBaseParams.ComputeTotalSize()
				+ (uint)(ChildIds.Count * 4)
				+ (uint)SwitchPackages.Sum(s => s.ComputeTotalSize())
				+ (uint)SwitchParams.Sum(s=> s.ComputeTotalSize());
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate NodeBaseParams
			knownValidationErrors.AddRange(NodeBaseParams.GetKnownValidationErrors(soundbank).Select(s => $"CAkSwitchCntr's '{UlID}' NodeBaseParams.{s}"));

			// Validate DefaultSwitch
			if (!SwitchPackages.Any(sp => sp.SwitchId == DefaultSwitch))
			{
				knownValidationErrors.Add($"CAkSwitchCntr's '{UlID}' DefaultSwitch is '{NodeBaseParams.DirectParentID}', but no switch package in the CAkSwitchCntr has that ID.");
			}

			// Validate ChildIds
			ChildIds.ForEach(id =>
			{
				if (!soundbank.HircItems.Any(hi => hi.UlID == id))
				{
					knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a ChildId that is '{id}', but no HIRC item in the soundbank has that ID.");
				}
				if (!SwitchPackages.Any(sp => sp.NodeIds.Contains(id)))
				{
					knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a ChildId that is '{id}', but no switch package in the CAkSwitchCntr has a node with that ID.");
				}
			});

			// Validate SwitchPackages
			if (SwitchPackages.Count == 0)
			{
				knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has no switch packages.");
			}
			SwitchPackages.ForEach(spackage =>
			{
				if (spackage.NodeIds.Count == 0)
				{
					knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a switch package '{spackage.SwitchId}' that has no node IDs.");
				}
				spackage.NodeIds.ForEach(nodeId =>
				{
					if (!soundbank.HircItems.Any(hi => hi.UlID == nodeId))
					{
						knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a switch package '{spackage.SwitchId}' that has a NodeId that is '{nodeId}', but no HIRC item in the soundbank has that ID.");
					}
					if (!ChildIds.Any(id => id == nodeId))
					{
						knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a switch package '{spackage.SwitchId}' that has a NodeId that is '{nodeId}', but no child in the CAkSwitchCntr has that ID.");
					}
					if (!SwitchParams.Any(sparam => sparam.NodeId == nodeId))
					{
						knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a switch package '{spackage.SwitchId}' that has a NodeId that is '{nodeId}', but no switch param in the CAkSwitchCntr has that node ID.");
					}
				});
			});

			// Validate SwitchParams
			SwitchParams.ForEach(sparam =>
			{
				if (!soundbank.HircItems.Any(hi => hi.UlID == sparam.NodeId))
				{
					knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a switch param that has NodeId '{sparam.NodeId}', but no HIRC item in the soundbank has that ID.");
				}
				if (!SwitchPackages.Any(sp => sp.NodeIds.Contains(sparam.NodeId)))
				{
					knownValidationErrors.Add($"CAkSwitchCntr '{UlID}' has a switch param that has NodeId '{sparam.NodeId}', but no switch package in the CAkSwitchCntr has a node with that ID.");
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

			NodeBaseParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(GroupType);
			binaryWriter.Write(GroupId);
			binaryWriter.Write(DefaultSwitch);
			binaryWriter.Write(IsContinuousValidation);
			binaryWriter.Write((uint)ChildIds.Count);
			for (int i = 0; i < ChildIds.Count; i++)
			{
				binaryWriter.Write(ChildIds[i]);
			}
			binaryWriter.Write((uint)SwitchPackages.Count);
			for (int i = 0; i < SwitchPackages.Count; i++)
			{
				SwitchPackages[i].WriteToBinary(binaryWriter);
			}
			binaryWriter.Write((uint)SwitchParams.Count);
			for (int i = 0; i < SwitchParams.Count; i++)
			{
				SwitchParams[i].WriteToBinary(binaryWriter);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkSwitchCntr '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class CAkSwitchPackage : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint SwitchId { get; set; }
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> NodeIds { get; set; } = new List<uint>();

		public CAkSwitchPackage() { }

		public CAkSwitchPackage(BinaryReader binaryReader)
		{
			SwitchId = binaryReader.ReadUInt32();
			uint nodeCount = binaryReader.ReadUInt32();
			for (int i = 0; i < nodeCount; i++)
			{
				NodeIds.Add(binaryReader.ReadUInt32());
			}
		}

		public uint ComputeTotalSize()
		{
			return 8 + (uint)(NodeIds.Count * 4);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(SwitchId);
			binaryWriter.Write((uint)NodeIds.Count);
			for (int i = 0; i < NodeIds.Count; i++)
			{
				binaryWriter.Write(NodeIds[i]);
			}
		}
	}

	public class AkSwitchNodeParams : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint NodeId { get; set; }
		public byte ByBitVector1 { get; set; }
		public byte ByBitVector2 { get; set; }
		public float FadeOutTime { get; set; }
		public float FadeInTime { get; set; }

		public AkSwitchNodeParams() { }

		public AkSwitchNodeParams(BinaryReader binaryReader)
		{
			NodeId = binaryReader.ReadUInt32();
			ByBitVector1 = binaryReader.ReadByte();
			ByBitVector2 = binaryReader.ReadByte();
			FadeOutTime = binaryReader.ReadSingle();
			FadeInTime = binaryReader.ReadSingle();
		}

		public uint ComputeTotalSize()
		{
			return 14;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(NodeId);
			binaryWriter.Write(ByBitVector1);
			binaryWriter.Write(ByBitVector2);
			binaryWriter.Write(FadeOutTime);
			binaryWriter.Write(FadeInTime);
		}
	}
}
