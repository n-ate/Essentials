using n_ate.Essentials.Enumerations;
using n_ate.Essentials.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace n_ate.Essentials
{
    /// <summary>
    /// Generic extension methods for objects
    /// </summary>
    public static class ObjectExtension
    {
        public static readonly MethodInfo MapTo2Method;
        public static readonly MethodInfo MapToMethod;
        private static readonly string defaultMessage = new string('*', 20000);
        private static readonly JsonSerializerOptions deserializeOptions;

        static ObjectExtension()
        {
            MapToMethod = typeof(ObjectExtension).GetMethod(nameof(MapTo), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object), typeof(bool), typeof(string[]) }, null)!;
            MapTo2Method = typeof(ObjectExtension).GetMethod(nameof(MapTo2), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object), typeof(bool), typeof(string[]) }, null)!;
            deserializeOptions = new JsonSerializerOptions();
            deserializeOptions.Converters.Add(new KnownTypeConverter()); //not great that this is initialized in a static class.. It should do though
        }

        /// <summary>
        /// Simple reflection based aggregation that has the option to ignore members by name
        /// </summary>
        public static T AggregateFrom<T>(this T target, T source, string deliminator = "; ")
        {
            //if (source == null) throw new ArgumentNullException(nameof(source));
            //if (target == null) throw new ArgumentNullException(nameof(target));
            if (target == null || source == null) return source;
            if (target.Equals(source)) return source;

            var type = typeof(T);
            var propertyTypeInfo = type.GetTypeInfo();
            var isDictionary = type.GetInterfaces().Contains(typeof(IDictionary));
            if (type.IsValueType || type == typeof(string))
            { //Value Types//
                object src = source;
                object aggregateValue;
                switch (target)
                {
                    case string: aggregateValue = $"{target}{deliminator}{src}"; break;
                    case Int16 int16: aggregateValue = int16 + (Int16)src; break;
                    case Int32 int32: aggregateValue = int32 + (Int32)src; break;
                    case Int64 int64: aggregateValue = int64 + (Int64)src; break;
                    case UInt16 uint16: aggregateValue = uint16 + (UInt16)src; break;
                    case UInt32 uint32: aggregateValue = uint32 + (UInt32)src; break;
                    case UInt64 uint64: aggregateValue = uint64 + (UInt64)src; break;
                    default: throw new NotImplementedException();
                }
                return (T)aggregateValue;
            }
            else if (source is IDictionary sourceDictionary && target is IDictionary targetDictionary)
            {
                foreach (var key in sourceDictionary.Keys)
                {
                    if (targetDictionary.Contains(key))
                    {
                        var sourceType = sourceDictionary[key]!.GetType();
                        var targetType = targetDictionary[key]!.GetType();
                        if (sourceType.IsAssignableTo(targetType))
                        {
                            targetDictionary[key] = AggregateFrom(targetDictionary[key], sourceDictionary[key]);
                        }
                        //else do nothing?
                    }
                    else
                    {
                        targetDictionary.Add(key, sourceDictionary[key]);
                    }
                }
            }
            else if (propertyTypeInfo.ImplementedInterfaces.Any(t => t == typeof(IEnumerable)))
            { //Arrays, Lists, IEnumerables, Collections, etc//
                var tgt = (IEnumerable)target;
                var src = (IEnumerable)source;
                var genericEnumerableType = propertyTypeInfo.ImplementedInterfaces.First(ti => (ti.Name == "IEnumerable`1"));
                var genericType = (genericEnumerableType as TypeInfo)!.GenericTypeArguments.First();
                var genericListType = typeof(List<>).MakeGenericType(genericType);
                var constructor = genericListType.GetConstructor(new Type[0])!;
                var list = (IList)constructor.Invoke(null);
                foreach (var t in tgt) list.Add(t);
                foreach (var s in src) list.Add(s);
                return (T)list;
            }
            else if (type.IsClass)
            { //Reference Types i.e. Objects//
                foreach (var property in type.GetProperties())
                {
                    if (property.CanWrite)
                    {
                        var sourcePropertyValue = property.GetValue(source, null);
                        var targetPropertyValue = property.GetValue(target, null);
                        var aggregatePropertyValue = targetPropertyValue.AggregateFrom(sourcePropertyValue);
                        property.SetValue(target, aggregatePropertyValue, null);
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// Calls a generic constructor with a runtime type.
        /// </summary>
        public static dynamic CallGenericConstructor(this Type constructorObjectType, params object[] arguments)
        {
            var argTypes = Type.GetTypeArray(arguments);
            var constructor = constructorObjectType.GetConstructors().FirstOrDefault(c =>
            {
                var properties = (c.ReflectedType as TypeInfo)?.DeclaredProperties?.ToArray();
                if (properties == null || properties.Length != argTypes.Length) return false;
                for (var i = 0; i < properties.Length; i++) if (properties[i].PropertyType.IsAssignableFrom(argTypes[i])) return false;
                return true;
            });
            if (constructor is null) throw new EntryPointNotFoundException($"Type does not contain a constructor with specified arguments. Type: {constructorObjectType.FullName}");
            dynamic result = constructor.Invoke(arguments);
            return result;
        }

        /// <summary>
        /// Calls a generic method with a runtime type.
        /// </summary>
        public static TReturn? CallInstanceGenericMethod<TReturn>(this object methodObject, string methodName, Type[] genericTypes, object[] arguments)
        {
            dynamic? result = methodObject.CallInstanceGenericMethod(methodName, genericTypes, arguments);
            return (TReturn?)result;
        }

        /// <summary>
        /// Calls a generic method with a runtime type.
        /// </summary>
        public static dynamic? CallInstanceGenericMethod(this object methodObject, string methodName, Type[] genericTypes, object[] arguments)
        {
            var convertMethod = methodObject.GetType().GetMethod(methodName);
            if (convertMethod is null) throw new ArgumentException($"Method of specified name was not found: {methodName}");
            var typedConvertMethod = convertMethod.MakeGenericMethod(genericTypes);
            dynamic? result = typedConvertMethod.Invoke(methodObject, arguments);
            //if (result is Task task) result = task.GetValueOrNull();
            return result;
        }

        /// <summary>
        /// Calls a generic method with a runtime type.
        /// </summary>
        public static dynamic? CallStaticGenericMethod(this Type methodObjectType, string methodName, Type[] genericTypes, object[] arguments)
        {
            //var argTypes = Type.GetTypeArray(arguments);
            var methods = methodObjectType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var nameMatches = methods.Where(m => m.Name == methodName).ToArray();
            if (!nameMatches.Any())
            {
                methods = methodObjectType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                nameMatches = methods.Where(m => m.Name == methodName).ToArray();
            }
            if (!nameMatches.Any()) throw new ArgumentException($"Could not find method: {methodName}");
            else if (nameMatches.Length > 1) throw new ArgumentException($"Found {nameMatches.Length} matching methods for {methodName}. Expected 1 match.");
            var typedConvertMethod = methods.First().MakeGenericMethod(genericTypes);
            dynamic? result = typedConvertMethod.Invoke(null, arguments);
            if (result is Task task) result = task.GetValueOrNull();
            return result;
        }

        /// <summary>
        /// Determines if any of the values are comparable
        /// </summary>
        public static T? GetComparable<T>(this T thisValue, T[] values, out string message)
        {
            string mostAlikeMessage = defaultMessage;
            var result = values.FirstOrDefault(v =>
            {
                bool same = thisValue.IsComparable(v, out string message);
                if (message.Length < mostAlikeMessage.Length) mostAlikeMessage = message;
                return same;
            });
            message = mostAlikeMessage;
            return result;
        }

        /// <summary>
        /// Calls a generic constructor with a runtime type.
        /// </summary>
        public static Type GetType(this Assembly assembly, string typeName, params Type[] genericTypes)
        {
            if (!typeName.Contains('`')) typeName = $"{typeName}`{genericTypes.Length}"; //adds generic numeral to name for searching..
            Type? rawType = assembly.GetType(typeName, false, false);
            if (rawType == null) rawType = assembly.GetType(typeName, false, true);
            if (rawType == null)
            {
                var allTypes = assembly.GetTypes();
                var names = allTypes.Select(t => t.Name).ToArray();
                var nameMatches = allTypes.Where(t => t.Name == typeName).ToArray();
                //var genericTypeMatches = nameMatches.Where(t=>
                //{
                //    if (t.GenericTypeParameters.Length != genericTypes.Length) return false;
                //    for(var i=0; i<genericTypes.Length; i++) if(genericTypes[i] != t.GenericTypeParameters[i]) return false;
                //    return true;
                //}).ToArray();
                if (nameMatches.Length == 0) throw new ArgumentException($"Unable to located type, {typeName} in assembly, {assembly.FullName}. Expected 1 matching type.");
                else if (nameMatches.Length > 1) throw new ArgumentException($"Multiple types located for type, {typeName} in assembly, {assembly.FullName}. Expected 1 matching type.");
                rawType = nameMatches[0];
            }
            var result = rawType.MakeGenericType(genericTypes);
            return result;
        }

        /// <summary>
        /// Gets the property or field value by name
        /// </summary>
        public static object? GetValue(this object source, string name, bool ignoreCase = false)
        {
            if (source != null)
            {
                var member = source.GetType().GetValueMemberInfo(name, ignoreCase);
                if (member is PropertyInfo property) return property.GetValue(source);
                if (member is FieldInfo field) return field.GetValue(source);
            }
            return null;
        }

        /// <summary>
        /// Gets the property or field value by name
        /// </summary>
        public static T? GetValue<T>(this object source, string name, bool ignoreCase = false)
        {
            if (source != null)
            {
                Type type = typeof(object);
                object? result;
                var member = source.GetType().GetValueMemberInfo(name, ignoreCase);
                if (member is PropertyInfo property)
                {
                    type = property.PropertyType;
                    result = property.GetValue(source);
                }
                else if (member is FieldInfo field)
                {
                    type = field.FieldType;
                    result = field.GetValue(source);
                }
                else return default;
                if (type.IsAssignableTo(typeof(T))) return (T?)result;
            }
            return default;
        }

        /// <summary>
        /// Determines if the object has a value member of the specified memberName
        /// </summary>
        public static bool HasValueMember(this object source, string memberName)
        {
            if (source is IDictionary<string, object> dictionary) return dictionary.ContainsKey(memberName);
            else return source.GetType().GetProperty(memberName) != null;
        }

        /// <summary>
        /// Determines if the type is assignable from IEnumerable, but not string
        /// </summary>
        public static bool IsCollection(this Object obj)
        {
            return obj.GetType().IsCollection();
        }

        /// <summary>
        /// Determines if the values are comparable
        /// </summary>
        public static bool IsComparable<T>(this T thisValue, T value)
        {
            return thisValue.IsComparable(value, out _);
        }

        /// <summary>
        /// Determines if the values are comparable
        /// </summary>
        public static bool IsComparable<T>(this T thisValue, T value, out string message)
        {
            var log = new StringBuilder();
            var result = IsComparable(typeof(T), thisValue, value, log);
            message = log.ToString();
            return result;
        }

        /// <summary>
        /// Determines if this is comparable to any of the values
        /// </summary>
        public static bool IsComparable<T>(this T thisValue, T[] values, out string message)
        {
            var comparable = thisValue.GetComparable(values, out message);
            return comparable != null && !comparable.Equals(default(T));
        }

        /// <summary>
        /// Determines if the object is null or if a collection of some type that the collection is empty.
        /// </summary>
        public static bool IsNullOrEmpty(this object? obj)
        {
            if (obj is string s) return string.IsNullOrEmpty(s);
            return obj == null || (typeof(IEnumerable).IsAssignableFrom(obj.GetType()) && !(obj as IEnumerable)!.GetEnumerator().MoveNext());
        }

        /// <summary>
        /// Maps this object to the specified return type
        /// </summary>
        /// <typeparam name="TReturn">The return type to map to</typeparam>
        /// <param name="ignoreMembers">Member names to ignore when mapping</param>
        public static TReturn? MapTo<TReturn>(this object? source, params string[] ignoreMembers)
            where TReturn : new()
        {
            if (source == null) return default;
            if (typeof(TReturn).IsAssignableFrom(typeof(Dictionary<string, object>))) return (TReturn)(object)source.ToDictionary(Capitalization.None, ignoreMembers);
            return source.MapTo<TReturn>(false, ignoreMembers);
        }

        /// <summary>
        /// Maps this object to the specified return type
        /// </summary>
        /// <typeparam name="TReturn">The return type to map to</typeparam>
        /// <param name="alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch">if true, a new object will be created even if source and destination types match</param>
        /// <param name="ignoreMembers">Member names to ignore when mapping</param>
        public static TReturn? MapTo<TReturn>(this object? source, bool alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch, params string[] ignoreMembers)
            where TReturn : new()
        {
            if (source == null) return default;
            var destination = new TReturn();
            var destinationType = destination.GetType();
            var sourceType = source.GetType();
            var sourceIsDictionary = sourceType.IsAssignableTo(typeof(IDictionary<string, object>));
            if (!alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch && destinationType == sourceType) return (TReturn)source;
            //TODO: ...
            //JsonElement? src = (source is JsonElement element) ? element : (JsonElement?)null;
            //if (src.HasValue)
            if (source is JsonElement jsonSource)
            {
                return JsonSerializer.Deserialize<TReturn>(jsonSource.ToString(), deserializeOptions);
            }
            foreach (var destinationProperty in destinationType.GetProperties())
            {
                if (!ignoreMembers.Contains(destinationProperty.Name))
                {
                    if (sourceIsDictionary)
                    { //handle dictionary
                        var dictionary = (source as IDictionary<string, object>)!;
                        var hasValue = dictionary.ContainsKey(destinationProperty.Name);
                        if (hasValue)
                        {
                            var rawValue = dictionary[destinationProperty.Name];
                            if (rawValue.TryChangeType(destinationProperty.PropertyType, out object? value))
                            {
                                destinationProperty.SetValue(destination, value, null);
                            }
                            else throw new ArgumentException($"Cannot assign dictionary key {destinationProperty.Name} of type {rawValue.GetType().Name} to destination property {destinationProperty.Name} of type {destinationProperty.PropertyType.Name}. Make the 2 types the same or add an ignoreMember value for {destinationProperty.Name}.");
                        }
                    }
                    else
                    {
                        var sourceProperty = sourceType.GetProperty(destinationProperty.Name);
                        //if (src.HasValue)
                        //{ //handle json
                        //    var name = destinationProperty.Name.Substring(0, 1).ToLower() + destinationProperty.Name[1..]; //var name = destinationProperty.GetJsonName();
                        //    var property = src.Value.EnumerateObject().FirstOrDefault(p => p.NameEquals(name) || p.NameEquals(destinationProperty.Name));
                        //    object? value = default(JsonProperty);
                        //    if (property.Value.ValueKind == JsonValueKind.Array)
                        //    {
                        //        if (destinationProperty.PropertyType.GetInterface(nameof(IEnumerable)) == null) throw new ArgumentException("Array kind does not implement IEnumberable interface");
                        //        var itemType = destinationProperty.PropertyType.GenericTypeArguments[0];
                        //        var arrayResults = new List<object?>();
                        //        foreach (var item in property.Value.EnumerateArray())
                        //        {
                        //            var mapTo = MapToMethod.MakeGenericMethod(itemType);
                        //            var itemResult = mapTo.Invoke(null, new object[] { item, alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch, ignoreMembers });
                        //            arrayResults.Add(itemResult);
                        //        }
                        //        var list = arrayResults.CastToList(itemType);
                        //        if (destinationProperty.PropertyType.IsAssignableFrom(list.GetType())) value = list;
                        //        else value = list.ToBoxedArray();
                        //    }
                        //    else if (property.Value.ValueKind == JsonValueKind.Object)
                        //    {
                        //        var mapTo = MapToMethod.MakeGenericMethod(destinationProperty.PropertyType);
                        //        value = mapTo.Invoke(null, new object[] { property.Value, alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch, ignoreMembers });//not tested
                        //        //throw new NotImplementedException("object kind not implemented.");
                        //    }
                        //    else
                        //    {
                        //        value = TypeDescriptor.GetConverter(destinationProperty.PropertyType).ConvertFromInvariantString(property.Value.ToString());// Convert.ChangeType(property.Value.ToString(), destinationProperty.PropertyType);
                        //    }
                        //    if (!value!.Equals(default(JsonProperty))) destinationProperty.SetValue(destination, value, null);
                        //}
                        //else
                        if (sourceProperty != null && sourceProperty.CanWrite)
                        { //handle object
                            var rawValue = sourceProperty.GetValue(source, null);
                            var propertyType = destinationProperty.PropertyType;
                            if (sourceProperty.PropertyType == destinationProperty.PropertyType)
                            {
                                destinationProperty.SetValue(destination, rawValue, null);
                            }
                            else if (rawValue!.TryChangeType(propertyType, out object? value))
                            {
                                destinationProperty.SetValue(destination, value, null);
                            }
                            else if (rawValue == null || rawValue!.ToString() == "")
                            {
                                if (propertyType.IsValueType) destinationProperty.SetValue(destination, Activator.CreateInstance(propertyType), null);
                                else destinationProperty.SetValue(destination, null, null);
                            }
                            else throw new NotImplementedException();
                        }
                    }
                }
            }
            return destination;
        }

        /// <summary>
        /// Simple reflection based mapper that has the option to ignore members by name
        /// </summary>
        public static TReturn? MapTo<TReturn>(this object? source, TReturn destination, params string[] ignoreMembers)
        {
            if (source == null) return default;
            var destinationType = destination!.GetType();
            var sourceType = source.GetType();
            var sourceIsDictionary = sourceType.IsAssignableTo(typeof(IDictionary<string, object>));
            if (destination.Equals(source)) return destination;

            foreach (var destinationProperty in destinationType.GetProperties())
            {
                if (!ignoreMembers.Contains(destinationProperty.Name))
                {
                    if (sourceIsDictionary)
                    { //handle dictionary
                        var dictionary = (source as IDictionary<string, object>)!;
                        var hasValue = dictionary.ContainsKey(destinationProperty.Name);
                        if (hasValue)
                        {
                            var rawValue = dictionary[destinationProperty.Name];
                            if (rawValue.TryChangeType(destinationProperty.PropertyType, out object? value))
                            {
                                destinationProperty.SetValue(destination, value, null);
                            }
                            else throw new NotImplementedException();
                        }
                    }
                    else
                    { //handle object
                        var sourceProperty = sourceType.GetProperty(destinationProperty.Name);
                        if (sourceProperty != null && sourceProperty.CanWrite)
                        {
                            var rawValue = sourceProperty.GetValue(source, null);
                            if (sourceProperty.PropertyType == destinationProperty.PropertyType)
                            {
                                destinationProperty.SetValue(destination, rawValue, null);
                            }
                            else if (rawValue!.TryChangeType(destinationProperty.PropertyType, out object? value))
                            {
                                destinationProperty.SetValue(destination, value, null);
                            }
                            else throw new NotImplementedException();
                        }
                    }
                }
            }
            return destination;
        }

        /// <summary>
        /// Maps this object to the specified return type
        /// </summary>
        /// <typeparam name="TReturn">The return type to map to</typeparam>
        /// <param name="includeMembers">Member names to ignore when mapping</param>
        public static TReturn? MapTo2<TReturn>(this object? source, params string[] includeMembers)
            where TReturn : new()
        {
            if (source == null) return default;
            if (typeof(TReturn).IsAssignableFrom(typeof(Dictionary<string, object>))) return (TReturn)(object)source.ToDictionary2(Capitalization.None, includeMembers);
            return source.MapTo2<TReturn>(false, includeMembers);
        }

        /// <summary>
        /// Maps this object to the specified return type
        /// </summary>
        /// <typeparam name="TReturn">The return type to map to</typeparam>
        /// <param name="alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch">if true, a new object will be created even if source and destination types match</param>
        /// <param name="ignoreMembers">Member names to ignore when mapping</param>
        public static TReturn? MapTo2<TReturn>(this object? source, bool alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch, params string[] includeMembers)
            where TReturn : new()
        {
            if (source == null) return default;
            var destination = new TReturn();
            var destinationType = destination.GetType();
            var sourceType = source.GetType();
            var sourceIsDictionary = sourceType.IsAssignableTo(typeof(IDictionary<string, object>));
            if (!alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch && destinationType == sourceType) return (TReturn)source;
            //JsonElement? src = (source is JsonElement element) ? element : (JsonElement?)null;
            //if (src.HasValue)
            if (source is JsonElement jsonSource)
            {
                return JsonSerializer.Deserialize<TReturn>(jsonSource.ToString(), deserializeOptions);
            }
            foreach (var destinationProperty in destinationType.GetProperties())
            {
                if (includeMembers.Contains(destinationProperty.Name))
                {
                    if (sourceIsDictionary)
                    { //handle dictionary
                        var dictionary = (source as IDictionary<string, object>)!;
                        var hasValue = dictionary.ContainsKey(destinationProperty.Name);
                        if (hasValue)
                        {
                            var rawValue = dictionary[destinationProperty.Name];
                            if (rawValue.TryChangeType(destinationProperty.PropertyType, out object? value))
                            {
                                destinationProperty.SetValue(destination, value, null);
                            }
                            else throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        var sourceProperty = sourceType.GetProperty(destinationProperty.Name);
                        //if (src.HasValue)
                        //{ //handle json
                        //    var name = destinationProperty.Name.Substring(0, 1).ToLower() + destinationProperty.Name[1..]; //var name = destinationProperty.GetJsonName();
                        //    var property = src.Value.EnumerateObject().FirstOrDefault(p => p.NameEquals(name) || p.NameEquals(destinationProperty.Name));
                        //    object? value = default(JsonProperty);
                        //    if (property.Value.ValueKind == JsonValueKind.Array)
                        //    {
                        //        if (destinationProperty.PropertyType.GetInterface(nameof(IEnumerable)) == null) throw new ArgumentException("Array kind does not implement IEnumberable interface");
                        //        var itemType = destinationProperty.PropertyType.GenericTypeArguments[0];
                        //        var arrayResults = new List<object?>();
                        //        foreach (var item in property.Value.EnumerateArray())
                        //        {
                        //            var mapTo2 = MapTo2Method.MakeGenericMethod(itemType);
                        //            var itemResult = mapTo2.Invoke(null, new object[] { item, alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch, includeMembers });
                        //            arrayResults.Add(itemResult);
                        //        }
                        //        var list = arrayResults.CastToList(itemType);
                        //        if (destinationProperty.PropertyType.IsAssignableFrom(list.GetType())) value = list;
                        //        else value = list.ToBoxedArray();
                        //    }
                        //    else if (property.Value.ValueKind == JsonValueKind.Object)
                        //    {
                        //        var mapTo2 = MapTo2Method.MakeGenericMethod(destinationProperty.PropertyType);
                        //        value = mapTo2.Invoke(null, new object[] { property.Value, alwaysMakeAcopyEvenIfSourceAndDestinationTypesMatch, includeMembers });//not tested
                        //        //throw new NotImplementedException("object kind not implemented.");
                        //    }
                        //    else
                        //    {
                        //        value = TypeDescriptor.GetConverter(destinationProperty.PropertyType).ConvertFromInvariantString(property.Value.ToString());// Convert.ChangeType(property.Value.ToString(), destinationProperty.PropertyType);
                        //    }
                        //    if (!value!.Equals(default(JsonProperty))) destinationProperty.SetValue(destination, value, null);
                        //}
                        //else
                        if (sourceProperty != null && sourceProperty.CanWrite)
                        { //handle object
                            var rawValue = sourceProperty.GetValue(source, null);
                            if (rawValue!.TryChangeType(destinationProperty.PropertyType, out object? value))
                            {
                                destinationProperty.SetValue(destination, value, null);
                            }
                            else throw new NotImplementedException();
                        }
                    }
                }
            }
            return destination;
        }

        /// <summary>
        /// Maps this object that is a collection to the specified return type array
        /// </summary>
        /// <typeparam name="TReturn">The return type to map the collection items to</typeparam>
        /// <param name="ignoreMembers">Member names to ignore when mapping</param>
        public static TReturn?[] MapToArray<TReturn>(this object? source, params string[] ignoreMembers)
            where TReturn : new()
        {
            if (source is null) return (TReturn[])Array.CreateInstance(typeof(TReturn), 0);
            if (source is IEnumerable srcCollection)
            {
                var collection = srcCollection.CastToCollection<object[]>();
                var result = new TReturn?[collection.Length];
                for (var i = 0; i < collection.Length; i++)
                {
                    result[i] = collection[i].MapTo<TReturn>(ignoreMembers);
                }
                return result;
            }
            throw new ArrayTypeMismatchException($"{source.GetType().Name} must be of type IEnumerable. E.g. Array, List, ArrayList, IEnumerable, Collection, etc.");
        }

        /// <summary>
        /// Sets the backing field of the property by property name
        /// </summary>
        public static void SetPropertyBackingField(this object source, string propertyName, object value, bool ignoreCase = false)
        {
            //if (source is null) throw new ArgumentException("Cannot set backing field of null object.");
            var field = source.GetType().GetBackingFieldInfo(propertyName, ignoreCase);
            if (field is null) throw new ArgumentException("Property backing field was not found.", nameof(propertyName));
            field.SetValue(source, value);
        }

        /// <summary>
        /// Sets the property or field value by name
        /// </summary>
        public static T? SetValue<T>(this T source, string memberName, object? value, bool ignoreCase = false)
        {
            object? src = source; //box to control boxing for setting fields on structs..
            if (src != null)
            {
                var member = src.GetType().GetValueMemberInfo(memberName, ignoreCase);
                if (member is PropertyInfo property) property.SetValue(src, value);
                else if (member is FieldInfo field) field.SetValue(src, value);
                else if (member is null) throw new ArgumentException("Object does not contain a value member of specified member name. I.e. field or property", nameof(memberName));
                else throw new Exception("Unexpected member type encountered: " + member.MemberType);
            }
            return (T?)src; //unbox
        }

        ///// <summary>
        ///// Gets the property or field value by name
        ///// </summary>
        //public static object GetValue(this object source, string memberName, bool ignoreCase = false)
        //{
        //    if (source == null) throw new ArgumentNullException(nameof(source));

        //    var member = source.GetType().GetValueMemberInfo(memberName, ignoreCase);
        //    if (member is PropertyInfo property) return property.GetValue(source);
        //    else if (member is FieldInfo field) return field.GetValue(source);
        //    else throw new Exception("Unexpected member type encountered: " + member.MemberType);
        //}

        public static T? ShallowCopy<T>(this T source, params string[] ignoreMembers)
                                    where T : new()
        {
            return source.MapTo<T>(true, ignoreMembers);
        }

        /// <summary>
        /// Maps this object to a Dictionary&lt;string, object>
        /// </summary>
        /// <param name="ignoreMembers">Member names to ignore when mapping</param>
        public static Dictionary<string, object?> ToDictionary(this object source, Capitalization capitalization = Capitalization.None, params string[] ignoreMembers)
        {
            var result = new Dictionary<string, object?>();
            if (capitalization == Capitalization.Camel) ignoreMembers = ignoreMembers.Select(m => m.FirstCharToLower()).ToArray();
            else if (capitalization == Capitalization.Pascal) ignoreMembers = ignoreMembers.Select(m => m.FirstCharToUpper()).ToArray();
            if (source is Dictionary<string, object> dict) source = dict.ToDictionary(kv => kv.Key, kv => (object?)kv.Value);
            if (source is Dictionary<string, object?> dictionary) //dictionary short-circuit
            {
                foreach (var kv in dictionary)
                {
                    var key = capitalization == Capitalization.Camel ? kv.Key.FirstCharToLower() : capitalization == Capitalization.Pascal ? kv.Key.FirstCharToUpper() : kv.Key;
                    if (!ignoreMembers.Contains(key)) //not ignored
                    {
                        if (result.ContainsKey(key)) //duplicate key so keep the value from the key that did not need modifying by the capitalization policy
                        {
                            if (key == kv.Key) result[key] = kv.Value;
                        }
                        else result.Add(key, kv.Value);
                    }
                }
                return result;
            }
            var fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var memberNames = fields.Select(f => f.Name).Concat(properties.Select(p => p.Name));
            if (capitalization == Capitalization.Camel) memberNames = memberNames.Select(m => m.FirstCharToLower()).ToArray();
            else if (capitalization == Capitalization.Pascal) memberNames = memberNames.Select(m => m.FirstCharToUpper()).ToArray();
            foreach (var name in memberNames)
            {
                if (!ignoreMembers.Contains(name)) result.Add(name, source.GetValue(name, capitalization != Capitalization.None));
            }
            return result;
        }

        /// <summary>
        /// Maps this object to a Dictionary&lt;string, object>
        /// </summary>
        /// <param name="includeMembers">Member names to include when mapping</param>
        public static Dictionary<string, object?> ToDictionary2(this object source, Capitalization capitalization = Capitalization.None, params string[] includeMembers)
        {
            var result = new Dictionary<string, object?>();
            if (capitalization == Capitalization.Camel) includeMembers = includeMembers.Select(m => m.FirstCharToLower()).ToArray();
            else if (capitalization == Capitalization.Pascal) includeMembers = includeMembers.Select(m => m.FirstCharToUpper()).ToArray();
            if (source is Dictionary<string, object> dictionary) //dictionary short-circuit
            {
                foreach (var kv in dictionary)
                {
                    var key = capitalization == Capitalization.Camel ? kv.Key.FirstCharToLower() : capitalization == Capitalization.Pascal ? kv.Key.FirstCharToUpper() : kv.Key;
                    if (includeMembers.Contains(key)) result.Add(key, kv.Value);
                }
                return result;
            }
            var fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var memberNames = fields.Select(f => f.Name).Concat(properties.Select(p => p.Name));
            if (capitalization == Capitalization.Camel) memberNames = memberNames.Select(m => m.FirstCharToLower()).ToArray();
            else if (capitalization == Capitalization.Pascal) memberNames = memberNames.Select(m => m.FirstCharToUpper()).ToArray();
            foreach (var name in memberNames)
            {
                if (includeMembers.Contains(name)) result.Add(name, source.GetValue(name, capitalization != Capitalization.None));
            }
            return result;
        }

        /// <summary>
        /// Shorthand because many methods require arrays
        /// </summary>
        public static T[] ToSingleItemArray<T>(this T source)
        {
            return new[] { source };
        }

        /// <summary>
        /// Tries to change the type to another type
        /// </summary>
        public static bool TryChangeType<T>(this object input, out T? value)
        {
            if (input.TryChangeType(typeof(T), out object? converted))
            {
                value = (T?)converted;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to change the type to another type
        /// </summary>
        public static bool TryChangeType(this object? input, Type type, out object? value)
        {
            try
            {
                if (input == null)
                {
                    value = type.GetDefaultValue();
                    return true;
                }
                var isInputDictionary = input.GetType().GetInterfaces().Contains(typeof(IDictionary));
                var isOutputDictionary =
                    (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)) ||
                    type.GetInterfaces().Any(i =>
                        i == typeof(IDictionary) || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                    );

                if (isInputDictionary && isOutputDictionary)
                {
                    var isOutputIReadonlyDictionary = type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) || type.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>);//TODO: handle types inherited from these
                    //var keyValueTypes = type.GenericTypeArguments;
                    IDictionary inDictionary = (input as IDictionary)!;
                    IDictionary outDictionary = isOutputIReadonlyDictionary ? (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(type.GenericTypeArguments))! : (IDictionary)Activator.CreateInstance(type)!;
                    foreach (var key in inDictionary.Keys)
                    {
                        bool keyOkay = TryChangeType(key, type.GenericTypeArguments[0], out object? k);
                        bool valueOkay = TryChangeType(inDictionary[key], type.GenericTypeArguments[1], out object? v);
                        if (keyOkay && valueOkay)
                        {
                            outDictionary.Add(k!, v);
                        }
                        else
                        {
                            value = null;
                            return false;
                        }
                    }
                    if (isOutputIReadonlyDictionary)
                    {
                        var readonlyType = typeof(ReadOnlyDictionary<,>).MakeGenericType(type.GenericTypeArguments);
                        value = Activator.CreateInstance(readonlyType, outDictionary);
                    }
                    else value = outDictionary;
                }
                else if (input is DateTime dt && type == typeof(string))
                {
                    value = dt.ToString("o");
                }
                else if (input is string str && type == typeof(DateTime))
                {
                    if (DateTime.TryParse(str, out var date)) value = date;
                    else value = default(DateTime);
                }
                else value = Convert.ChangeType(input, type);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the property or field value by name
        /// </summary>
        public static bool TryGetValue<TReturn>(this object source, string name, out TReturn? value, bool ignoreCase = false)
        {
            if (TryGetValue(source, name, out object? v, ignoreCase) && typeof(TReturn).IsAssignableFrom(v!.GetType()))
            {
                value = (TReturn)v;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get the property or field value by name
        /// </summary>
        public static bool TryGetValue(this object source, string name, out object? value, bool ignoreCase = false)
        {
            object? result = null;
            if (source != null)
            {
                var flags = ignoreCase ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase : BindingFlags.Instance | BindingFlags.Public;
                var property = source.GetType().GetProperty(name, flags);
                if (property != null)
                {
                    result = property.GetValue(source);
                }
                else
                {
                    var field = source.GetType().GetField(name, flags);
                    if (field != null)
                    {
                        result = field.GetValue(source);
                    }
                }
                if (result == null) //try private
                {
                    flags = ignoreCase ? BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase : BindingFlags.Instance | BindingFlags.NonPublic;
                    property = source.GetType().GetProperty(name, flags);
                    if (property != null)
                    {
                        result = property.GetValue(source);
                    }
                    else
                    {
                        var field = source.GetType().GetField(name, flags);
                        if (field != null)
                        {
                            result = field.GetValue(source);
                        }
                    }
                }
            }
            value = result;
            return result != null;
        }

        /// <summary>
        /// Used internally to determine the equality of IEnumerables
        /// </summary>
        private static bool CompareEnumerables<T>(IEnumerable<T> thisValue, IEnumerable<T> thatValue, StringBuilder log, string memberPath)
        {
            bool equal;
            if (thisValue.Count() != thatValue.Count())
            {
                equal = false;
                log.Append($"{memberPath} are not equal. length: ( {thisValue.Count()} , {thatValue.Count()} ).");
            }
            else
            {
                var thisArray = thisValue.ToArray();
                var thatArray = thatValue.ToArray();
                Array.Sort(thisArray, new ReflectionSortComparer());
                Array.Sort(thatArray, new ReflectionSortComparer());
                equal = thisArray.SequenceEqual(thatArray, new IsComparableEqualityComparer<T>(log, memberPath));
            }
            return equal;
        }

        /// <summary>
        /// Gets the current member path from an ancestor path and MemberInfo
        /// </summary>
        private static string GetMemberPath(string ancestorPath, MemberInfo member)
        {
            return (ancestorPath + "." + member.Name).TrimStart('.');
        }

        /// <summary>
        /// Used internally to build a list of differences, determining if the objects are comparable
        /// </summary>
        private static bool IsComparable(Type type, object? thisValue, object? thatValue, StringBuilder log, string memberPath = "")
        {
            bool equal = true;
            var typeName = type is TypeInfo info ? info.Name : type.Name;
            if (thisValue == null || thatValue == null)
            {
                equal = thisValue == thatValue;
                if (!equal) log.Append($"{memberPath} are not equal. values: ( {thisValue} , {thatValue} ).");
            }
            else if (typeName == "KeyValuePair`2")
            {
                var property = thisValue.GetType().GetProperty(nameof(KeyValuePair<int, int>.Value))!;
                var value1 = property.GetValue(thisValue);
                var value2 = property.GetValue(thatValue);
                if (!IsComparable(property.PropertyType, value1, value2, log, GetMemberPath(memberPath, property))) equal = false;
            }
            else if (typeName.StartsWith("Tuple"))
            {
                throw new NotImplementedException($"{nameof(IsComparable)} type {typeName} is not implemented.");
            }
            else if (type.IsValueType || type == typeof(string))
            {
                if (type == typeof(DateTime)) equal = object.Equals(((DateTime)thisValue).ToUniversalTime().ToString("o"), ((DateTime)thatValue).ToUniversalTime().ToString("o"));
                else equal = object.Equals(thisValue, thatValue);
                if (!equal) log.Append($"{memberPath} are not equal. values: ( {thisValue} , {thatValue} ).");
            }
            else if (type is TypeInfo typeInfo && typeInfo.ImplementedInterfaces.Any(t => t == typeof(IEnumerable)))
            {
                var genericEnumerableType = typeInfo.ImplementedInterfaces.First(ti => (ti.Name == "IEnumerable`1"));
                var genericType = (genericEnumerableType as TypeInfo)!.GenericTypeArguments.First();

                MethodInfo method = typeof(ObjectExtension).GetMethod(nameof(ObjectExtension.CompareEnumerables), BindingFlags.Static | BindingFlags.NonPublic)!;
                MethodInfo compareEnumerablesMethod = method.MakeGenericMethod(genericType);
                equal = (bool)compareEnumerablesMethod.Invoke(null, new object[] { thisValue, thatValue, log, memberPath + "[]" })!;
            }
            else
            {
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var value1 = property.GetValue(thisValue);
                    var value2 = property.GetValue(thatValue);
                    if (!IsComparable(property.PropertyType, value1, value2, log, GetMemberPath(memberPath, property))) equal = false;
                }
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var value1 = field.GetValue(thisValue);
                    var value2 = field.GetValue(thatValue);
                    if (!IsComparable(field.FieldType, value1, value2, log, GetMemberPath(memberPath, field))) equal = false;
                }
            }

            return equal;
        }

        /// <summary>
        /// Generic equality comparer
        /// </summary>
        private class IsComparableEqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly StringBuilder log;
            private readonly string memberPath;

            public IsComparableEqualityComparer(StringBuilder log, string memberPath)
            {
                this.log = log;
                this.memberPath = memberPath;
            }

            public bool Equals([AllowNull] T x, [AllowNull] T y)
            {
                var childLog = new StringBuilder();
                var areEqual = IsComparable(typeof(T), x, y, childLog, memberPath);
                if (!areEqual) log.Append(childLog);
                return areEqual;
            }

            public int GetHashCode([DisallowNull] T obj)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Used for comparing 2 objects via reflection
        /// </summary>
        private class ReflectionSortComparer : IComparer
        {
            public int Compare(object? a, object? b)
            {
                if (a == b) return 0;
                if (a is not null && a.Equals(b)) return 0;
                if (a is not null && b is not null)
                { // do type comparisons, if both types are available
                    if (a.GetType() != b.GetType())
                    {
                        return a.GetType().Name.CompareTo(b.GetType().Name);
                    }
                }
                var t = a == null ? b!.GetType() : a.GetType();
                var properties = t.GetProperties();
                foreach (var property in properties)
                {
                    var aProperty = property.GetValue(a);
                    var bProperty = property.GetValue(b);
                    var sortBy = CompareSingle(aProperty, bProperty);
                    if (sortBy == 0) continue;
                    else return sortBy;
                }
                return 0;
            }

            private int CompareSingle(object? a, object? b)
            {
                if (a == b) return 0;
                if (a is not null && a.Equals(b)) return 0;
                Type t = (a == null ? b!.GetType() : a.GetType());
                if (a != b && t == typeof(String))
                {
                    var order = string.Compare(a as string, b as string);
                    return order;
                }
                else if (t == typeof(float) || t == typeof(decimal) || t == typeof(double) || t == typeof(int) || t == typeof(long))
                {
                    var aDecimal = (decimal)Convert.ToDecimal(a);
                    var bDecimal = (decimal)Convert.ToDecimal(b);
                    var order = aDecimal.CompareTo(bDecimal);
                    return order;
                }
                else if (t == typeof(DateTime))
                {
                    var order = DateTime.Compare((DateTime)a!, (DateTime)b!);
                    return order;
                }
                else if (t == typeof(TimeSpan))
                {
                    var order = TimeSpan.Compare((TimeSpan)a!, (TimeSpan)b!);
                    return order;
                }
                else if (t is object && a!.GetType().GetProperty("Id") != null)
                {
                    var propertyInfo = a.GetType().GetProperty("Id")!;
                    return CompareSingle(propertyInfo.GetValue(a), propertyInfo.GetValue(b));
                }
                else throw new NotImplementedException($"Sorting by property type {t!.FullName} is not implemented.");
            }
        }
    }
}