using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CAkAction : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.Action;

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
		public CAkActionType UlActionType { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint IdExt { get; set; }
		public byte IdExt_4 { get; set; }
		public AkPropBundle AkPropBundle1 { get; set; } = new AkPropBundle();
		public AkPropBundle AkPropBundle2 { get; set; } = new AkPropBundle();

		public ActiveActionParams? ActiveActionParams { get; set; }
		public PlayActionParams? PlayActionParams { get; set; }
		public SeekActionParams? SeekActionParams { get; set; }
		public ValueActionParams? ValueActionParams { get; set; }

		public CAkAction() { }

		public CAkAction(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			UlActionType = (CAkActionType)binaryReader.ReadUInt16();
			IdExt = binaryReader.ReadUInt32();
			IdExt_4 = binaryReader.ReadByte();
			AkPropBundle1 = new AkPropBundle(binaryReader);
			AkPropBundle2 = new AkPropBundle(binaryReader);

			if (UlActionType == CAkActionType.Mute || UlActionType == CAkActionType.Unmute)
			{
				ValueActionParams = new ValueActionParams(binaryReader);
			}
			else if (UlActionType == CAkActionType.Pause || UlActionType == CAkActionType.Resume || UlActionType == CAkActionType.Stop)
			{
				ActiveActionParams = new ActiveActionParams(binaryReader, UlActionType);
			}
			else if (UlActionType == CAkActionType.Play)
			{
				PlayActionParams = new PlayActionParams(binaryReader);
			}
			else if (UlActionType == CAkActionType.Seek)
			{
				SeekActionParams = new SeekActionParams(binaryReader);
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CakAction '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			uint size = 16 + AkPropBundle1.ComputeTotalSize() + AkPropBundle2.ComputeTotalSize();
			if (ActiveActionParams != null) size += ActiveActionParams.ComputeTotalSize();
			if (PlayActionParams != null) size += PlayActionParams.ComputeTotalSize();
			if (SeekActionParams != null) size += SeekActionParams.ComputeTotalSize();
			if (ValueActionParams != null) size += ValueActionParams.ComputeTotalSize();
			return size;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			uint expectedSize = ComputeTotalSize() - 5;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.Write(UlID);

			binaryWriter.Write((short)UlActionType);
			binaryWriter.Write(IdExt);
			binaryWriter.Write(IdExt_4);
			AkPropBundle1.WriteToBinary(binaryWriter);
			AkPropBundle2.WriteToBinary(binaryWriter);
			ActiveActionParams?.WriteToBinary(binaryWriter);
			PlayActionParams?.WriteToBinary(binaryWriter);
			SeekActionParams?.WriteToBinary(binaryWriter);
			ValueActionParams?.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CakAction '{UlID}' size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public enum CAkActionType : ushort
	{
		Stop = 259,
		Pause = 515,
		Resume = 771,
		Play = 1027,
		Mute = 1539,
		Unmute = 1795,
		Seek = 7683,
	}

	public class ActiveActionParams : WwiseObject
	{
		public byte ByBitVector { get; set; }

		public PauseActionSpecificParams? PauseActionSpecificParams { get; set; }
		public ResumeActionSpecificParams? ResumeActionSpecificParams {  get; set; }

		public ExceptParams ExceptParams { get; set; } = new ExceptParams();

		public ActiveActionParams() { }

		public ActiveActionParams(BinaryReader binaryReader, CAkActionType actionType)
		{
			ByBitVector = binaryReader.ReadByte();
			if (actionType == CAkActionType.Pause)
			{
				PauseActionSpecificParams = new PauseActionSpecificParams(binaryReader);
			}
			else if (actionType == CAkActionType.Resume)
			{
				ResumeActionSpecificParams = new ResumeActionSpecificParams(binaryReader);
			}
			ExceptParams = new ExceptParams(binaryReader);
		}

		public uint ComputeTotalSize()
		{
			uint size = 1 + ExceptParams.ComputeTotalSize();
			if (PauseActionSpecificParams != null) size += PauseActionSpecificParams.ComputeTotalSize();
			if (ResumeActionSpecificParams != null) size += ResumeActionSpecificParams.ComputeTotalSize();
			return size;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
			PauseActionSpecificParams?.WriteToBinary(binaryWriter);
			ResumeActionSpecificParams?.WriteToBinary(binaryWriter);
			ExceptParams.WriteToBinary(binaryWriter);
		}
	}

	public class PauseActionSpecificParams : WwiseObject
	{
		public byte ByBitVector { get; set; }

		public PauseActionSpecificParams() { }

		public PauseActionSpecificParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 1;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
		}
	}

	public class ResumeActionSpecificParams : WwiseObject
	{
		public byte ByBitVector { get; set; }

		public ResumeActionSpecificParams() { }

		public ResumeActionSpecificParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 1;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
		}
	}

	public class PlayActionParams : WwiseObject
	{
		public byte ByBitVector { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint FileId { get; set; }

		public PlayActionParams() { }

		public PlayActionParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();
			FileId = binaryReader.ReadUInt32();
		}

		public uint ComputeTotalSize()
		{
			return 5;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
			binaryWriter.Write(FileId);
		}
	}

	public class SeekActionParams : WwiseObject
	{
		public byte IsSeekRelativeToDuration { get; set; }
		public RandomizerModifier RandomizerModifier { get; set; } = new RandomizerModifier();
		public byte SnapToNearestMarker { get; set; }
		public ExceptParams ExceptParams { get; set; } = new ExceptParams();

		public SeekActionParams() { }

		public SeekActionParams(BinaryReader binaryReader)
		{
			IsSeekRelativeToDuration = binaryReader.ReadByte();
			RandomizerModifier = new RandomizerModifier(binaryReader);
			SnapToNearestMarker = binaryReader.ReadByte();
			ExceptParams = new ExceptParams(binaryReader);
		}

		public uint ComputeTotalSize()
		{
			return 2 + RandomizerModifier.ComputeTotalSize() + ExceptParams.ComputeTotalSize();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(IsSeekRelativeToDuration);
			RandomizerModifier.WriteToBinary(binaryWriter);
			binaryWriter.Write(SnapToNearestMarker);
			ExceptParams.WriteToBinary(binaryWriter);
		}
	}

	public class RandomizerModifier : WwiseObject
	{
		public float SeekValue { get; set; }
		public float SeekValueMin { get; set; }
		public float SeekValueMax { get; set; }

		public RandomizerModifier() { }

		public RandomizerModifier(BinaryReader binaryReader)
		{
			SeekValue = binaryReader.ReadSingle();
			SeekValueMin = binaryReader.ReadSingle();
			SeekValueMax = binaryReader.ReadSingle();
		}

		public uint ComputeTotalSize()
		{
			return 12;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(SeekValue);
			binaryWriter.Write(SeekValueMin);
			binaryWriter.Write(SeekValueMax);
		}
	}

	public class ValueActionParams : WwiseObject
	{
		public byte ByBitVector { get; set; }
		public ExceptParams ExceptParams { get; set; } = new ExceptParams();

		public ValueActionParams() { }

		public ValueActionParams(BinaryReader binaryReader)
		{
			ByBitVector = binaryReader.ReadByte();
			ExceptParams = new ExceptParams(binaryReader);
		}

		public uint ComputeTotalSize()
		{
			return 1 + ExceptParams.ComputeTotalSize();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
			ExceptParams.WriteToBinary(binaryWriter);
		}
	}

	public class ExceptParams : WwiseObject
	{
		public List<object> Exceptions { get; set; } = new List<object>();

		public ExceptParams() { }

		public ExceptParams(BinaryReader binaryReader)
		{
			uint exceptionCount = binaryReader.ReadUInt32();
			if (exceptionCount > 0)
			{
				throw new Exception("ExceptParams.Exceptions is not supported.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 4;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((uint)Exceptions.Count);
			if (Exceptions.Count > 0)
			{
				throw new Exception("ExceptParams.Exceptions is not supported.");
			}
		}
	}
}
