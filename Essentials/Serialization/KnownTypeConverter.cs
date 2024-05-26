using n_ate.Essentials;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// The KnownTypeConverter uses KnownTypeAttributes on a class to determine how to deserialize an interface
    /// </summary>
    public class KnownTypeConverter : JsonConverterFactory
    {
        /// <param name="assembliesToIndex">Specific assemblies to index, otherwise those loaded into the app domain at the time of calling will be indexed.</param>
        public KnownTypeConverter(params Assembly[] assembliesToIndex) : base()
        {
            if (assembliesToIndex.Length == 0) assembliesToIndex = AppDomain.CurrentDomain.GetAssemblies();
            var knownTypeAttributes = new List<(Type, Type)>();
            foreach (var assembly in assembliesToIndex)
            {
                try
                {
                    foreach (var exportedType in assembly.ExportedTypes)
                    {
                        var knownTypes = exportedType.GetCustomAttributes<KnownTypeAttribute>();
                        foreach (var known in knownTypes)
                        {
                            if (known is not null && known.Type is not null) knownTypeAttributes.Add((known.Type, exportedType));
                        }
                    }
                }
                catch { Console.WriteLine(nameof(KnownTypeConverter) + " encountered an error while trying to map assembly: " + assembly.FullName); }
            }
            this.KnownTypes = knownTypeAttributes.ToDictionary(x => x.Item1, x => x.Item2);
        }

        public Dictionary<Type, Type> KnownTypes { get; private set; }

        public override bool CanConvert(Type typeToConvert)
        {
            Type type = typeToConvert.GetInnerType();
            return KnownTypes.TryGetValue(type, out _);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetInnerType();
            var method = this.GetType().GetMethod(nameof(GetKnownTypeConverter), BindingFlags.Instance | BindingFlags.NonPublic)!;
            KnownTypes.TryGetValue(innerType, out Type? mappedType);
            var genericMethod = method.MakeGenericMethod(typeToConvert.IsCollection() ? typeToConvert : mappedType!);
            var converter = (JsonConverter)genericMethod.Invoke(this, null)!;
            return converter;
        }

        private KnownTypeConverter<T> GetKnownTypeConverter<T>()
        {
            return new KnownTypeConverter<T>();
        }
    }

    internal class KnownTypeConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsCollection())
            {
                typeToConvert = GetConcreteCollectionType(typeToConvert, options);//convert to concrete collection type
            }
            var result = JsonSerializer.Deserialize(ref reader, typeToConvert, options);
            if (typeToConvert.IsCollection())
            {
                result = (result as IEnumerable)!.CastToCollection<T>();//cast concrete collection type to expected interface collection type
            }
            return (T)result!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var valueType = value?.GetType() ?? typeof(T);
            var modifiedOptions = new JsonSerializerOptions(options);
            modifiedOptions.Converters.Remove(modifiedOptions.Converters.First(c => c is KnownTypeConverter));//remove this converter to prevent recursion. We don't want to handle writes.
            JsonSerializer.Serialize(writer, value, valueType, modifiedOptions);
        }

        private static Type GetConcreteCollectionType(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetInnerType();
            var knownTypes = (options.Converters.First(c => c is KnownTypeConverter) as KnownTypeConverter)!.KnownTypes;
            knownTypes.TryGetValue(innerType, out Type? mappedType);
            typeToConvert = typeToConvert.IsArray ? mappedType!.MakeArrayType() : typeToConvert.GetGenericTypeDefinition().MakeGenericType(mappedType!);//make collection type with concrete class, not the interface
            return typeToConvert;
        }
    }
}