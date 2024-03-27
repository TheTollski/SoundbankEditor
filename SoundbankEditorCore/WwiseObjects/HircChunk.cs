using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects
{
	public class HircChunk : WwiseRootObject
	{
		public string? Tag { get; set; }
		public List<HircItem> HircItems { get; set; } = new List<HircItem>();

		public HircChunk()
		{
		}

		public HircChunk(BinaryReader binaryReader)
		{
			Tag = Encoding.UTF8.GetString(binaryReader.ReadBytes(4));
			uint chunkSize = binaryReader.ReadUInt32();
			long position = binaryReader.BaseStream.Position;

			uint numReleasableHircItem = binaryReader.ReadUInt32();
			for (int i = 0; i < numReleasableHircItem; i++)
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
				else if (hircType == HircType.Attenuation) // 14
				{
					HircItems.Add(new CAkAttenuation(binaryReader));
				}
				else if (hircType == HircType.FxShareSet) // 18
				{
					HircItems.Add(new CAkFxShareSet(binaryReader));
				}
				else
				{
					HircItems.Add(new HircItemUnknown(binaryReader));
				}
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject != chunkSize)
			{
				throw new Exception($"Expected to read {chunkSize} bytes from HIRC chunk but {bytesReadFromThisObject} bytes were read.");
			}
		}

		public uint ComputeTotalSize()
		{
			return 12 + (uint)HircItems.Sum(hi => hi.ComputeTotalSize());
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(Tag[0]);
			binaryWriter.Write(Tag[1]);
			binaryWriter.Write(Tag[2]);
			binaryWriter.Write(Tag[3]);
			uint expectedSize = ComputeTotalSize() - 8;
			binaryWriter.Write(expectedSize);
			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write((uint)HircItems.Count);
			for (int i = 0; i < HircItems.Count; i++)
			{
				HircItems[i].WriteToBinary(binaryWriter);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != expectedSize)
			{
				throw new Exception($"Expected HIRC chunk size to be {expectedSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
