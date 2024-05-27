using n_ate.Essentials;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// Uses <see cref="PropertySets.PropertySetAttribute"/>s on properties to determine which properties to serialize.
    /// </summary>
    public class PropertySetConverter : JsonConverterFactory
    {
        /// <param name="propertySet">The given property set name on the <see cref="PropertySets.PropertySetAttribute"/>.</param>
        public PropertySetConverter(string propertySet) : base()
        {
            this.PropertySet = propertySet;
        }

        public string PropertySet { get; private set; }

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
            return typeToConvert.HasPropertySet();
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetInnerType();
            var method = this.GetType().GetMethod(nameof(GetPropertySetConverter), BindingFlags.Instance | BindingFlags.NonPublic)!;
            var genericMethod = method.MakeGenericMethod(typeToConvert)!;
            var converter = (JsonConverter)genericMethod.Invoke(this, null)!;
            return converter;
        }

        private PropertySetConverter<T> GetPropertySetConverter<T>()
        {
            return new PropertySetConverter<T>(PropertySet, this);
        }
    }

    internal class PropertySetConverter<T> : JsonConverter<T>
    {
        internal PropertySetConverter(string propertySet, PropertySetConverter factory) : base()
        {
            this.PropertySet = propertySet;
            this.ConverterFactory = factory;
        }

        public PropertySetConverter ConverterFactory { get; }
        public string PropertySet { get; }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.HasPropertySet();
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            T result;
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var includeProperties = typeToConvert.GetPropertiesOfSets(PropertySet);
                result = Activator.CreateInstance<T>();
                var initialDepth = reader.CurrentDepth;
                reader.Read();
                while (reader.CurrentDepth > initialDepth)
                {
                    string propertyName = reader.GetString()!;

                    if (PropertySetConverter.TryFindMatch(includeProperties, propertyName, options.PropertyNameCaseInsensitive, out var match))
                    { //property is in the set so update
                        var value = JsonSerializer.Deserialize(ref reader, match!.PropertyType, options);
                        match.SetValue(result, value);
                        reader.Read();
                    }
                    else
                    { //move reader forward to next field
                        do
                        {
                            reader.Read();
                        }
                        while (
                            !(reader.CurrentDepth == initialDepth + 1 && reader.TokenType == JsonTokenType.PropertyName) && //break when first level property name is encountered
                            reader.CurrentDepth != initialDepth //break when everything has been read
                        );
                    }
                }
                while (reader.CurrentDepth > initialDepth) reader.Read();
            }
            else throw new JsonException("Expected start of object. Index:" + reader.TokenStartIndex);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            string? referenceId = null;
            var preserveHandler = options.ReferenceHandler as PreserveReferenceHandler;
            if (preserveHandler is not null)
            {
                referenceId = preserveHandler.GetOrCreateReferenceId(value, out var referenceIdExtant);
                if (referenceIdExtant)
                { //reference was already added, so output { $ref } object.
                    preserveHandler.WriteReferenceObject(writer, referenceId);
                    return;
                }
            }

            var includeProperties = value.GetPropertiesOfSets(PropertySet).OrderBy(p => p.Name).ToArray();
            writer.WriteStartObject();
            if (preserveHandler is not null) preserveHandler.WriteReferenceIdProperty(writer, referenceId!); //circular reference handler available, output $id property.
            foreach (var property in includeProperties)
            {
                var propertyName = (options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase).ConvertName(property.Name);
                writer.WritePropertyName(propertyName);
                JsonSerializer.Serialize(writer, property.GetValue(value), property.PropertyType, options);
            }
            writer.WriteEndObject();
        }
    }
}