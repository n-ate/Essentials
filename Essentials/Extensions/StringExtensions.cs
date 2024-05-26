using System;
using System.Text.RegularExpressions;

namespace n_ate.Essentials
{
    public static class StringExtensions
    {
        private static readonly MatchEvaluator _addSpaces =
            new MatchEvaluator(m =>
            {
                var all = m.Groups["all"];
                var remnant = m.Groups["remnant"];
                var start = m.Groups["start"];
                var end = m.Groups["end"];
                if (start.Success)
                {
                    if (!start.Value.Equals(" "))
                    {
                        return string.Concat(start.Value, ' ', remnant.Value);
                    }
                }
                else if (all.Index > 0 && end.Success)
                {
                    return string.Concat(' ', all.Value);
                }
                return all.Value;
            }
        );

        private static readonly Regex _captureCapitals = new Regex("(?<all>(?<start>[a-z ])?(?<remnant>[A-Z](?<end>[a-z])?))");

        /// <summary>
        /// Adds spaces before each capital letter to make a friendly name
        /// </summary>
        public static string CamelCaseToFriendly(this string value)
        {
            return _captureCapitals.Replace(value, _addSpaces);
        }

        public static bool FirstCharIsLower(this string value) => value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            "" => throw new ArgumentException($"{nameof(value)} cannot be empty", nameof(value)),
            _ => char.ToLower(value[0]) == value[0]
        };

        public static bool FirstCharIsUpper(this string value) => !FirstCharIsLower(value);

        public static string FirstCharToLower(this string value) => value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            "" => throw new ArgumentException($"{nameof(value)} cannot be empty", nameof(value)),
            _ => string.Concat(char.ToLower(value[0]), value[1..])
        };

        public static string FirstCharToUpper(this string value) => value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            "" => throw new ArgumentException($"{nameof(value)} cannot be empty", nameof(value)),
            _ => string.Concat(char.ToUpper(value[0]), value[1..])
        };
    }
}