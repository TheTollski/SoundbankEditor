﻿using SoundbankEditor.Core.Utility;
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
	public class CAkFxShareSet : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.FxShareSet;

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
		public uint FxId { get; set; }
		public uint Size { get; set; }
		public AkParameterEQFXParams AkParameterEQFXParams { get; set; } = new AkParameterEQFXParams();
		public List<object> Media { get; set; } = new List<object> { };
		public InitialRtpc InitialRtpc { get; set; } = new InitialRtpc();
		public List<object> RtpcInit { get; set; } = new List<object>();

		public CAkFxShareSet()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkFxShareSet(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			FxId = binaryReader.ReadUInt32();
			Size = binaryReader.ReadUInt32();
			AkParameterEQFXParams = new AkParameterEQFXParams(binaryReader, Size);
			byte numBankData = binaryReader.ReadByte();
			if (numBankData > 0)
			{
				throw new Exception("CAkFxShareSet.Media is not supported.");
			}
			InitialRtpc = new InitialRtpc(binaryReader);
			ushort numInit = binaryReader.ReadUInt16();
			if (numInit > 0)
			{
				throw new Exception("CAkFxShareSet.RtpcInit is not supported.");
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkFxShareSet '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 20 + AkParameterEQFXParams.ComputeTotalSize() + InitialRtpc.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkFxShareSet '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
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

			binaryWriter.Write(FxId);
			binaryWriter.Write(Size);
			AkParameterEQFXParams.WriteToBinary(binaryWriter);
			binaryWriter.Write((byte)Media.Count);
			if (Media.Count > 0)
			{
				throw new Exception("CAkFxShareSet.Media is not supported.");
			}
			InitialRtpc.WriteToBinary(binaryWriter);
			binaryWriter.Write((ushort)RtpcInit.Count);
			if (RtpcInit.Count > 0)
			{
				throw new Exception("CAkFxShareSet.RtpcInit is not supported.");
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkFxShareSet '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class AkParameterEQFXParams : WwiseObject
	{
		public List<EQModuleParams> ParamsList { get; set; } = new List<EQModuleParams>();
		public float OutputLevel { get; set; }
		public byte ProcessLFE { get; set; }

		public AkParameterEQFXParams() { }

		public AkParameterEQFXParams(BinaryReader binaryReader, uint size)
		{
			uint count = size / 17;
			for (int i = 0; i < count; i++)
			{
				ParamsList.Add(new EQModuleParams(binaryReader));
			}
			OutputLevel = binaryReader.ReadSingle();
			ProcessLFE = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 5 + (uint)ParamsList.Sum(p => p.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			for (int i = 0; i < ParamsList.Count;i++)
			{
				ParamsList[i].WriteToBinary(binaryWriter);
			}
			binaryWriter.Write(OutputLevel);
			binaryWriter.Write(ProcessLFE);
		}
	}

	public class EQModuleParams : WwiseObject
	{
		public uint FilterType { get; set; }
		public float Gain { get; set; }
		public float Frequency { get; set; }
		public float QFactor { get; set; }
		public byte OnOff { get; set; }

		public EQModuleParams() { }

		public EQModuleParams(BinaryReader binaryReader)
		{
			FilterType = binaryReader.ReadUInt32();
			Gain = binaryReader.ReadSingle();
			Frequency = binaryReader.ReadSingle();
			QFactor = binaryReader.ReadSingle();
			OnOff = binaryReader.ReadByte();
		}

		public uint ComputeTotalSize()
		{
			return 17;
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(FilterType);
			binaryWriter.Write(Gain);
			binaryWriter.Write(Frequency);
			binaryWriter.Write(QFactor);
			binaryWriter.Write(OnOff);
		}
	}
}

