using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CakAction : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		public uint UlID { get; set; }
		public ActionType UlActionType { get; set; }
		public uint IdExt { get; set; }
		public byte IdExt_4 { get; set; }
		public AkPropBundle AkPropBundle1 { get; set; } = new AkPropBundle();
		public AkPropBundle AkPropBundle2 { get; set; } = new AkPropBundle();

		public PlayActionParams? PlayActionParams { get; set; }
		public ValueActionParams? ValueActionParams { get; set; }

		public byte[]? ExtraData { get; set; }

		public CakAction() { }

		public CakAction(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			UlActionType = (ActionType)binaryReader.ReadUInt16();
			IdExt = binaryReader.ReadUInt32();
			IdExt_4 = binaryReader.ReadByte();
			AkPropBundle1 = new AkPropBundle(binaryReader);
			AkPropBundle2 = new AkPropBundle(binaryReader);

			bool knownType = true;
			if (UlActionType == ActionType.Mute || UlActionType == ActionType.Unmute)
			{
				ValueActionParams = new ValueActionParams(binaryReader);
			}
			else if (UlActionType == ActionType.Play)
			{
				PlayActionParams = new PlayActionParams(binaryReader);
			}
			else
			{
				knownType = false;
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				if (knownType)
				{
					throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CakAction '{UlID}'.");
				}

				ExtraData = binaryReader.ReadBytes((int)DwSectionSize - bytesReadFromThisObject);
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);
			binaryWriter.Write(UlID);
			binaryWriter.Write((short)UlActionType);
			binaryWriter.Write(IdExt);
			binaryWriter.Write(IdExt_4);
			AkPropBundle1.WriteToBinary(binaryWriter);
			AkPropBundle2.WriteToBinary(binaryWriter);
			PlayActionParams?.WriteToBinary(binaryWriter);
			ValueActionParams?.WriteToBinary(binaryWriter);

			if (ExtraData != null)
			{
				binaryWriter.Write(ExtraData);
			}
		}
	}

	public enum ActionType : ushort
	{
		Resume = 771,
		Play = 1027,
		Mute = 1539,
		Unmute = 1795,
	}

	public class AkPropBundle : WwiseObject
	{
		public byte PropCount { get; set; }
		public List<AkProp> Props { get; set; } = new List<AkProp>();

		public AkPropBundle() { }

		public AkPropBundle(BinaryReader binaryReader)
		{
			PropCount = binaryReader.ReadByte();
			for (int i = 0; i < PropCount; i++)
			{
				Props.Add(new AkProp { Id = binaryReader.ReadByte() });
			}
			for (int i = 0; i < PropCount; i++)
			{
				Props[i].Value = binaryReader.ReadSingle();
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(PropCount);
			for (int i = 0; i < Props.Count; i++)
			{
				binaryWriter.Write(Props[i].Id);
			}
			for (int i = 0; i < Props.Count; i++)
			{
				binaryWriter.Write(Props[i].Value);
			}
		}
	}

	public class AkProp
	{
		public byte Id { get; set; }
		public float Value { get; set; }

		public AkProp() { }

		// Note: An AkProp is not stored as a whole in the binary. For a list, all the AkProp IDs are stored and then all the values are stored.
	}

	public class PlayActionParams : WwiseObject
	{
		public byte ByBitVector { get; set; }
		public uint FileId { get; set; }

		public PlayActionParams() { }

		public PlayActionParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();
			// Wwiser shows an extra field "eFadeCurve" that isn't in the data, it must be extrapolated from "byBitVector".
			FileId = binaryReader.ReadUInt32();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
			binaryWriter.Write(FileId);
		}
	}

	public class ValueActionParams : WwiseObject
	{
		public byte ByBitVector { get; set; }
		public uint ExceptionCount { get; set; }
		public List<object>? Exceptions { get; set; }

		public ValueActionParams() { }

		public ValueActionParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();
			// Wwiser shows an extra field "eFadeCurve" that isn't in the data, it must be extrapolated from "byBitVector".
			ExceptionCount = binaryReader.ReadUInt32();

			if (ExceptionCount > 0)
			{
				throw new Exception("ValueActionParams.Exceptions is not supported.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
			binaryWriter.Write(ExceptionCount);

			if (ExceptionCount > 0)
			{
				throw new Exception("ValueActionParams.Exceptions is not supported.");
			}
		}
	}
}
