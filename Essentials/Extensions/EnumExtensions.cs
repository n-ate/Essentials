using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace n_ate.Essentials
{
    public static class Enum<T> where T : Enum
    {
        private static ConcurrentDictionary<Type, ReadOnlyDictionary<string, T>> _enumCacheCaseSensitive = new ConcurrentDictionary<Type, ReadOnlyDictionary<string, T>>();
        private static ConcurrentDictionary<Type, ReadOnlyDictionary<string, T>> _enumCacheNotCaseSensitive = new ConcurrentDictionary<Type, ReadOnlyDictionary<string, T>>();

        /// <summary>
        /// Gets the specified enum as an array of key value pairs
        /// </summary>
        public static ReadOnlyDictionary<string, T> GetEnumLookup(bool caseSensitive)
        {
            var type = typeof(T);
            var lookupSource = caseSensitive ? _enumCacheCaseSensitive : _enumCacheNotCaseSensitive;
            if (!lookupSource.TryGetValue(type, out var lookup)) //pull from cache
            {
                var values = Enum.GetValues(type).Cast<T>();
                var result = new Dictionary<string, T>(values.Count() * (caseSensitive ? 2 : 4));
                if (caseSensitive)
                {
                    foreach (var value in values)
                    {
                        var description = value.GetDescription();
                        result[description] = value;
                        var name = value.ToString();
                        if (!result.ContainsKey(name)) result[name] = value; //description attributes take precedence over names
                    }
                }
                else
                {
                    foreach (var value in values)
                    {
                        var description = value.GetDescription();
                        var lower = description.FirstCharToLower();
                        var upper = description.FirstCharToUpper();
                        result[lower] = value;
                        result[upper] = value;
                        var name = value.ToString();
                        lower = name.FirstCharToLower();
                        upper = name.FirstCharToUpper();
                        if (!result.ContainsKey(lower)) result[lower] = value; //description attributes take precedence over names
                        if (!result.ContainsKey(upper)) result[upper] = value; //description attributes take precedence over names
                    }
                }
                lookup = new ReadOnlyDictionary<string, T>(result);
                lookupSource.TryAdd(type, lookup);
            }
            return lookup;
        }
    }

    public static class EnumExtensions
    {
        private static readonly MethodInfo _getEnum = typeof(EnumExtensions).GetMethods().First(m => m.Name == nameof(GetEnum) && m.GetGenericArguments().Length == 1);

        /// <summary>
        /// Gets the declared attribute value
        /// </summary>
        public static T? GetAttributeOfType<T>(this Enum value) where T : System.Attribute
        {
            if (value == null) throw new ArgumentException($"Cannot get attribute {typeof(T).Name} of null.");
            var type = value.GetType();
            var memInfo = type.GetMember(value.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Gets the description for the specified enum value
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            if (value == null) throw new ArgumentException($"Cannot get description of null enum.");
            var attribute = value.GetAttributeOfType<DescriptionAttribute>();
            return (attribute == null ? Enum.GetName(value.GetType(), value) : attribute.Description)!;
        }

        /// <summary>
        /// Gets the matching enum description value by enumType
        /// </summary>
        public static T GetEnum<T>(this string description, bool ignoreCase = true) where T : Enum
        {
            var lookup = Enum<T>.GetEnumLookup(ignoreCase);
            return lookup[description];
        }

        /// <summary>
        /// Gets the matching enum description value by enumType
        /// </summary>
        public static object? GetEnum(this string description, Type enumType, bool ignoreCase = true)
        {
            var method = _getEnum.MakeGenericMethod(enumType);
            return method.Invoke(null, new object[] { description, ignoreCase });
        }
    }
}