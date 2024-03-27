using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.Utility
{
	public class WwiseShortIdJsonConverter :  JsonConverter<uint>
	{
		public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string valueString = reader.GetString();
			if (!uint.TryParse(valueString.Split(" ")[0], out uint value))
			{
				throw new Exception($"Unable to convert '{valueString}' to a Wwise short ID. The value must be an unsigned integer optionally followed by a space and additional text.");
			}

			return value;
		}

		public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
		{
			string? name = WwiseShortIdUtility.KnownShortIdsMap.ContainsKey(value)
				? WwiseShortIdUtility.KnownShortIdsMap[value]
				: "?";

			writer.WriteStringValue($"{value} [{name}]");
		}
	}
}
