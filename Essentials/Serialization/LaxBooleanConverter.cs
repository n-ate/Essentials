using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// Json Converter that converts strings, ints, and booleans to booleans
    /// E.g. 0, 1, True, TRUE, "FaLSe", etc
    /// </summary>
    public class LaxBooleanConverter : JsonConverter<bool>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return base.CanConvert(typeToConvert);
        }

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int num)) return num > 0;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (bool.TryParse(reader.GetString(), out bool value)) return value;
                else if (int.TryParse(reader.GetString(), out int num)) return num > 0;
            }
            else return reader.GetBoolean();
            throw new ArgumentException($"Value, \"{reader.GetString()}\" cannot be converted to bool.");
            //return (bool)JsonSerializer.Deserialize(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
            //JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}