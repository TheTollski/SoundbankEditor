using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CakSound : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		public uint UlID { get; set; }
		public AkBankSourceData AkBankSourceData { get; set; } = new AkBankSourceData();
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();
		public PositioningParams PositioningParams { get; set; } = new PositioningParams();
		public AuxParams AuxParams { get; set; } = new AuxParams();
		public AdvSettingsParams AdvSettingsParams { get; set; } = new AdvSettingsParams();
		public StateChunk StateChunk { get; set; } = new StateChunk();
		public InitialRTPC InitialRTPC { get; set; } = new InitialRTPC();
		public byte[]? ExtraData { get; set; }

		public CakSound() { }

		public CakSound(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			AkBankSourceData = new AkBankSourceData(binaryReader);
			NodeBaseParams = new NodeBaseParams(binaryReader);
			PositioningParams = new PositioningParams(binaryReader);
			AuxParams = new AuxParams(binaryReader);
			AdvSettingsParams = new AdvSettingsParams(binaryReader);
			StateChunk = new StateChunk(binaryReader);
			InitialRTPC = new InitialRTPC(binaryReader);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CakSound '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);
			binaryWriter.Write(UlID);
			AkBankSourceData.WriteToBinary(binaryWriter);
			NodeBaseParams.WriteToBinary(binaryWriter);
			PositioningParams.WriteToBinary(binaryWriter);
			AuxParams.WriteToBinary(binaryWriter);
			AdvSettingsParams.WriteToBinary(binaryWriter);
			StateChunk.WriteToBinary(binaryWriter);
			InitialRTPC.WriteToBinary(binaryWriter);

			if (ExtraData != null)
			{
				binaryWriter.Write(ExtraData);
			}
		}
	}

	public class AkBankSourceData : WwiseObject
	{
		public uint UlPluginId { get; set; }
		public byte StreamType { get; set; }
		public AkMediaInformation AkMediaInformation { get; set; } = new AkMediaInformation();

		public AkBankSourceData() { }

		public AkBankSourceData(BinaryReader binaryReader)
		{
			UlPluginId = binaryReader.ReadUInt32();
			StreamType = binaryReader.ReadByte();
			AkMediaInformation = new AkMediaInformation(binaryReader);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(UlPluginId);
			binaryWriter.Write(StreamType);
			AkMediaInformation.WriteToBinary(binaryWriter);
		}
	}

	public class AkMediaInformation : WwiseObject
	{
		public uint SourceId { get; set; }
		public uint FileId { get; set; }
		public uint InMemoryMediaSize { get; set; }
		public byte SourceBits {  get; set; }

		public AkMediaInformation() { }

		public AkMediaInformation(BinaryReader binaryReader)
		{
			SourceId = binaryReader.ReadUInt32();
			FileId = binaryReader.ReadUInt32();
			InMemoryMediaSize = binaryReader.ReadUInt32();
			SourceBits = binaryReader.ReadByte();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(SourceId);
			binaryWriter.Write(FileId);
			binaryWriter.Write(InMemoryMediaSize);
			binaryWriter.Write(SourceBits);
		}
	}

	public class NodeBaseParams : WwiseObject
	{
		public NodeInitialFxParams NodeInitialFxParams { get; set; } = new NodeInitialFxParams();
		public byte OverrideAttachmentParams { get; set; }
		public uint OverrideBusId { get; set; }
		public uint DirectParentID { get; set; }
		public byte ByBitVector { get; set; }
		public NodeInitialParams NodeInitialParams { get; set; } = new NodeInitialParams();

		public NodeBaseParams() { }

		public NodeBaseParams(BinaryReader binaryReader)
		{
			NodeInitialFxParams = new NodeInitialFxParams(binaryReader);
			OverrideAttachmentParams = binaryReader.ReadByte();
			OverrideBusId = binaryReader.ReadUInt32();
			DirectParentID = binaryReader.ReadUInt32();
			ByBitVector = binaryReader.ReadByte();
			NodeInitialParams = new NodeInitialParams(binaryReader);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			NodeInitialFxParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(OverrideAttachmentParams);
			binaryWriter.Write(OverrideBusId);
			binaryWriter.Write(DirectParentID);
			binaryWriter.Write(ByBitVector);
			NodeInitialParams.WriteToBinary(binaryWriter);
		}
	}

	public class NodeInitialFxParams : WwiseObject
	{
		public byte IsOverrideParentFX { get; set; }
		public byte FxCount { get; set; }

		public NodeInitialFxParams() { }

		public NodeInitialFxParams(BinaryReader binaryReader)
		{
			IsOverrideParentFX = binaryReader.ReadByte();
			FxCount = binaryReader.ReadByte();

			if (FxCount > 0)
			{
				throw new Exception("NodeInitialFxParams.FxList is not supported.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(IsOverrideParentFX);
			binaryWriter.Write(FxCount);

			if (FxCount > 0)
			{
				throw new Exception("NodeInitialFxParams.FxList is not supported.");
			}
		}
	}

	public class NodeInitialParams : WwiseObject
	{
		public AkPropBundle AkPropBundle1 { get; set; } = new AkPropBundle();
		public AkPropBundle AkPropBundle2 { get; set; } = new AkPropBundle();

		public NodeInitialParams() { }

		public NodeInitialParams(BinaryReader binaryReader)
		{
			AkPropBundle1 = new AkPropBundle(binaryReader);
			AkPropBundle2 = new AkPropBundle(binaryReader);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			AkPropBundle1.WriteToBinary(binaryWriter);
			AkPropBundle2.WriteToBinary(binaryWriter);
		}
	}

	public class PositioningParams : WwiseObject
	{
		public byte ByVector { get; set; }

		public PositioningParams() { }

		public PositioningParams(BinaryReader binaryReader)
		{
			ByVector = binaryReader.ReadByte();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByVector);
		}
	}

	public class AuxParams : WwiseObject
	{
		public byte ByBitVector { get; set; }

		public AuxParams() { }

		public AuxParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
		}
	}

	public class AdvSettingsParams : WwiseObject
	{
		public byte ByBitVector1 { get; set; }
		public byte VirtualQueueBehavior { get; set; }
		public ushort MaxNumInstance { get; set; }
		public byte BelowThresholdBehavior { get; set; }
		public byte ByBitVector2 { get; set; }

		public AdvSettingsParams() { }

		public AdvSettingsParams(BinaryReader binaryReader)
		{
			ByBitVector1 = binaryReader.ReadByte();
			VirtualQueueBehavior = binaryReader.ReadByte();
			MaxNumInstance = binaryReader.ReadUInt16();
			BelowThresholdBehavior = binaryReader.ReadByte();
			ByBitVector2 = binaryReader.ReadByte();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector1);
			binaryWriter.Write(VirtualQueueBehavior);
			binaryWriter.Write(MaxNumInstance);
			binaryWriter.Write(BelowThresholdBehavior);
			binaryWriter.Write(ByBitVector2);
		}
	}

	public class StateChunk : WwiseObject
	{
		public uint NumStateGroups { get; set; }

		public StateChunk() { }

		public StateChunk(BinaryReader binaryReader)
		{
			NumStateGroups = binaryReader.ReadUInt32();

			if (NumStateGroups > 0)
			{
				throw new Exception("StateChunk.StateGroups is not supported.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(NumStateGroups);

			if (NumStateGroups > 0)
			{
				throw new Exception("StateChunk.StateGroups is not supported.");
			}
		}
	}

	public class InitialRTPC : WwiseObject
	{
		public ushort NumRTPC { get; set; }

		public InitialRTPC() { }

		public InitialRTPC(BinaryReader binaryReader)
		{
			NumRTPC = binaryReader.ReadUInt16();

			if (NumRTPC > 0)
			{
				throw new Exception("InitialRTPC.RTPCs is not supported.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(NumRTPC);

			if (NumRTPC > 0)
			{
				throw new Exception("InitialRTPC.RTPCs is not supported.");
			}
		}
	}
}
