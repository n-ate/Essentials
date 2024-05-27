using n_ate.Essentials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace n_ate.Essentials.PropertySets
{
    /// <summary>
    /// Attribute used to decorate properties with set names for later use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertySetAttribute : Attribute
    {
        /// <param name="setNames">The names of each set of properties. These set names may be used for serialization or other purposes. See <see cref="PropertySetExtensions"/></param>
        public PropertySetAttribute(params string[] setNames)
        {
        }

        /// <summary>
        /// Does the property have a <see cref="PropertySetAttribute"/>.
        /// </summary>
        public static bool HasAttribute(PropertyInfo property) => property.CustomAttributes.Any(attr => attr.IsOfType<PropertySetAttribute>());

        /// <summary>
        /// Does the property NOT have the designated set name.
        /// </summary>
        public static bool PropertyIsNotOfSet(PropertyInfo property, string setName) => !PropertyIsOfSet(property, setName);

        /// <summary>
        /// Does the property have the designated set name.
        /// </summary>
        public static bool PropertyIsOfSet(PropertyInfo property, string setName)
        {
            return property.CustomAttributes.Any(attr =>
            {
                var isPropertySetAttribute = attr.IsOfType<PropertySetAttribute>();
                if (isPropertySetAttribute)
                {
                    var arguments = attr.ConstructorArguments.First().Value as IEnumerable<CustomAttributeTypedArgument>;
                    var containsSetName = arguments?.Select(a => a.Value).Contains(setName) ?? false;
                    return containsSetName;
                }
                else return false;
            });
        }
    }
}