using BNKEditor.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CAkAction : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		public uint UlID { get; set; }
		public CAkActionType UlActionType { get; set; }
		public uint IdExt { get; set; }
		public byte IdExt_4 { get; set; }
		public AkPropBundle AkPropBundle1 { get; set; } = new AkPropBundle();
		public AkPropBundle AkPropBundle2 { get; set; } = new AkPropBundle();

		public PlayActionParams? PlayActionParams { get; set; }
		public ValueActionParams? ValueActionParams { get; set; }

		public byte[]? ExtraData { get; set; }

		public CAkAction() { }

		public CAkAction(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			UlActionType = (CAkActionType)binaryReader.ReadUInt16();
			IdExt = binaryReader.ReadUInt32();
			IdExt_4 = binaryReader.ReadByte();
			AkPropBundle1 = new AkPropBundle(binaryReader);
			AkPropBundle2 = new AkPropBundle(binaryReader);

			bool knownType = true;
			if (UlActionType == CAkActionType.Mute || UlActionType == CAkActionType.Unmute)
			{
				ValueActionParams = new ValueActionParams(binaryReader);
			}
			else if (UlActionType == CAkActionType.Play)
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

			long position = binaryWriter.BaseStream.Position;

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

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != DwSectionSize)
			{
				throw new Exception($"Expected CAkAction '{UlID}' section size to be {DwSectionSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public enum CAkActionType : ushort
	{
		Resume = 771,
		Play = 1027,
		Mute = 1539,
		Unmute = 1795,
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
		public List<object> Exceptions { get; set; } = new List<object>();

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
			if (ExceptionCount != Exceptions.Count)
			{
				throw new Exception($"Expected ValueActionParams to have {ExceptionCount} children but it has {Exceptions.Count}.");
			}

			binaryWriter.Write(ByBitVector);
			binaryWriter.Write(ExceptionCount);
			if (ExceptionCount > 0)
			{
				throw new Exception("ValueActionParams.Exceptions is not supported.");
			}
		}
	}
}
