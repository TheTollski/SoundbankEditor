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

	public class AkPropBundleMinMax : WwiseObject
	{
		public List<AkPropMinMax> Props { get; set; } = new List<AkPropMinMax>();

		public AkPropBundleMinMax() { }

		public AkPropBundleMinMax(BinaryReader binaryReader)
		{
			byte propCount = binaryReader.ReadByte();
			for (int i = 0; i < propCount; i++)
			{
				Props.Add(new AkPropMinMax { Id = binaryReader.ReadByte() });
			}
			for (int i = 0; i < propCount; i++)
			{
				Props[i].Min = binaryReader.ReadSingle();
				Props[i].Max = binaryReader.ReadSingle();
			}
		}

		public uint ComputeTotalSize()
		{
			return 1 + (uint)(9 * Props.Count);
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
				binaryWriter.Write(Props[i].Min);
				binaryWriter.Write(Props[i].Max);
			}
		}
	}

	public class AkPropMinMax
	{
		public byte Id { get; set; }
		public float Min { get; set; }
		public float Max { get; set; }

		public AkPropMinMax() { }

		// Note: An AkPropMinMax is not stored as a whole in the binary. For a list, all the AkProp IDs are stored and then all the values are stored.
	}
}
