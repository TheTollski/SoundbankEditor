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
		public string? Tag { get; set; }
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
