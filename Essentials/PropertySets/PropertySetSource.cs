using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace n_ate.Essentials.PropertySets
{
    internal static class PropertySetSource
    {
        private static ConcurrentDictionary<(Type Type, string SetName), (PropertyInfo[] Set, PropertyInfo[] Excluded)> _cache = new ConcurrentDictionary<(Type Type, string SetName), (PropertyInfo[] Set, PropertyInfo[] Excluded)>();

        internal static PropertyInfo[] GetPropertiesNotOfSet(Type type, string setName)
        {
            return FilterPropertiesBySet(type, setName).Excluded;
        }

        internal static PropertyInfo[] GetPropertiesOfSet(Type type, params string[] setNames)
        {
            if (setNames.Length == 1) return FilterPropertiesBySet(type, setNames[0]).Set;
            else return setNames.SelectMany(n => FilterPropertiesBySet(type, n).Set).Distinct().ToArray();
        }

        private static (PropertyInfo[] Set, PropertyInfo[] Excluded) FilterPropertiesBySet(Type type, string setName)
        {
            if (type.IsPrimitive) return (new PropertyInfo[0], new PropertyInfo[0]); //Primitive types can't have property sets.
            (PropertyInfo[], PropertyInfo[]) result;
            var key = (type, setName);
            if (!_cache.TryGetValue(key, out result))
            {
                var set = type.GetProperties().Where(p => PropertySetAttribute.PropertyIsOfSet(p, setName)).ToArray();
                var excluded = type.GetProperties().Where(p => PropertySetAttribute.PropertyIsNotOfSet(p, setName)).ToArray();
                result = (set, excluded);
                _cache.TryAdd(key, result);
            }
            return result;
        }

        internal static bool HasPropertySet(Type type)
        {
            if (type.IsPrimitive) return false; //Primitive types can't have property sets.
            return type.GetProperties().Any(p => PropertySetAttribute.HasAttribute(p));
        }
    }
}