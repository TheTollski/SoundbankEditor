using SoundbankEditor.Core.Utility;
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
	public class CAkAttenuation : HircItem
	{
		private const HircType EXPECTED_HIRC_TYPE = HircType.Attenuation;

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
		public byte IsConeEnabled{ get; set; }
		public sbyte CurveToUse0{ get; set; }
		public sbyte CurveToUse1 { get; set; }
		public sbyte CurveToUse2 { get; set; }
		public sbyte CurveToUse3 { get; set; }
		public sbyte CurveToUse4 { get; set; }
		public sbyte CurveToUse5 { get; set; }
		public sbyte CurveToUse6 { get; set; }
		public List<CAkConversionTable> Curves { get; set; } = new List<CAkConversionTable>();
		public InitialRtpc InitialRtpc { get; set; } = new InitialRtpc();

		public CAkAttenuation()
		{
			EHircType = EXPECTED_HIRC_TYPE;
		}

		public CAkAttenuation(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			uint sectionSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;
			UlID = binaryReader.ReadUInt32();

			IsConeEnabled = binaryReader.ReadByte();
			CurveToUse0 = binaryReader.ReadSByte();
			CurveToUse1 = binaryReader.ReadSByte();
			CurveToUse2 = binaryReader.ReadSByte();
			CurveToUse3 = binaryReader.ReadSByte();
			CurveToUse4 = binaryReader.ReadSByte();
			CurveToUse5 = binaryReader.ReadSByte();
			CurveToUse6 = binaryReader.ReadSByte();
			byte curveCount = binaryReader.ReadByte();
			for (int i = 0; i < curveCount; i++)
			{
				Curves.Add(new CAkConversionTable(binaryReader));
			}
			InitialRtpc = new InitialRtpc(binaryReader);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != sectionSize)
			{
				throw new Exception($"Expected to read {sectionSize} bytes from CAkAttenuation '{UlID}' but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 18 + (uint)Curves.Sum(c => c.ComputeTotalSize()) + InitialRtpc.ComputeTotalSize();
		}

		public List<string> GetKnownValidationErrors(SoundBank soundbank)
		{
			var knownValidationErrors = new List<string>();

			// Validate UlID
			int hircItemsWithMatchingIdCount = soundbank.HircItems.Count(hi => hi.UlID == UlID);
			if (hircItemsWithMatchingIdCount != 1)
			{
				knownValidationErrors.Add($"CAkAttenuation '{UlID}' has the same ID as {hircItemsWithMatchingIdCount - 1} other HIRC item{(hircItemsWithMatchingIdCount == 1 ? "" : "s")}.");
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

			binaryWriter.Write(IsConeEnabled);
			binaryWriter.Write(CurveToUse0);
			binaryWriter.Write(CurveToUse1);
			binaryWriter.Write(CurveToUse2);
			binaryWriter.Write(CurveToUse3);
			binaryWriter.Write(CurveToUse4);
			binaryWriter.Write(CurveToUse5);
			binaryWriter.Write(CurveToUse6);
			binaryWriter.Write((byte)Curves.Count);
			for (int i = 0; i < Curves.Count; i++)
			{
				Curves[i].WriteToBinary(binaryWriter);
			}
			InitialRtpc.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new SerializationException($"Expected CAkAttenuation '{UlID}' section size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class CAkConversionTable : WwiseObject
	{
		public byte Scaling { get; set; }
		public List<AkRTPCGraphPoint> GraphPoints { get; set; } = new List<AkRTPCGraphPoint>();

		public CAkConversionTable() { }

		public CAkConversionTable(BinaryReader binaryReader)
		{
			Scaling = binaryReader.ReadByte();
			ushort graphPointCount = binaryReader.ReadUInt16();
			for (int i = 0; i < graphPointCount; i++)
			{
				GraphPoints.Add(new AkRTPCGraphPoint(binaryReader));
			}
		}

		public uint ComputeTotalSize()
		{
			return 3 + (uint)GraphPoints.Sum(gp => gp.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(Scaling);
			binaryWriter.Write((ushort)GraphPoints.Count);
			for (int i = 0; i < GraphPoints.Count; i++)
			{
				GraphPoints[i].WriteToBinary(binaryWriter);
			}
		}
	}
}
