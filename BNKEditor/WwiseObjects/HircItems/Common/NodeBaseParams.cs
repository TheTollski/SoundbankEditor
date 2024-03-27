using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems.Common
{
	public class NodeBaseParams : WwiseObject
	{
		public NodeInitialFxParams NodeInitialFxParams { get; set; } = new NodeInitialFxParams();
		public byte OverrideAttachmentParams { get; set; }
		public uint OverrideBusId { get; set; }
		public uint DirectParentID { get; set; }
		public byte ByBitVector { get; set; }
		public NodeInitialParams NodeInitialParams { get; set; } = new NodeInitialParams();
		public PositioningParams PositioningParams { get; set; } = new PositioningParams();
		public AuxParams AuxParams { get; set; } = new AuxParams();
		public AdvSettingsParams AdvSettingsParams { get; set; } = new AdvSettingsParams();
		public StateChunk StateChunk { get; set; } = new StateChunk();
		public InitialRtpc InitialRtpc { get; set; } = new InitialRtpc();

		public NodeBaseParams() { }

		public NodeBaseParams(BinaryReader binaryReader)
		{
			NodeInitialFxParams = new NodeInitialFxParams(binaryReader);
			OverrideAttachmentParams = binaryReader.ReadByte();
			OverrideBusId = binaryReader.ReadUInt32();
			DirectParentID = binaryReader.ReadUInt32();
			ByBitVector = binaryReader.ReadByte();
			NodeInitialParams = new NodeInitialParams(binaryReader);
			PositioningParams = new PositioningParams(binaryReader);
			AuxParams = new AuxParams(binaryReader);
			AdvSettingsParams = new AdvSettingsParams(binaryReader);
			StateChunk = new StateChunk(binaryReader);
			InitialRtpc = new InitialRtpc(binaryReader);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			NodeInitialFxParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(OverrideAttachmentParams);
			binaryWriter.Write(OverrideBusId);
			binaryWriter.Write(DirectParentID);
			binaryWriter.Write(ByBitVector);
			NodeInitialParams.WriteToBinary(binaryWriter);
			PositioningParams.WriteToBinary(binaryWriter);
			AuxParams.WriteToBinary(binaryWriter);
			AdvSettingsParams.WriteToBinary(binaryWriter);
			StateChunk.WriteToBinary(binaryWriter);
			InitialRtpc.WriteToBinary(binaryWriter);
		}
	}

	public class NodeInitialFxParams : WwiseObject
	{
		public byte IsOverrideParentFX { get; set; }
		public byte FxCount { get; set; }
		public byte? BitsFxBypass { get; set; }
		public List<FxChunk> FxChunks { get; set; } = new List<FxChunk>();

		public NodeInitialFxParams() { }

		public NodeInitialFxParams(BinaryReader binaryReader)
		{
			IsOverrideParentFX = binaryReader.ReadByte();
			FxCount = binaryReader.ReadByte();

			if (FxCount > 0)
			{
				BitsFxBypass = binaryReader.ReadByte();
				for (int i = 0; i < FxCount; i++)
				{
					FxChunks.Add(new FxChunk(binaryReader));
				}
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(IsOverrideParentFX);
			binaryWriter.Write(FxCount);

			if (BitsFxBypass != null)
			{
				binaryWriter.Write(BitsFxBypass.Value);
			}

			for (int i = 0; i < FxChunks.Count; i++)
			{
				FxChunks[i].WriteToBinary(binaryWriter);
			}
		}
	}

	public class FxChunk : WwiseObject
	{
		public byte FxIndex { get; set; }
		public uint FxId { get; set; }
		public byte IsShareSet { get; set; }
		public byte IsRendered { get; set; }

		public FxChunk() { }

		public FxChunk(BinaryReader binaryReader)
		{
			FxIndex = binaryReader.ReadByte();
			FxId = binaryReader.ReadUInt32();
			IsShareSet = binaryReader.ReadByte();
			IsRendered = binaryReader.ReadByte();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(FxIndex);
			binaryWriter.Write(FxId);
			binaryWriter.Write(IsShareSet);
			binaryWriter.Write(IsRendered);
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
		public byte? Bits3d { get; set; }
		public uint? AttenuationId { get; set; }

		public PositioningParams() { }

		public PositioningParams(BinaryReader binaryReader)
		{
			ByVector = binaryReader.ReadByte();

			// I'm not sure about some of the value in the bit vector, so this logic will probably need to change.

			bool _positioningInfoOverrideParent = (ByVector >> 0 & 1) == 1;
			bool _3dPositioningAvailable = (ByVector >> 3 & 1) == 1;

			if (_positioningInfoOverrideParent && _3dPositioningAvailable)
			{
				Bits3d = binaryReader.ReadByte();
				AttenuationId = binaryReader.ReadUInt32();

				if (Bits3d == 0)
				{
					throw new Exception("Cannot handle empty Bits3d.");
				}
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByVector);

			if (Bits3d != null)
			{
				binaryWriter.Write(Bits3d.Value);
			}

			if (AttenuationId != null)
			{
				binaryWriter.Write(AttenuationId.Value);
			}
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

	public class InitialRtpc : WwiseObject
	{
		public ushort RtpcCount { get; set; }
		public List<Rtpc> RtpcList { get; set; } = new List<Rtpc>();

		public InitialRtpc() { }

		public InitialRtpc(BinaryReader binaryReader)
		{
			RtpcCount = binaryReader.ReadUInt16();

			for (int i = 0; i < RtpcCount; i++)
			{
				RtpcList.Add(new Rtpc(binaryReader));
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(RtpcCount);

			for (int i = 0; i < RtpcList.Count; i++)
			{
				RtpcList[i].WriteToBinary(binaryWriter);
			}
		}
	}

	public class Rtpc : WwiseObject
	{
		public uint RtpcId { get; set; }
		public byte RtpcType { get; set; }
		public byte RtpcAccum { get; set; }
		public byte ParamId { get; set; }
		public uint RtpcCurveId { get; set; }
		public byte Scaling { get; set; }
		public ushort ListCount { get; set; }
		public List<AkRTPCGraphPoint> GraphPoints { get; set; } = new List<AkRTPCGraphPoint>();

		public Rtpc() { }

		public Rtpc(BinaryReader binaryReader)
		{
			RtpcId = binaryReader.ReadUInt32();
			RtpcType = binaryReader.ReadByte();
			RtpcAccum = binaryReader.ReadByte();
			ParamId = binaryReader.ReadByte();
			RtpcCurveId = binaryReader.ReadUInt32();
			Scaling = binaryReader.ReadByte();
			ListCount = binaryReader.ReadUInt16();

			for (int i = 0; i < ListCount; i++)
			{
				GraphPoints.Add(new AkRTPCGraphPoint(binaryReader));
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(RtpcId);
			binaryWriter.Write(RtpcType);
			binaryWriter.Write(RtpcAccum);
			binaryWriter.Write(ParamId);
			binaryWriter.Write(RtpcCurveId);
			binaryWriter.Write(Scaling);
			binaryWriter.Write(ListCount);

			for (int i = 0; i < GraphPoints.Count; i++)
			{
				GraphPoints[i].WriteToBinary(binaryWriter);
			}
		}
	}

	public class AkRTPCGraphPoint : WwiseObject
	{
		public float From { get; set; }
		public float To { get; set; }
		public uint Interp { get; set; }

		public AkRTPCGraphPoint() { }

		public AkRTPCGraphPoint(BinaryReader binaryReader)
		{
			From = binaryReader.ReadSingle();
			To = binaryReader.ReadSingle();
			Interp = binaryReader.ReadUInt32();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(From);
			binaryWriter.Write(To);
			binaryWriter.Write(Interp);
		}
	}
}
