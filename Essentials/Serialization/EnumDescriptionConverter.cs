using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// Enum Description converter respects the Description attribute found on enum values for the purposes of serialization and deserialization
    /// </summary>
    public class EnumDescriptionConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var method = this.GetType().GetMethod(nameof(GetEnumConverter), BindingFlags.Instance | BindingFlags.NonPublic)!;
            var genericMethod = method.MakeGenericMethod(typeToConvert)!;
            var converter = (JsonConverter)genericMethod.Invoke(this, null)!;
            return converter;
        }

        private EnumConverter<T> GetEnumConverter<T>()
             where T : Enum
        {
            return new EnumConverter<T>();
        }
    }

    /// <summary>
    /// Used internally for deserializing enums
    /// </summary>
    internal class EnumConverter<T> : JsonConverter<T>
         where T : Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value;
            if (reader.TokenType == JsonTokenType.Number)
            {
                value = reader.GetInt32().ToString();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                value = reader.GetString()!;
                //var new = value.GetEnum<T>();
                //var old = (T)value.GetEnum(typeof(T));
                return value.GetEnum<T>();
            }
            else throw new ArgumentException("Unexpected token type encountered while deserializing enum. Type: " + reader.TokenType.GetDescription());
            return (T)value.GetEnum(typeof(T))!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<string>(writer, (value as Enum).GetDescription(), options);
        }
    }
}