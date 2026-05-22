using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace n_ate.Essentials.Parsing
{
    public static partial class MarkdownParser
    {
        public class MarkdownConsole : CConsole
        {
            public MarkdownConsole() : base(ConsoleColor.DarkBlue)
            {
            }
        }

        private static readonly MarkdownConsole Con = new();
        private const string KEY_THROW_IT_AWAY = "__THROW_IT_AWAY";

        /// <summary>
        /// Extracts the values of specified fields from the provided markdown text and returns them as a case-insensitive dictionary.
        /// </summary>
        /// <param name="markdownText">The markdown text containing field labels and their corresponding values to extract.</param>
        /// <param name="fieldNames">An array of expected field names to search for and extract from the markdown text. Unexpected fields are logged but still returned.</param>
        /// <returns>A dictionary containing the extracted field names as keys and their associated values from the markdown text.
        /// The dictionary uses case-insensitive key comparison.</returns>
        public static Dictionary<string, string> ExtractMarkdownFields(string markdownText, params string[] fieldNames)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var lines = markdownText.Trim('\n', ' ').Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var unexpectedFieldNames = new List<string>();
            var currentFieldKey = KEY_THROW_IT_AWAY;
            var currentFieldValue = new StringBuilder();
            foreach (var line in lines)
            {
                if (TryGetMarkdownFieldInfo(line, out var info)) //line is a markdown field label or starts with one
                {
                    result[currentFieldKey] = currentFieldValue.ToString().Trim(' ', '\n', '"', '“', '”'); //save the previous field value before moving to the next one
                    currentFieldValue.Clear();
                    var fieldKey = fieldNames.FirstOrDefault(f => info.FieldName.StartsWith(f, StringComparison.OrdinalIgnoreCase));
                    if (fieldKey is null) unexpectedFieldNames.Add(info.FieldName);
                    currentFieldKey = fieldKey ?? info.FieldName;
                    currentFieldValue.Append(line.Substring(info.LabelLength).Trim());
                }
                else currentFieldValue.Append("\n" + line);
            }
            if (unexpectedFieldNames.Any()) Con.WriteLine($"Unexpected markdown label(s) in LLM output ({string.Join(", ", unexpectedFieldNames.Distinct())}):\n{markdownText}");
            result[currentFieldKey] = currentFieldValue.ToString().TrimStart(' ', '\n', '"', '“', '”').TrimEnd(' ', '\n', '"', '“', '”', '-'); //save the last field value
            if (result[currentFieldKey].StartsWith('*') && result[currentFieldKey].EndsWith('*')) result[currentFieldKey] = result[currentFieldKey].Trim('*');
            result.Remove(KEY_THROW_IT_AWAY);
            if (result.Count < fieldNames.Length) //Fallback to non-bold, non-header labels by using expected fieldNames
            {
                var revisedMarkdownText = markdownText;
                foreach (var name in fieldNames) revisedMarkdownText = revisedMarkdownText.Replace($"\n{name}:", $"\n**{name}**:");
                if (markdownText.Length < revisedMarkdownText.Length) return ExtractMarkdownFields(revisedMarkdownText);
            }
            return result;
        }

        public static MarkdownBlock[] SliceByHeaders(string llmMarkdown)
        {
            var headersRegex = markdownHeaderRegex();
            var matches = headersRegex.Matches(llmMarkdown);
            var sections = new List<(int Level, MarkdownBlock Section)>();
            for (var i = 0; i < matches.Count; i++)
            {
                var current = new MarkdownBlock();
                var level = matches[i].Groups["symbols"].Value.Length;
                var levelDiff = sections.Count == 0 ? 0 : level - sections.Last().Level;
                if (levelDiff > 0) sections.Add((level, current));
                else
                {
                    var parentSection = sections.Last().Section;
                    while (levelDiff > 0)
                    {
                        levelDiff--;
                        parentSection = parentSection?.ChildBlocks?.LastOrDefault();
                    }
                    if (parentSection is null) throw new Exception($"Unexpected header structure. Headers must go in order from 1 - 6+. Skipping a level will result in error.");
                    parentSection.ChildBlocks = [.. parentSection.ChildBlocks, current];
                }
                var lineLength = matches[i].Value.Length;

                current.IndexInSource = matches[i].Index;
                current.Header = GetParsedMarkdownHeader(matches[i].Groups["header"].Value);

                var contentIndex = matches[i].Index + lineLength + 1;
                var nextSectionIndex = (i < matches.Count - 1) ? matches[i + 1].Index : llmMarkdown.Length;
                if (contentIndex < nextSectionIndex) current.Content = llmMarkdown.Substring(contentIndex, nextSectionIndex - contentIndex).Trim('\n', ' ');
            }
            return [.. sections.Select(s => s.Section)];
        }

        public static MarkdownSection[] SliceByTopLevelHeaders(string llmMarkdown)
        {
            var headersRegex = markdownHeaderSimpleRegex();
            var matches = headersRegex.Matches(llmMarkdown);
            if (matches.Count == 0) return [];

            var sections = new List<MarkdownSection>();
            var previousContentIndex = 0;
            int topLevel = topLevel = matches[0].Groups["symbols"].Value.Length; //assume the first header is the top level, and all subsequent headers of the same level are siblings, while headers with more symbols are children
            for (var i = 0; i < matches.Count; i++)
            {
                var level = matches[i].Groups["symbols"].Value.Length;
                if (level != topLevel) continue; //skip headers that are not the top level; they will be included in the content of their parent section

                var current = new MarkdownSection
                {
                    IndexInSource = matches[i].Index,
                    Header = GetParsedMarkdownHeader(matches[i].Groups["header"].Value)
                };

                var lineLength = matches[i].Value.Length;
                var contentIndex = matches[i].Index + lineLength + 1; //+1 to skip the newline character after the header line
                if (sections.Count > 0)
                {
                    var previous = sections.Last();
                    previous.Content = llmMarkdown.Substring(previousContentIndex, (matches[i].Index - previousContentIndex)).Trim('\n', ' ');
                }
                previousContentIndex = contentIndex;
                sections.Add(current);
            }
            sections.Last().Content = llmMarkdown.Substring(previousContentIndex);
            return sections.ToArray();
        }

        private static MarkdownHeader GetParsedMarkdownHeader(string headerLine)
        { // "### Scope definition: SD4"
            if (headerLine.Contains('\n')) throw new ArgumentException($"Header line cannot contain a newline character: {headerLine}");
            var markdownHeaderRegex = new Regex(@"^(?<symbols>#+)\s+(?<name>[^:]+)\s*:?\s*(?<raw_id>(?<id_prefix>[a-zA-Z][a-zA-Z]?)(?<id>[1-9][0-9]*))?$", RegexOptions.ExplicitCapture);
            var match = markdownHeaderRegex.Match(headerLine.Trim());

            var result = new MarkdownHeader
            {
                Raw = headerLine
            };
            if (match.Success)
            {
                result.Level = match.Groups["symbols"].Value.Length;
                result.Name = match.Groups["name"].Value;
                result.RawId = match.Groups["raw_id"].Value;
                if (!string.IsNullOrEmpty(result.RawId))
                {
                    result.HasId = true;
                    result.IdPrefix = match.Groups["id_prefix"].Value.ToUpper();
                    result.Id = int.Parse(match.Groups["id"].Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates a single line of LLM output text to determine if it contains a markdown field label
        /// #, ##, ###, **, etc
        /// </summary>
        /// <returns>The name of the field.</returns>
        public static bool TryGetMarkdownFieldInfo(string line, out (string FieldName, int LabelLength) info)
        {
            string fieldName;
            int labelLength;
            var l = line.Trim();
            if (line.Contains('\n')) throw new ArgumentException($"Line contains a newline character, which is not expected: {line}");
            if (l.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                var HeaderLabelRegex = new Regex(@"^#+\s*(?<header>.+)"); //TODO: consider pulling a full header using the same header regex as in SliceByHeaders
                var match = HeaderLabelRegex.Match(l);
                if (!match.Success) throw new ArgumentException($"Line starts with # but doesn't match expected header format: {line}");
                labelLength = line.Length;
                fieldName = match.Groups["header"].Value;
            }
            else if (l.StartsWith("**", StringComparison.OrdinalIgnoreCase))
            {
                var match = BoldLabelRegex().Match(line);
                fieldName = match.Groups[1].Value.Trim();
                labelLength = match.Groups[0].Value.Length;
            }
            else
            {
                info = (string.Empty, -1);
                return false;
            }
            info = (fieldName, labelLength);
            return true;
        }

        /// <summary>
        /// Finds bold labels in a single line of LLM output text, despite LLM output quirks. E.g. "> **field-name**: ", "**field-name:**", "  ** field name **:  "
        /// </summary>
        [GeneratedRegex(@"^>? *\*\*([^\*]+):?\*\*:? *")] //Received unexpected: *field name* (101 chars)\nfield value...
        private static partial Regex BoldLabelRegex();

        public static int[] ExtractHashIds(string expectedName, string llmMarkdown)
        {
            var n = expectedName.ToLower();
            //var refrenceExtractor = new Regex($@"((^|\n)-? ?{n}?:([^#\n]*#(?<numbers>\d+))*|\({n}?(:| )([^#\n\)\d]*#?(?<numbers>\d+))*)");
            //var refrenceExtractor = new Regex($@"((^|\n)-? ?{n}?:([^#\n]*#(?<numbers>\d+))*|\({n}?(:|\s)([^\n\)\d]*(?<numbers>\d+))*)");
            var refrenceExtractor = new Regex($@"((^|\n)-*\s*{n}?([^\d\n(]*(?<numbers>\d+)?(\([^\n)]*\))*)*|\({n}?(:|\s)([^\n\)\d]*(?<numbers>\d+))*)");
            return refrenceExtractor.Matches(llmMarkdown.ToLower()).SelectMany(m => m.Groups["numbers"].Captures.Select(c => int.Parse(c.Value))).ToArray();
        }

        [GeneratedRegex(@"^(?<header>(?<symbols>#+)\s+.+)$", RegexOptions.Multiline)]
        private static partial Regex markdownHeaderSimpleRegex();

        // "### Scope definition: SD4"
        [GeneratedRegex(@"^(?<symbols>#+)\s+(?<header>.+)$", RegexOptions.Multiline)]
        private static partial Regex markdownHeaderRegex();
    }
}