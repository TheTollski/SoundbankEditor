using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects.HircItems.Common
{
	public class AkPropBundle : WwiseObject
	{
		public byte PropCount { get; set; }
		public List<AkProp> Props { get; set; } = new List<AkProp>();

		public AkPropBundle() { }

		public AkPropBundle(BinaryReader binaryReader)
		{
			PropCount = binaryReader.ReadByte();
			for (int i = 0; i < PropCount; i++)
			{
				Props.Add(new AkProp { Id = binaryReader.ReadByte() });
			}
			for (int i = 0; i < PropCount; i++)
			{
				Props[i].Value = binaryReader.ReadSingle();
			}
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			if (PropCount != Props.Count)
			{
				throw new Exception($"Expected AkPropBundle to have {PropCount} props but it has {Props.Count}.");
			}

			binaryWriter.Write(PropCount);
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
