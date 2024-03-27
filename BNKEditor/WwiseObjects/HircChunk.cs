using BNKEditor.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects
{
	public class HircChunk : WwiseRootObject
	{
		public WwiseRootObjectHeader Header { get; set; } = new WwiseRootObjectHeader();
		public uint NumReleasableHircItem { get; set; }
		public List<HircItem> HircItems { get; set; } = new List<HircItem>();

		public HircChunk()
		{
		}

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

				if (hircType == HircType.Sound) // 2
				{
					HircItems.Add(new CAkSound(binaryReader));
				}
				else if (hircType == HircType.Action) // 3
				{
					HircItems.Add(new CAkAction(binaryReader));
				}
				else if (hircType == HircType.Event) // 4
				{
					HircItems.Add(new CAkEvent(binaryReader));
				}
				else if (hircType == HircType.RandomSequenceContainer) // 5
				{
					HircItems.Add(new CAkRanSeqCntr(binaryReader));
				}
				else if (hircType == HircType.SwitchContainer) // 6
				{
					HircItems.Add(new CAkSwitchCntr(binaryReader));
				}
				else if (hircType == HircType.ActorMixer) // 7
				{
					HircItems.Add(new CAkActorMixer(binaryReader));
				}
				else
				{
					HircItems.Add(new HircItemUnknown(binaryReader));
				}
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < Header.DwChunkSize)
			{
				throw new Exception($"{Header.DwChunkSize - bytesReadFromThisObject} extra bytes found at the end of HIRC chunk.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (NumReleasableHircItem != HircItems.Count)
			{
				throw new Exception($"Expected HIRC chunk to have {NumReleasableHircItem} items but it has {HircItems.Count}.");
			}

			Header.WriteToBinary(binaryWriter);

			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(NumReleasableHircItem);
			for (int i = 0; i < HircItems.Count; i++)
			{
				HircItems[i].WriteToBinary(binaryWriter);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != Header.DwChunkSize)
			{
				throw new Exception($"Expected HIRC chunk size to be {Header.DwChunkSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
