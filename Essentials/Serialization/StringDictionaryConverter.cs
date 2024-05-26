using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// Json Converter that can convert almost anything
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StringDictionaryConverter<T> : JsonConverter<Dictionary<string, T>>
    {
        /// <summary>
        /// Read Json
        /// </summary>
        public override Dictionary<string, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var dictionary = new Dictionary<string, T>();
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    string key = reader.GetString()!;
                    var value = JsonSerializer.Deserialize<T>(ref reader, options)!;
                    dictionary.Add(key, value);
                }
                //Below is in case T is object
                var updates = new List<(string Key, T Value)>();
                foreach (var kv in dictionary)
                {
                    if (kv.Value is JsonElement)
                    {
                        var value = GetValue<T>(options, (JsonElement)(object)kv.Value)!;
                        updates.Add((kv.Key, value));
                    }
                }
                foreach (var (Key, Value) in updates)
                {
                    dictionary[Key] = Value;
                }
                return dictionary;
            }
            else throw new JsonException("Unexpected start of object. Index:" + reader.TokenStartIndex);
        }

        /// <summary>
        /// Write Json
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Dictionary<string, T> dictionary, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var keyValue in dictionary)
            {
                if (keyValue.Value?.GetType()?.Name == "JArray") throw new JsonException($"Encountered nested JArray while serializing a dictionary. Property: {keyValue.Key}. Did you intend to use Dictionary<string, object>?");
                writer.WritePropertyName(JsonEncodedText.Encode(keyValue.Key, options.Encoder));
                if (keyValue.Value == null) writer.WriteNullValue();
                else JsonSerializer.Serialize(writer, keyValue.Value, keyValue.Value.GetType(), options);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Gets the value from the JsonElement
        /// </summary>
        private static TValue? GetValue<TValue>(JsonSerializerOptions options, JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Number:
                    return (TValue)(object)value.GetDouble();

                case JsonValueKind.String:
                    return (TValue)(object)value.GetString()!;

                case JsonValueKind.Array:
                    var enumerator = value.EnumerateArray();
                    var okay = enumerator.MoveNext();
                    if (enumerator.Current.ValueKind == JsonValueKind.Object)
                        return (TValue)(object)JsonSerializer.Deserialize<Dictionary<string, TValue>[]>(value.GetRawText(), options)!;
                    else
                    {
                        var list = new List<object?>();
                        while (okay)
                        {
                            list.Add(GetValue<object>(options, enumerator.Current));
                            okay = enumerator.MoveNext();
                        }
                        return (TValue)(object)list.ToArray();
                    }//return (T)(object)JsonSerializer.Deserialize<object[]>(value.GetRawText(), options);

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return (TValue?)(object?)null;

                case JsonValueKind.False:
                    return (TValue)(object)false;

                case JsonValueKind.True:
                    return (TValue)(object)true;

                case JsonValueKind.Object:
                    return (TValue)(object)value;

                default:
                    throw new ArgumentException("Unhandled type in json deserialization");
            }
        }
    }
}