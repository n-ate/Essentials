using n_ate.Essentials.PropertySets;
using System;
using System.Linq;
using System.Reflection;

namespace n_ate.Essentials
{
    public static class PropertySetExtensions
    {
        /// <summary>
        /// Gets the properties that are NOT decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setName"/>.
        /// </summary>
        /// <param name="setName">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property without the correctly declared <paramref name="setName"/>.</returns>
        /// <exception cref="ArgumentNullException">The object is null.</exception>
        public static PropertyInfo[] GetPropertiesNotOfSet(this object obj, string setName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var type = obj.GetType();
            return PropertySetSource.GetPropertiesNotOfSet(type, setName);
        }

        /// <summary>
        /// Gets the properties that are NOT decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setName"/>.
        /// </summary>
        /// <param name="setName">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property without the correctly declared <paramref name="setName"/>.</returns>
        /// <exception cref="ArgumentNullException">The type is null.</exception>
        public static PropertyInfo[] GetPropertiesNotOfSet(this Type type, string setName)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return PropertySetSource.GetPropertiesNotOfSet(type, setName);
        }

        /// <summary>
        /// Gets the properties that are decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setNames"/>.
        /// </summary>
        /// <param name="setNames">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property with the correctly declared <paramref name="setNames"/>.</returns>
        /// <exception cref="ArgumentNullException">The object is null.</exception>
        public static PropertyInfo[] GetPropertiesOfSets(this object obj, params string[] setNames)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var type = obj.GetType();
            return PropertySetSource.GetPropertiesOfSet(type, setNames);
        }

        /// <summary>
        /// Gets the properties that are decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setName"/>.
        /// </summary>
        /// <param name="setName">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property with the correctly declared <paramref name="setName"/>.</returns>
        /// <exception cref="ArgumentNullException">The type is null.</exception>
        public static PropertyInfo[] GetPropertiesOfSets(this Type type, params string[] setNames)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return PropertySetSource.GetPropertiesOfSet(type, setNames);
        }

        /// <summary>
        /// Gets the property names that are NOT decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setName"/>.
        /// </summary>
        /// <param name="setName">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property name without the correctly declared <paramref name="setName"/>.</returns>
        /// <exception cref="ArgumentNullException">The object is null.</exception>
        public static string[] GetPropertyNamesNotOfSet(this object obj, string setName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var type = obj.GetType();
            return PropertySetSource.GetPropertiesNotOfSet(type, setName).Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Gets the property names that are NOT decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setName"/>.
        /// </summary>
        /// <param name="setName">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property name without the correctly declared <paramref name="setName"/>.</returns>
        /// <exception cref="ArgumentNullException">The type is null.</exception>
        public static string[] GetPropertyNamesNotOfSet(this Type type, string setName)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return PropertySetSource.GetPropertiesNotOfSet(type, setName).Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Gets the property names that are decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setName"/>.
        /// </summary>
        /// <param name="setName">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property name with the correctly declared <paramref name="setName"/>.</returns>
        /// <exception cref="ArgumentNullException">The object is null.</exception>
        public static string[] GetPropertyNamesOfSets(this object obj, params string[] setNames)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var type = obj.GetType();
            return PropertySetSource.GetPropertiesOfSet(type, setNames).Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Gets the property names that are decorated with <see cref="PropertySetAttribute"/> and the <paramref name="setName"/>.
        /// </summary>
        /// <param name="setName">The name of the set on the property's <see cref="PropertySetAttribute"/>.</param>
        /// <returns>Each property name with the correctly declared <paramref name="setName"/>.</returns>
        /// <exception cref="ArgumentNullException">The type is null.</exception>
        public static string[] GetPropertyNamesOfSets(this Type type, params string[] setNames)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return PropertySetSource.GetPropertiesOfSet(type, setNames).Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Gets if any property is decorated with the <see cref="PropertySetAttribute"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The type is null.</exception>
        public static bool HasPropertySet(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return PropertySetSource.HasPropertySet(type);
        }

        /// <summary>
        /// Gets if any property is decorated with the <see cref="PropertySetAttribute"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The type is null.</exception>
        public static bool HasPropertySet(this object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return PropertySetSource.HasPropertySet(obj.GetType());
        }
    }
}