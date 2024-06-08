using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace n_ate.Essentials.Serialization
{
    /// <summary>
    /// A JSON serialization converter that auto-resolves interfaces to concrete types based on properties of the type and the JSON fields being deserialized.
    /// </summary>
    public class InterfaceAutoResolveConverter : JsonConverterFactory
    {
        /// <param name="assembliesToIndex">Specific assemblies to index, otherwise those loaded into the app domain at the time of calling will be indexed.</param>
        public InterfaceAutoResolveConverter(params Assembly[] assembliesToIndex) : base()
        {
            if (assembliesToIndex.Length == 0) assembliesToIndex = AppDomain.CurrentDomain.GetAssemblies();
            ConcreteTypes = assembliesToIndex
                .SelectMany(a => a.GetLoadableTypes().Where(t => t is not null && !t.IsInterface))
                .ToArray();
        }

        public ConcurrentDictionary<Type, Type[]> InterfaceLazyCache { get; } = new ConcurrentDictionary<Type, Type[]>();
        public Type[] ConcreteTypes { get; }

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsInterface)
            {
                var concreteTypes = InterfaceLazyCache.GetOrAdd(typeToConvert, i => ConcreteTypes.Where(t => t.IsAssignableTo(i)).ToArray());
                if (concreteTypes.Any()) return true;
            }
            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var method = this.GetType().GetMethod(nameof(GetInterfaceResolverConverter), BindingFlags.Instance | BindingFlags.NonPublic)!;
            var genericMethod = method.MakeGenericMethod(typeToConvert);
            var converter = (JsonConverter)genericMethod.Invoke(this, null)!;
            return converter;
        }

        private InterfaceResolverConverter<T> GetInterfaceResolverConverter<T>()
        {
            var interfaceType = typeof(T).GetInnerType();
            var concreteTypes = InterfaceLazyCache.GetOrAdd(interfaceType, i => ConcreteTypes.Where(t => t.IsAssignableTo(i)).ToArray());
            return new InterfaceResolverConverter<T>(concreteTypes);
        }
    }

    internal class InterfaceResolverConverter<T> : JsonConverter<T>
    {
        public Type[] ConcreteTypes { get; }

        public InterfaceResolverConverter(Type[] concreteTypes)
        {
            ConcreteTypes = concreteTypes;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //if (typeToConvert.IsCollection())
            //{
            //}
            //else
            //{
            var bestType = ConcreteTypes.First();
            if (ConcreteTypes.Length > 1)
            {
                var obj = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                if (obj is null) return default!;
                bestType = GetBestMatchingConcreteType([.. obj.Keys]);

                var json = JsonSerializer.Serialize(obj, options);
                var bytes = Encoding.UTF8.GetBytes(json);
                var span = new ReadOnlySpan<byte>(bytes);
                var reader2 = new Utf8JsonReader(span);
                var obj2 = JsonSerializer.Deserialize(ref reader2, bestType, options);
                return (T)obj2!;
            }
            else
            {
                var obj = JsonSerializer.Deserialize(ref reader, bestType, options);
                return (T)obj!;
            }
            //}
            //return default(T);
        }

        private Type GetBestMatchingConcreteType(string[] fieldNames)
        {
            if (ConcreteTypes.Length == 1) return ConcreteTypes[0];
            var scores = ConcreteTypes.Select(t => new TypeMatchScore { Type = t, FieldCount = fieldNames.Length }).ToArray();
            foreach (var score in scores)
            {
                var properties = score.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
                score.PropertyCount = properties.Length;
                foreach (var property in properties)
                {
                    if (fieldNames.Any(f => f.Equals(property.Name, StringComparison.OrdinalIgnoreCase))) score.NameMatches++;
                }
            }
            var result = scores.MaxBy(s => s.Score) ?? scores.OrderByDescending(s => s.Score).First();

            Debug.WriteLine($"Determined best matching concrete type for interface {typeof(T).Name} is {result.Type.Name} with a match score of {Math.Round(result.Score * 100, 1)}%");
            return result.Type;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var valueType = value?.GetType() ?? typeof(T);
            var modifiedOptions = new JsonSerializerOptions(options);
            modifiedOptions.Converters.Remove(modifiedOptions.Converters.First(c => c is InterfaceAutoResolveConverter));//remove this converter to prevent recursion. We don't want to handle writes.
            JsonSerializer.Serialize(writer, value, valueType, modifiedOptions);
        }

        private Type GetConcreteCollectionType(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetInnerType();
            //var knownTypes = (options.Converters.First(c => c is InterfaceResolverConverter) as InterfaceResolverConverter)!.KnownTypes;
            //knownTypes.TryGetValue(innerType, out Type? mappedType);
            var mappedType = ConcreteTypes.First();
            typeToConvert = typeToConvert.IsArray ? mappedType!.MakeArrayType() : typeToConvert.GetGenericTypeDefinition().MakeGenericType(mappedType!);//make collection type with concrete class, not the interface
            return typeToConvert;
        }

        internal class TypeMatchScore
        {
            public Type Type { get; set; }
            public int FieldCount { get; set; }
            public int PropertyCount { get; set; }
            public int NameMatches { get; set; }

            public float Score
            {
                get
                {
                    var fieldMatchPercent = NameMatches / (float)FieldCount;
                    var propertyMatchPercent = NameMatches / (float)PropertyCount;
                    return (fieldMatchPercent * 9 + propertyMatchPercent) / 10; //field match is 90% of the score.
                }
            }
        }
    }
}