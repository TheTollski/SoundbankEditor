using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BNKEditor.WwiseObjects
{
	public interface WwiseObject
	{
		public void WriteToBinary(BinaryWriter binaryWriter);
	}
}
