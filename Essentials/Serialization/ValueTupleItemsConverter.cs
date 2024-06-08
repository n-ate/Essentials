using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// <see cref="ValueTuple"/>s seem to ignore converters for deserialization. This converter ensures that all other converters are used when deserializing <see cref="ValueTuple"/>s.
    /// </summary>
    public class ValueTupleItemsConverter : JsonConverterFactory
    {
        public static bool TryFindMatch(PropertyInfo[] propertiesToSearch, string propertyName, bool propertyNameCaseInsensitive, out PropertyInfo? match)
        {
            if (propertyNameCaseInsensitive) propertyName = propertyName.FirstCharToLower();
            match = propertiesToSearch.FirstOrDefault(p => p.Name == propertyName);
            if (match is null && propertyNameCaseInsensitive)
            {
                match = propertiesToSearch.FirstOrDefault(p => p.Name == propertyName.FirstCharToUpper());
            }
            return match is not null;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            var canConvert = typeToConvert.IsGenericType && !typeToConvert.IsInterface && typeToConvert.GetInterfaces().Any(i => i == typeof(ITuple));
            return canConvert;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetInnerType();
            MethodInfo method = this.GetType().GetMethod(nameof(GetValueTupleItemsConverter), BindingFlags.Instance | BindingFlags.NonPublic)!;
            MethodInfo generic = method.MakeGenericMethod(typeToConvert);
            JsonConverter converter = (JsonConverter)generic.Invoke(this, null)!;
            return converter;
        }

        private ValueTupleItemsConverter<T> GetValueTupleItemsConverter<T>()
        {
            return new ValueTupleItemsConverter<T>(this);
        }
    }

    internal class ValueTupleItemsConverter<T> : JsonConverter<T>
    {
        internal ValueTupleItemsConverter(ValueTupleItemsConverter factory) : base()
        {
            this.ConverterFactory = factory;
        }

        public ValueTupleItemsConverter ConverterFactory { get; }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            T? result = default;
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var initialDepth = reader.CurrentDepth;
                reader.Read();
                while (reader.CurrentDepth > initialDepth)
                {
                    string fieldName = reader.GetString()!;
                    var field = typeToConvert.GetField(fieldName);
                    if (field is null) throw new NotImplementedException();
                    else
                    {
                        var itemValue = JsonSerializer.Deserialize(ref reader, field.FieldType, options)!;
                        result = result.SetValue(fieldName, itemValue);
                    }
                    reader.Read();
                }
                while (reader.CurrentDepth > initialDepth) reader.Read();
            }
            else throw new JsonException("Expected start of object. Index:" + reader.TokenStartIndex);
            return result!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}