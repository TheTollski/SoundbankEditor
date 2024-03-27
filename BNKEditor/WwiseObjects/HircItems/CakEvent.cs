using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems
{
	public class CAkEvent : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		public uint UlID { get; set; }
		public uint ActionCount { get; set; }
		public List<uint> ActionIds { get; set; } = new List<uint>();

		public CAkEvent() { }

		public CAkEvent(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			ActionCount = binaryReader.ReadUInt32();
			for (int i = 0; i < ActionCount; i++)
			{
				ActionIds.Add(binaryReader.ReadUInt32());
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CAkEvent '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (ActionCount != ActionIds.Count)
			{
				throw new Exception($"Expected CAkEvent '{UlID}' to have {ActionCount} actions but it has {ActionIds.Count}.");
			}

			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);

			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(UlID);
			binaryWriter.Write(ActionCount);
			for (int i = 0; i<ActionIds.Count; i++)
			{
				binaryWriter.Write(ActionIds[i]);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != DwSectionSize)
			{
				throw new Exception($"Expected CAkEvent '{UlID}' section size to be {DwSectionSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
