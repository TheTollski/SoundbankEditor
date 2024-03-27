using BNKEditor.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CAkSwitchCntr : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		public uint UlID { get; set; }
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();
		public byte GroupType { get; set; }
		public uint GroupId { get; set; }
		public uint DefaultSwitch { get; set; }
		public byte IsContinuousValidation { get; set; }
		public uint ChildCount { get; set; }
		public List<uint> ChildIds { get; set; } = new List<uint>();
		public uint SwitchPackageCount { get; set; }
		public List<CAkSwitchPackage> SwitchPackages { get; set; } = new List<CAkSwitchPackage>();
		public uint SwitchParamsCount { get; set; }
		public List<AkSwitchNodeParams> SwitchParams { get; set; } = new List<AkSwitchNodeParams>();

		public CAkSwitchCntr() { }

		public CAkSwitchCntr(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			NodeBaseParams = new NodeBaseParams(binaryReader);
			GroupType = binaryReader.ReadByte();
			GroupId = binaryReader.ReadUInt32();
			DefaultSwitch = binaryReader.ReadUInt32();
			IsContinuousValidation = binaryReader.ReadByte();
			ChildCount = binaryReader.ReadUInt32();
			for (int i = 0; i < ChildCount; i++)
			{
				ChildIds.Add(binaryReader.ReadUInt32());
			}
			SwitchPackageCount = binaryReader.ReadUInt32();
			for (int i = 0; i < SwitchPackageCount; i++)
			{
				SwitchPackages.Add(new CAkSwitchPackage(binaryReader));
			}
			SwitchParamsCount = binaryReader.ReadUInt32();
			for (int i = 0; i < SwitchParamsCount; i++)
			{
				SwitchParams.Add(new AkSwitchNodeParams(binaryReader));
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CAkSwitchCntr '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (ChildCount != ChildIds.Count)
			{
				throw new Exception($"Expected CAkSwitchCntr '{UlID}' to have {ChildCount} children but it has {ChildIds.Count}.");
			}
			if (SwitchPackageCount != SwitchPackages.Count)
			{
				throw new Exception($"Expected CAkSwitchCntr '{UlID}' to have {SwitchPackageCount} SwitchPackages but it has {SwitchPackages.Count}.");
			}
			if (SwitchParamsCount != SwitchParams.Count)
			{
				throw new Exception($"Expected CAkSwitchCntr '{UlID}' to have {SwitchParamsCount} SwitchParams but it has {SwitchParams.Count}.");
			}

			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);

			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(UlID);
			NodeBaseParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(GroupType);
			binaryWriter.Write(GroupId);
			binaryWriter.Write(DefaultSwitch);
			binaryWriter.Write(IsContinuousValidation);
			binaryWriter.Write(ChildCount);
			for (int i = 0; i < ChildIds.Count; i++)
			{
				binaryWriter.Write(ChildIds[i]);
			}
			binaryWriter.Write(SwitchPackageCount);
			for (int i = 0; i < SwitchPackages.Count; i++)
			{
				SwitchPackages[i].WriteToBinary(binaryWriter);
			}
			binaryWriter.Write(SwitchParamsCount);
			for (int i = 0; i < SwitchParams.Count; i++)
			{
				SwitchParams[i].WriteToBinary(binaryWriter);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != DwSectionSize)
			{
				throw new Exception($"Expected CAkSwitchCntr '{UlID}' section size to be {DwSectionSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class CAkSwitchPackage : WwiseObject
	{
		public uint SwitchId { get; set; }
		public uint NodeCount { get; set; }
		public List<uint> NodeIds { get; set; } = new List<uint>();

		public CAkSwitchPackage() { }

		public CAkSwitchPackage(BinaryReader binaryReader)
		{
			SwitchId = binaryReader.ReadUInt32();
			NodeCount = binaryReader.ReadUInt32();
			for (int i = 0; i < NodeCount; i++)
			{
				NodeIds.Add(binaryReader.ReadUInt32());
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (NodeCount != NodeIds.Count)
			{
				throw new Exception($"Expected CAkSwitchPackage to have {NodeCount} nodes but it has {NodeIds.Count}.");
			}

			binaryWriter.Write(SwitchId);
			binaryWriter.Write(NodeCount);
			for (int i = 0; i < NodeIds.Count; i++)
			{
				binaryWriter.Write(NodeIds[i]);
			}
		}
	}

	public class AkSwitchNodeParams : WwiseObject
	{
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
