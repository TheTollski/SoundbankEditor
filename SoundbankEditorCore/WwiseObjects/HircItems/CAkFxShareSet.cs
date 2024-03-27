using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems
{
	public class CAkFxShareSet : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint UlID { get; set; }
		public uint FxId { get; set; }
		public uint Size { get; set; }
		public AkParameterEQFXParams AkParameterEQFXParams { get; set; } = new AkParameterEQFXParams();
		public byte NumBankData { get; set; }
		public List<object> Media { get; set; } = new List<object> { };
		public InitialRtpc InitialRtpc { get; set; } = new InitialRtpc();
		public ushort NumInit { get; set; }
		public List<object> RtpcInit { get; set; } = new List<object>();

		public CAkFxShareSet() { }

		public CAkFxShareSet(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();
			FxId = binaryReader.ReadUInt32();
			Size = binaryReader.ReadUInt32();
			AkParameterEQFXParams = new AkParameterEQFXParams(binaryReader, Size);
			NumBankData = binaryReader.ReadByte();
			if (NumBankData > 0)
			{
				throw new Exception("CAkFxShareSet.Media is not supported.");
			}
			InitialRtpc = new InitialRtpc(binaryReader);
			NumInit = binaryReader.ReadUInt16();
			if (NumInit > 0)
			{
				throw new Exception("CAkFxShareSet.RtpcInit is not supported.");
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CAkFxShareSet '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (NumBankData != Media.Count)
			{
				throw new Exception($"Expected CAkFxShareSet '{UlID}' to have {NumBankData} bank data but it has {Media.Count}.");
			}
			if (NumInit != RtpcInit.Count)
			{
				throw new Exception($"Expected CAkFxShareSet '{UlID}' to have {NumInit} RTPC init items but it has {RtpcInit.Count}.");
			}

			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);

			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(UlID);
			binaryWriter.Write(FxId);
			binaryWriter.Write(Size);
			AkParameterEQFXParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(NumBankData);
			if (NumBankData > 0)
			{
				throw new Exception("CAkFxShareSet.Media is not supported.");
			}
			InitialRtpc.WriteToBinary(binaryWriter);
			binaryWriter.Write(NumInit);
			if (NumInit > 0)
			{
				throw new Exception("CAkFxShareSet.RtpcInit is not supported.");
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != DwSectionSize)
			{
				throw new Exception($"Expected CAkFxShareSet '{UlID}' section size to be {DwSectionSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class AkParameterEQFXParams
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

	public class EQModuleParams
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

