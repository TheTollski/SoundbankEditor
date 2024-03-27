using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects
{
	[JsonDerivedType(typeof(BankHeader))]
	[JsonDerivedType(typeof(HircChunk))]
	[JsonDerivedType(typeof(StringMappingChunk))]
	public interface WwiseRootObject
	{
		public WwiseRootObjectHeader Header { get; set; }

		public void WriteToBinary(BinaryWriter binaryWriter);
	}

	public class WwiseRootObjectHeader
	{
		public string? DwTag { get; set; }
		public uint DwChunkSize { get; set; }

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
