using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects.HircItems.Common
{
	public class AkPropBundle : WwiseObject
	{
		public List<AkProp> Props { get; set; } = new List<AkProp>();

		public AkPropBundle() { }

		public AkPropBundle(BinaryReader binaryReader)
		{
			byte propCount = binaryReader.ReadByte();
			for (int i = 0; i < propCount; i++)
			{
				Props.Add(new AkProp { Id = binaryReader.ReadByte() });
			}
			for (int i = 0; i < propCount; i++)
			{
				Props[i].Value = binaryReader.ReadSingle();
			}
		}

		public uint ComputeTotalSize()
		{
			return 1 + (uint)(5 * Props.Count);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write((byte)Props.Count);
			for (int i = 0; i < Props.Count; i++)
			{
				binaryWriter.Write(Props[i].Id);
			}
			for (int i = 0; i < Props.Count; i++)
			{
				binaryWriter.Write(Props[i].Value);
			}
		}
	}

	public class AkProp
	{
		public byte Id { get; set; }
		public float Value { get; set; }

		public AkProp() { }

		// Note: An AkProp is not stored as a whole in the binary. For a list, all the AkProp IDs are stored and then all the values are stored.
	}
}
