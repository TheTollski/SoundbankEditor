using SoundbankEditor.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems.Common
{
	public class NodeBaseParams : WwiseObject
	{
		public NodeInitialFxParams NodeInitialFxParams { get; set; } = new NodeInitialFxParams();
		public byte OverrideAttachmentParams { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint OverrideBusId { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
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

		public uint ComputeTotalSize()
		{
			return 10
				+ NodeInitialFxParams.ComputeTotalSize()
				+ NodeInitialParams.ComputeTotalSize()
				+ PositioningParams.ComputeTotalSize()
				+ AuxParams.ComputeTotalSize()
				+ AdvSettingsParams.ComputeTotalSize()
				+ StateChunk.ComputeTotalSize()
				+ InitialRtpc.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate DirectParentID (Does DirectParentID even do anything?)
			//if (!soundbank.HircItems.Any(hi => hi.UlID == DirectParentID))
			//{
			//	knownValidationErrors.Add($"DirectParentID is '{DirectParentID}', but no HIRC item in the soundbank has that ID.");
			//}

			return knownValidationErrors;
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
		public byte? BitsFxBypass { get; set; }
		public List<FxChunk> FxChunks { get; set; } = new List<FxChunk>();

		public NodeInitialFxParams() { }

		public NodeInitialFxParams(BinaryReader binaryReader)
		{
			IsOverrideParentFX = binaryReader.ReadByte();
			byte fxCount = binaryReader.ReadByte();

			if (fxCount > 0)
			{
				BitsFxBypass = binaryReader.ReadByte();
				for (int i = 0; i < fxCount; i++)
				{
					FxChunks.Add(new FxChunk(binaryReader));
				}
			}
		}

		public uint ComputeTotalSize()
		{
			uint size = 2 + (uint)FxChunks.Sum(fc => fc.ComputeTotalSize());
			if (BitsFxBypass != null) size += 1;
			return size;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(IsOverrideParentFX);
			binaryWriter.Write((byte)FxChunks.Count);
			if (BitsFxBypass != null) binaryWriter.Write(BitsFxBypass.Value);
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

		public uint ComputeTotalSize()
		{
			return 7;
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
		public AkPropBundle AkPropBundle { get; set; } = new AkPropBundle();
		public AkPropBundleMinMax AkPropBundleMinMax { get; set; } = new AkPropBundleMinMax();

		public NodeInitialParams() { }

		public NodeInitialParams(BinaryReader binaryReader)
		{
			AkPropBundle = new AkPropBundle(binaryReader);
			AkPropBundleMinMax = new AkPropBundleMinMax(binaryReader);
		}

		public uint ComputeTotalSize()
		{
			return AkPropBundle.ComputeTotalSize() + AkPropBundleMinMax.ComputeTotalSize();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			AkPropBundle.WriteToBinary(binaryWriter);
			AkPropBundleMinMax.WriteToBinary(binaryWriter);
		}
	}

	public class PositioningParams : WwiseObject
	{
		public byte ByVector { get; set; }
		public byte? Bits3d { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
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

		public uint ComputeTotalSize()
		{
			uint size = 1;
			if (Bits3d != null) size += 1;
			if (AttenuationId != null) size += 4;
			return size;
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
		public uint? AuxId1 { get; set; }
		public uint? AuxId2 { get; set; }
		public uint? AuxId3 { get; set; }
		public uint? AuxId4 { get; set; }

		public AuxParams() { }

		public AuxParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();

			bool hasAux = (ByBitVector >> 3 & 1) == 1;
			if (hasAux)
			{
				AuxId1 = binaryReader.ReadUInt32();
				AuxId2 = binaryReader.ReadUInt32();
				AuxId3 = binaryReader.ReadUInt32();
				AuxId4 = binaryReader.ReadUInt32();
			}
		}

		public uint ComputeTotalSize()
		{
			uint size = 1;
			if (AuxId1 != null) size += 4;
			if (AuxId2 != null) size += 4;
			if (AuxId3 != null) size += 4;
			if (AuxId4 != null) size += 4;
			return size;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
			if (AuxId1 != null) binaryWriter.Write(AuxId1.Value);
			if (AuxId2 != null) binaryWriter.Write(AuxId2.Value);
			if (AuxId3 != null) binaryWriter.Write(AuxId3.Value);
			if (AuxId4 != null) binaryWriter.Write(AuxId4.Value);
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

		public uint ComputeTotalSize()
		{
			return 6;
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
		public List<object> StateGroups { get; set; } = new List<object>();

		public StateChunk() { }

		public StateChunk(BinaryReader binaryReader)
		{
			uint numStateGroups = binaryReader.ReadUInt32();
			if (numStateGroups > 0)
			{
				throw new Exception("StateChunk.StateGroups is not supported.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 4;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((uint)StateGroups.Count);
			if (StateGroups.Count > 0)
			{
				throw new Exception("StateChunk.StateGroups is not supported.");
			}
		}
	}

	public class InitialRtpc : WwiseObject
	{
		public List<Rtpc> RtpcList { get; set; } = new List<Rtpc>();

		public InitialRtpc() { }

		public InitialRtpc(BinaryReader binaryReader)
		{
			ushort rtpcCount = binaryReader.ReadUInt16();
			for (int i = 0; i < rtpcCount; i++)
			{
				RtpcList.Add(new Rtpc(binaryReader));
			}
		}

		public uint ComputeTotalSize()
		{
			return 2 + (uint)RtpcList.Sum(r => r.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((ushort)RtpcList.Count);
			for (int i = 0; i < RtpcList.Count; i++)
			{
				RtpcList[i].WriteToBinary(binaryWriter);
			}
		}
	}

	public class Rtpc : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint RtpcId { get; set; }
		public byte RtpcType { get; set; }
		public byte RtpcAccum { get; set; }
		public byte ParamId { get; set; }
		public uint RtpcCurveId { get; set; }
		public byte Scaling { get; set; }
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
			ushort listCount = binaryReader.ReadUInt16();
			for (int i = 0; i < listCount; i++)
			{
				GraphPoints.Add(new AkRTPCGraphPoint(binaryReader));
			}
		}

		public uint ComputeTotalSize()
		{
			return 14 + (uint)GraphPoints.Sum(gp => gp.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(RtpcId);
			binaryWriter.Write(RtpcType);
			binaryWriter.Write(RtpcAccum);
			binaryWriter.Write(ParamId);
			binaryWriter.Write(RtpcCurveId);
			binaryWriter.Write(Scaling);
			binaryWriter.Write((ushort)GraphPoints.Count);
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

		public uint ComputeTotalSize()
		{
			return 12;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(From);
			binaryWriter.Write(To);
			binaryWriter.Write(Interp);
		}
	}
}
