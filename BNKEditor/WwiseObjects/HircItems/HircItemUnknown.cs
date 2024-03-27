using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class HircItemUnknown : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		public uint UlID { get; set; }
		public byte[]? Data { get; set; }

		public HircItemUnknown(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				Data = binaryReader.ReadBytes((int)DwSectionSize - bytesReadFromThisObject);
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);
			binaryWriter.Write(UlID);
			if (Data != null)
			{
				binaryWriter.Write(Data);
			}
		}
	}
}
