using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.Utility
{
	public class WwiseShortIdJsonConverter :  JsonConverter<uint>
	{
		public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string valueString = reader.GetString();
			if (uint.TryParse(valueString.Split(" ")[0], out uint value))
			{
				return value;
			}

			if (Regex.Match(valueString, "^\\[\\w+\\]$").Success)
			{
				string name = valueString.Substring(1, valueString.Length - 2);
				WwiseShortIdUtility.AddNames(new List<string> { name }, true);

				return WwiseShortIdUtility.ConvertToShortId(name);
			}

			throw new JsonException($"Unable to convert '{valueString}' to a Wwise short ID. The value must be either 1) an unsigned integer optionally followed by a space and additional text or 2) alphanumeric characters or underscores wrapped in square brackets.");
		}

		public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
		{
			string? name = WwiseShortIdUtility.GetNameFromShortId(value);

			writer.WriteStringValue($"{value} [{name ?? "?"}]");
		}
	}
}
