using SoundbankEditor.Core.Utility;
using SoundbankEditor.Core.WwiseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditorCore.WwiseObjects.HircItems.Common
{
	public class ArgumentList : WwiseObject
	{
		public List<AkGameSync> GameSyncs { get; set; } = new List<AkGameSync>();

		public ArgumentList() { }

		public ArgumentList(BinaryReader binaryReader, uint count)
		{
			for (int i = 0; i < count; i++)
			{
				GameSyncs.Add(new AkGameSync { Group = binaryReader.ReadUInt32() });
			}
			for (int i = 0; i < count; i++)
			{
				GameSyncs[i].GroupType = binaryReader.ReadByte();
			}
		}

		public uint ComputeTotalSize()
		{
			return (uint)(5 * GameSyncs.Count);
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			for (int i = 0; i < GameSyncs.Count; i++)
			{
				binaryWriter.Write(GameSyncs[i].Group);
			}
			for (int i = 0; i < GameSyncs.Count; i++)
			{
				binaryWriter.Write(GameSyncs[i].GroupType);
			}
		}
	}

	public class AkGameSync
	{
		[JsonConverter(typeof(WwiseShortIdJsonConverter))]
		public uint Group { get; set; }
		public byte GroupType { get; set; }

		public AkGameSync() { }

		// Note: An AkGameSync is not stored as a whole in the binary. For a list, all the AkGameSync Groups are stored and then all the GroupTypes are stored.
	}
}
