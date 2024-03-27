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
		public StateActionParams? StateActionParams { get; set; }
		public ValueActionParams? ValueActionParams { get; set; }

		public CAkAction()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

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

			if (
				UlActionType == CAkActionType.Mute ||
				UlActionType == CAkActionType.ResetLPF_M ||
				UlActionType == CAkActionType.SetBusVolume_M ||
				UlActionType == CAkActionType.SetGameParameter_O ||
				UlActionType == CAkActionType.SetLPF_M ||
				UlActionType == CAkActionType.SetVolume_O ||
				UlActionType == CAkActionType.Unmute
			)
			{
				ValueActionParams = new ValueActionParams(binaryReader, UlActionType);
			}
			else if (
				UlActionType == CAkActionType.Pause ||
				UlActionType == CAkActionType.Resume ||
				UlActionType == CAkActionType.Stop_E ||
				UlActionType == CAkActionType.Stop_E_O
			)
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
			else if (UlActionType == CAkActionType.SetState)
			{
				StateActionParams = new StateActionParams(binaryReader);
			}
			else
			{
				throw new Exception($"CAkAction '{UlID}' has an unsupported action type '{UlActionType}'.");
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
			if (StateActionParams != null) size += StateActionParams.ComputeTotalSize();
			if (ValueActionParams != null) size += ValueActionParams.ComputeTotalSize();
			return size;
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkAction '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
			}

			// Validate IdExt
			if (!soundbank.HircItems.Any(hi => hi.UlID == IdExt))
			{
				knownValidationErrors.Add($"CAkAction '{UlID}' has IdExt '{IdExt}', but no HIRC item in the soundbank has that ID.");
			}

			return knownValidationErrors;
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
			StateActionParams?.WriteToBinary(binaryWriter);
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
		Stop_E = 258,
		Stop_E_O = 259,
		Pause = 515,
		Resume = 771,
		Play = 1027,
		Mute = 1539,
		Unmute = 1795,
		SetVolume_O = 2563,
		SetBusVolume_M = 3074,
		SetLPF_M = 3586,
		ResetLPF_M = 3842,
		SetState = 4612,
		SetGameParameter_O = 4867,
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

	public class StateActionParams : WwiseObject
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint StateGroup { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint TargetStateId { get; set; }

		public StateActionParams() { }

		public StateActionParams(BinaryReader binaryReader)
		{
			StateGroup = binaryReader.ReadUInt32();
			TargetStateId = binaryReader.ReadUInt32();
		}

		public uint ComputeTotalSize()
		{
			return 8;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(StateGroup);
			binaryWriter.Write(TargetStateId);
		}
	}

	public class ValueActionParams : WwiseObject
	{
		public byte ByBitVector { get; set; }
		public AkPropActionSpecificParams? AkPropActionSpecificParams { get; set; }
		public GameParameterActionSpecificParams? GameParameterActionSpecificParams { get; set; }
		public ExceptParams ExceptParams { get; set; } = new ExceptParams();

		public ValueActionParams() { }

		public ValueActionParams(BinaryReader binaryReader, CAkActionType actionType)
		{
			ByBitVector = binaryReader.ReadByte();

			if (actionType == CAkActionType.ResetLPF_M ||
					actionType == CAkActionType.SetBusVolume_M ||
					actionType == CAkActionType.SetLPF_M ||
					actionType == CAkActionType.SetVolume_O)
			{
				AkPropActionSpecificParams = new AkPropActionSpecificParams(binaryReader);
			}
			if (actionType == CAkActionType.SetGameParameter_O)
			{
				GameParameterActionSpecificParams = new GameParameterActionSpecificParams(binaryReader);
			}

			ExceptParams = new ExceptParams(binaryReader);
		}

		public uint ComputeTotalSize()
		{
			uint size = 1 + ExceptParams.ComputeTotalSize();
			if (AkPropActionSpecificParams != null) size += AkPropActionSpecificParams.ComputeTotalSize();
			if (GameParameterActionSpecificParams != null) size += GameParameterActionSpecificParams.ComputeTotalSize();
			return size;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ByBitVector);
			if (AkPropActionSpecificParams != null) AkPropActionSpecificParams.WriteToBinary(binaryWriter);
			if (GameParameterActionSpecificParams != null) GameParameterActionSpecificParams.WriteToBinary(binaryWriter);
			ExceptParams.WriteToBinary(binaryWriter);
		}
	}

	public class AkPropActionSpecificParams : WwiseObject
	{
		public byte ValueMeaning { get; set; }
		public float RandomizerModifierBase { get; set; }
		public float RandomizerModifierMin { get; set; }
		public float RandomizerModifierMax { get; set; }

		public AkPropActionSpecificParams() { }

		public AkPropActionSpecificParams(BinaryReader binaryReader)
		{
			ValueMeaning = binaryReader.ReadByte();
			RandomizerModifierBase = binaryReader.ReadSingle();
			RandomizerModifierMin = binaryReader.ReadSingle();
			RandomizerModifierMax = binaryReader.ReadSingle();
		}

		public uint ComputeTotalSize()
		{
			return 13;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ValueMeaning);
			binaryWriter.Write(RandomizerModifierBase);
			binaryWriter.Write(RandomizerModifierMin);
			binaryWriter.Write(RandomizerModifierMax);
		}
	}

	public class GameParameterActionSpecificParams : WwiseObject
	{
		public byte BypassTransition { get; set; }
		public byte ValueMeaning { get; set; }
		public float RangedParameterBase { get; set; }
		public float RangedParameterMin { get; set; }
		public float RangedParameterMax { get; set; }

		public GameParameterActionSpecificParams() { }

		public GameParameterActionSpecificParams(BinaryReader binaryReader)
		{
			BypassTransition = binaryReader.ReadByte();
			ValueMeaning = binaryReader.ReadByte();
			RangedParameterBase = binaryReader.ReadSingle();
			RangedParameterMin = binaryReader.ReadSingle();
			RangedParameterMax = binaryReader.ReadSingle();
		}

		public uint ComputeTotalSize()
		{
			return 14;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(BypassTransition);
			binaryWriter.Write(ValueMeaning);
			binaryWriter.Write(RangedParameterBase);
			binaryWriter.Write(RangedParameterMin);
			binaryWriter.Write(RangedParameterMax);
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
