using BNKEditor.Utility;
using BNKEditor.WwiseObjects.HircItems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CAkAttenuation : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
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
		public byte CurveCount { get; set; }
		public List<CAkConversionTable> Curves { get; set; } = new List<CAkConversionTable>();
		public InitialRtpc InitialRtpc { get; set; } = new InitialRtpc();

		public CAkAttenuation() { }

		public CAkAttenuation(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

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
			CurveCount = binaryReader.ReadByte();
			for (int i = 0; i < CurveCount; i++)
			{
				Curves.Add(new CAkConversionTable(binaryReader));
			}
			InitialRtpc = new InitialRtpc(binaryReader);

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CAkAttenuation '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (CurveCount != Curves.Count)
			{
				throw new Exception($"Expected CAkAttenuation '{UlID}' to have {CurveCount} curves but it has {Curves.Count}.");
			}

			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);

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
			binaryWriter.Write(CurveCount);
			for (int i = 0; i < Curves.Count; i++)
			{
				Curves[i].WriteToBinary(binaryWriter);
			}
			InitialRtpc.WriteToBinary(binaryWriter);

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != DwSectionSize)
			{
				throw new Exception($"Expected CAkAttenuation '{UlID}' section size to be {DwSectionSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}

	public class CAkConversionTable
	{
		public byte Scaling { get; set; }
		public ushort GraphPointCount { get; set; }
		public List<AkRTPCGraphPoint> GraphPoints { get; set; } = new List<AkRTPCGraphPoint>();

		public CAkConversionTable() { }

		public CAkConversionTable(BinaryReader binaryReader)
		{
			Scaling = binaryReader.ReadByte();
			GraphPointCount = binaryReader.ReadUInt16();
			for (int i = 0; i < GraphPointCount; i++)
			{
				GraphPoints.Add(new AkRTPCGraphPoint(binaryReader));
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (GraphPointCount != GraphPoints.Count)
			{
				throw new Exception($"Expected CAkConversionTable to have {GraphPointCount} graph points but it has {GraphPoints.Count}.");
			}

			binaryWriter.Write(Scaling);
			binaryWriter.Write(GraphPointCount);
			for (int i = 0; i < GraphPoints.Count; i++)
			{
				GraphPoints[i].WriteToBinary(binaryWriter);
			}
		}
	}
}
