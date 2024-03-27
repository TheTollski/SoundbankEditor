using BNKEditor.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects
{
	public class HircChunk : WwiseRootObject
	{
		public WwiseRootObjectHeader Header { get; set; }
		public uint NumReleasableHircItem { get; set; }
		public List<HircItem> HircItems { get; set; } = new List<HircItem>();

		public HircChunk(BinaryReader binaryReader)
		{
			Header = new WwiseRootObjectHeader
			{
				DwTag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4)),
				DwChunkSize = binaryReader.ReadUInt32(),
			};

			long position = binaryReader.BaseStream.Position;

			NumReleasableHircItem = binaryReader.ReadUInt32();

			for (int i = 0; i < NumReleasableHircItem; i++)
			{
				HircType hircType = (HircType)binaryReader.ReadByte();
				binaryReader.BaseStream.Position -= 1;

				if (hircType == HircType.Action) // 3
				{
					HircItems.Add(new CakAction(binaryReader));
				}
				else if (hircType == HircType.Event) // 4
				{
					HircItems.Add(new CAkEvent(binaryReader));
				}
				else
				{
					HircItems.Add(new HircItemUnknown(binaryReader));
				}
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < Header.DwChunkSize)
			{
				// throw?
				binaryReader.ReadBytes((int)Header.DwChunkSize - bytesReadFromThisObject);
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			Header.WriteToBinary(binaryWriter);
			binaryWriter.Write(NumReleasableHircItem);
			for (int i = 0; i < HircItems.Count; i++)
			{
				HircItems[i].WriteToBinary(binaryWriter);
			}
		}
	}
}
