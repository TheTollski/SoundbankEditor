using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.WwiseObjects
{
	[JsonDerivedType(typeof(BankHeader), "BKHD")]
	[JsonDerivedType(typeof(HircChunk), "HIRC")]
	[JsonDerivedType(typeof(StringMappingChunk), "STID")]
	public interface WwiseRootObject : WwiseObject
	{
		public WwiseRootObjectHeader Header { get; set; }
	}

	public class WwiseRootObjectHeader : WwiseObject
	{
		public string? DwTag { get; set; }
		public uint DwChunkSize { get; set; }

		public uint ComputeTotalSize()
		{
			throw new NotImplementedException();
		}

		public void WriteToBinary(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(DwTag[0]);
			binaryWriter.Write(DwTag[1]);
			binaryWriter.Write(DwTag[2]);
			binaryWriter.Write(DwTag[3]);
			binaryWriter.Write(DwChunkSize);
		}
	}

	public enum WwiseRootObjectType
	{
		BKHD,
		HIRC,
		STID,
		DIDX,
		DATA,
	}
}
