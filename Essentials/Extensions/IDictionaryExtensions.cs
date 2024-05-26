using System.Collections.Generic;

namespace n_ate.Essentials
{
    public static class IDictionaryExtensions
    {
        public static void AggregateFrom<TKey, TValue>(this IDictionary<TKey, TValue> value, IEnumerable<IDictionary<TKey, TValue>> dictionaries, string deliminator = "; ")
        {
            foreach (var dictionary in dictionaries) value.AggregateFrom(dictionary, deliminator);
        }
    }
}