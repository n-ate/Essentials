using System.Runtime.CompilerServices;

namespace n_ate.Essentials
{
    public static class TupleExtensions
    {
        public static bool HasNullItems(this ITuple value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == null) return true;
            }
            return false;
        }
    }
}