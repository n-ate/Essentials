using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace n_ate.Essentials
{
    public static class CustomAttributeDataExtensions
    {
        public static CustomAttributeData[] ByType<TAttribute>(this IEnumerable<CustomAttributeData> attributeData) where TAttribute : Attribute
        {
            return attributeData.Where(a => a.IsOfType<TAttribute>()).ToArray();
        }

        public static CustomAttributeData? FirstByType<TAttribute>(this IEnumerable<CustomAttributeData> attributeData) where TAttribute : Attribute
        {
            return attributeData.FirstOrDefault(a => a.IsOfType<TAttribute>());
        }

        public static object GetValue(this CustomAttributeData? attributeData, int index)
        {
            return attributeData.GetValues(index).First();
        }

        public static object GetValue(this CustomAttributeData? attributeData, string name)
        {
            return attributeData.GetValues(name).First();
        }

        public static TResult GetValue<TResult>(this CustomAttributeData? attributeData, int index)
        {
            return (TResult)attributeData.GetValues(index).First();
        }

        public static TResult GetValue<TResult>(this CustomAttributeData? attributeData, string name)
        {
            return (TResult)attributeData.GetValues(name).First();
        }

        public static object[] GetValues(this CustomAttributeData? attributeData, params object[] indexOrName)
        {
            if (attributeData is null) throw new ArgumentException(nameof(attributeData));
            var result = new List<object>();
            var i = 0;
            foreach (var key in indexOrName)
            {
                i++;
                if (key is int index)
                {
                    if (index >= attributeData.ConstructorArguments.Count) throw new IndexOutOfRangeException($"{nameof(indexOrName)} argument number {i} is out of range.");
                    result.Add(attributeData.ConstructorArguments[index].Value!);
                }
                else if (key is string name)
                {
                    var argument = attributeData.NamedArguments.FirstOrDefault(a => a.MemberName == name).TypedValue.Value;
                    if (argument == null) throw new ArgumentException($"Argument named {name} does not exist on custom attribute.", nameof(indexOrName));
                    result.Add(argument);
                }
            }
            return result.ToArray();
        }

        public static bool IsOfType<TAttribute>(this CustomAttributeData attributeData) where TAttribute : Attribute
        {
            return attributeData.AttributeType.IsAssignableTo(typeof(TAttribute));
        }
    }
}