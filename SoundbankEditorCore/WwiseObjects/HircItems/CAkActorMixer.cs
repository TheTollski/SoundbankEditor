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
	public class CAkActorMixer : HircItem
	{
		public HircType EHircType { get; set; }
		public uint DwSectionSize { get; set; }
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint UlID { get; set; }
		public NodeBaseParams NodeBaseParams { get; set; } = new NodeBaseParams();
		public uint ChildCount { get; set; }
		[JsonConverter(typeof(JsonCollectionItemConverter<uint, WwiseShortIdJsonConverter>))]
		public List<uint> ChildIds { get; set; } = new List<uint>();

		public CAkActorMixer() { }

		public CAkActorMixer(BinaryReader binaryReader)
		{
			EHircType = (HircType)binaryReader.ReadByte();
			DwSectionSize = binaryReader.ReadUInt32();

			long position = binaryReader.BaseStream.Position;

			UlID = binaryReader.ReadUInt32();

			NodeBaseParams = new NodeBaseParams(binaryReader);
			ChildCount = binaryReader.ReadUInt32();
			for (int i = 0; i < ChildCount; i++)
			{
				ChildIds.Add(binaryReader.ReadUInt32());
			}

			int bytesReadFromThisObject = (int)(binaryReader.BaseStream.Position - position);
			if (bytesReadFromThisObject < DwSectionSize)
			{
				throw new Exception($"{DwSectionSize - bytesReadFromThisObject} extra bytes found at the end of CAkActorMixer '{UlID}'.");
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (ChildCount != ChildIds.Count)
			{
				throw new Exception($"Expected CAkActorMixer '{UlID}' to have {ChildCount} children but it has {ChildIds.Count}.");
			}

			binaryWriter.Write((byte)EHircType);
			binaryWriter.Write(DwSectionSize);

			long position = binaryWriter.BaseStream.Position;

			binaryWriter.Write(UlID);
			NodeBaseParams.WriteToBinary(binaryWriter);
			binaryWriter.Write(ChildCount);
			for (int i = 0; i < ChildIds.Count; i++)
			{
				binaryWriter.Write(ChildIds[i]);
			}

			int bytesWrittenFromThisObject = (int)(binaryWriter.BaseStream.Position - position);
			if (bytesWrittenFromThisObject != DwSectionSize)
			{
				throw new Exception($"Expected CAkActorMixer '{UlID}' section size to be {DwSectionSize} but it was {bytesWrittenFromThisObject}.");
			}
		}
	}
}
