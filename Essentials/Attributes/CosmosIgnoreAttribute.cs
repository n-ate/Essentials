using System;
using System.Linq;
using System.Reflection;

namespace Acuity.Essentials.Attributes
{
    /// <summary>
    /// Attribute used to decorate properties that should be ignored by Cosmos. The magic is done by checking each property with PropertyHasIgnoreAttribute().
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CosmosIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Does the property info have a CosmosIgnoreAttribute.
        /// </summary>
        public static bool DoesHaveAttribute(PropertyInfo property) => property.CustomAttributes.Any(a => a.IsOfType<CosmosIgnoreAttribute>());

        /// <summary>
        /// Does the property info NOT have a CosmosIgnoreAttribute.
        /// </summary>
        public static bool DoesNotHaveAttribute(PropertyInfo property) => !DoesHaveAttribute(property);
    }
}