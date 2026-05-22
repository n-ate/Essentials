using System;
using System.Text;

namespace n_ate.Essentials
{
    public static class StringBuilderExtensions
    {
        private static readonly char[] vowels = ['a', 'e', 'i', 'o', 'u'];

        public static bool ShouldUseAn(this string word)
        {
            if (string.IsNullOrEmpty(word)) throw new ArgumentNullException(nameof(word));
            //if (word.StartsWith('y') && vowels.Contains(word[1])) return true;
            return word.StartsWith(vowels);
        }

        public static string GetFieldSpacer(this string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldValue)) return string.Empty;
            return fieldValue.Contains('\n') ? "\n" : " ";
        }

        public static StringBuilder AppendMarkdownFieldLine(this StringBuilder sb, string label, string value) => sb.AppendLine($"**{label}**:{value.GetFieldSpacer()}{value}\n");
    }
}