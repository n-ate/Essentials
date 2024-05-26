using System;
using System.Collections;
using System.Collections.Generic;

namespace n_ate.Essentials
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Casts the IEnumerable to the reflected IEnumerable type
        /// </summary>
        public static IList CastToList(this IEnumerable source)
        {
            Type itemType = source.GetType().GetInnerType();
            var listType = typeof(List<>).MakeGenericType(itemType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in source) list.Add(item);
            return list;
        }

        /// <summary>
        /// Casts the IEnumerable to the specified type of list
        /// </summary>
        public static IList CastToList(this IEnumerable source, Type itemType)
        {
            var listType = typeof(List<>).MakeGenericType(itemType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in source) list.Add(item);
            return list;
        }

        /// <summary>
        /// Casts the IEnumerable to the specified collection type
        /// </summary>
        public static TCollection CastToCollection<TCollection>(this IEnumerable source)
        {
            return (TCollection)source.CastToCollection(typeof(TCollection));
        }

        /// <summary>
        /// Casts the IEnumerable to the specified collection type.
        /// </summary>
        /// <returns>Returns default if cast is invalid.</returns>
        public static TCollection? AsCollection<TCollection>(this IEnumerable source) where TCollection : IEnumerable
        {
            try
            {
                return (TCollection)source.CastToCollection(typeof(TCollection));
            }
            catch { return default; }
        }

        /// <summary>
        /// Casts the IEnumerable to the specified collection type
        /// </summary>
        public static IEnumerable CastToCollection(this IEnumerable source, Type collectionType)
        {
            var listType = typeof(List<>).MakeGenericType(collectionType.GetInnerType());
            var list = (IList)Activator.CreateInstance(listType)!;
            if (source != null) foreach (var item in source) list.Add(item);
            if (collectionType.IsArray) return (IEnumerable)list.ToBoxedArray();
            else if (typeof(IList).IsAssignableFrom(collectionType)) return list;
            else throw new NotImplementedException("Cast for collection type is not implemented. Type: " + collectionType.FullName);
        }

        /// <summary>
        /// Changes the IEnumerable to an array of the correct type boxed as an object
        /// </summary>
        public static object ToBoxedArray(this IEnumerable source)
        {
            var type = source.GetType();
            var arrayType = type.GetInnerType();// (type.IsGenericType) ? type.GetElementType() : typeof(object);//.GetGenericArguments()[0];
            var enumerator = source.GetEnumerator();
            var count = 0;
            while (enumerator.MoveNext()) count++;
            var array = Array.CreateInstance(arrayType, count);
            enumerator.Reset();
            count = 0;
            while (enumerator.MoveNext())
            {
                count++;
                array.SetValue(enumerator.Current, count - 1);
            }
            return array;
        }

        public static long iCount(this IEnumerable source)
        {
            long count = 0;
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext()) count++;
            return count;
        }

        public static object iFirst(this IEnumerable source)
        {
            var enumerator = source.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public static bool TryChangeCollectionType<TItem>(this IEnumerable source, out TItem[]? result)
        {
            var list = new List<TItem>();
            var enumerator = source.GetEnumerator();
            result = null;
            while (enumerator.MoveNext())
            {
                try
                {
                    if (enumerator.Current.TryChangeType<TItem>(out var r)) list.Add(r!);
                    else return false;
                }
                catch { return false; }
            }
            result = list.ToArray();
            return true;
        }
    }
}