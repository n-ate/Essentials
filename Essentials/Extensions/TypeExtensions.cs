using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace n_ate.Essentials
{
    /// <summary>
    /// Extension methods for types
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the fieldInfo that backs the property
        /// </summary>
        public static FieldInfo? GetBackingFieldInfo(this Type sourceType, string propertyName, bool ignoreCase = false)
        {
            FieldInfo? field = null;
            if (sourceType != null)
            {
                var flagsPublic = ignoreCase ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase : BindingFlags.Instance | BindingFlags.Public;
                var flagsNonPublic = ignoreCase ? BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase : BindingFlags.Instance | BindingFlags.NonPublic;
                var member = sourceType.GetProperty(propertyName, flagsPublic);
                if (member is null) member = sourceType.GetProperty(propertyName, flagsNonPublic);
                var fieldKey = $"<{member?.Name ?? propertyName}>";
                var fields = sourceType.GetFields(flagsNonPublic);
                field = fields.FirstOrDefault(f => f.Name.Contains(fieldKey));
            }
            return field;
        }

        /// <summary>
        /// Gets a default value for a type
        /// </summary>
        public static object? GetDefaultValue(this Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }

        /// <summary>
        /// Gets the element type of the collection if the type is a collection otherwise the type is returned
        /// </summary>
        public static Type GetInnerType(this Type type)
        {
            return type.IsCollection() ? type.GetElementType() ?? type.GenericTypeArguments.FirstOrDefault() ?? type : type;
        }

        /// <summary>
        /// Constructs a dictionary for looking up properties by their JSON name
        /// </summary>
        public static Dictionary<string, PropertyInfo> GetJsonLookup(this Type type)
        {
            return type.GetProperties().ToDictionary(p =>
            {
                var name = p.Attributes.GetAttributeOfType<JsonPropertyNameAttribute>();
                if (name == null) return p.Name.FirstCharToLower();
                else return name.Name;
            });
        }

        /// <summary>
        /// Gets the property or field type
        /// </summary>
        public static Type GetPropertyType(this Type sourceType, string name, bool ignoreCase = false)
        {
            var member = sourceType.GetValueMemberInfo(name, ignoreCase);
            if (member is PropertyInfo property) return property.PropertyType;
            if (member is FieldInfo field) return field.FieldType;
            if (member is null) throw new ArgumentException("Type has no member of specified name.", nameof(name));
            throw new Exception("Unexpected member type encountered: " + member.MemberType);
        }

        /// <summary>
        /// Gets the propertyInfo or fieldInfo
        /// </summary>
        public static MemberInfo? GetValueMemberInfo(this Type sourceType, string name, bool ignoreCase = false)
        {
            MemberInfo? member = null;
            if (sourceType != null)
            {
                var flagsPublic = ignoreCase ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase : BindingFlags.Instance | BindingFlags.Public;
                var flagsNonPublic = ignoreCase ? BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase : BindingFlags.Instance | BindingFlags.NonPublic;
                member = sourceType.GetProperty(name, flagsPublic);
                if (member == null) member = sourceType.GetField(name, flagsPublic);
                if (member == null) member = sourceType.GetProperty(name, flagsNonPublic);
                if (member == null) member = sourceType.GetField(name, flagsNonPublic);
            }
            return member;
        }

        /// <summary>
        /// Determines if the type is assignable from IEnumerable, but not string
        /// </summary>
        public static bool IsCollection(this Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}