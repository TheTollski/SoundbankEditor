using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoundbankEditor.Core.Utility
{
	// Taken from https://github.com/dotnet/runtime/issues/54189#issuecomment-861628532

	/// <summary>
	/// Json collection converter.
	/// </summary>
	/// <typeparam name="TDatatype">Type of item to convert.</typeparam>
	/// <typeparam name="TConverterType">Converter to use for individual items.</typeparam>
	public class JsonCollectionItemConverter<TDatatype, TConverterType> : JsonConverter<List<TDatatype>>
			where TConverterType : JsonConverter
	{
		/// <summary>
		/// Reads a json string and deserializes it into an object.
		/// </summary>
		/// <param name="reader">Json reader.</param>
		/// <param name="typeToConvert">Type to convert.</param>
		/// <param name="options">Serializer options.</param>
		/// <returns>Created object.</returns>
		public override List<TDatatype> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				return default(List<TDatatype>);
			}

			JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(options);
			jsonSerializerOptions.Converters.Clear();
			jsonSerializerOptions.Converters.Add(Activator.CreateInstance<TConverterType>());

			List<TDatatype> returnValue = new List<TDatatype>();

			while (reader.TokenType != JsonTokenType.EndArray)
			{
				if (reader.TokenType != JsonTokenType.StartArray)
				{
					returnValue.Add((TDatatype)JsonSerializer.Deserialize(ref reader, typeof(TDatatype), jsonSerializerOptions));
				}

				reader.Read();
			}

			return returnValue;
		}

		/// <summary>
		/// Writes a json string.
		/// </summary>
		/// <param name="writer">Json writer.</param>
		/// <param name="value">Value to write.</param>
		/// <param name="options">Serializer options.</param>
		public override void Write(Utf8JsonWriter writer, List<TDatatype> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(options);
			jsonSerializerOptions.Converters.Clear();
			jsonSerializerOptions.Converters.Add(Activator.CreateInstance<TConverterType>());

			writer.WriteStartArray();

			foreach (TDatatype data in value)
			{
				JsonSerializer.Serialize(writer, data, jsonSerializerOptions);
			}

			writer.WriteEndArray();
		}
	}
}
