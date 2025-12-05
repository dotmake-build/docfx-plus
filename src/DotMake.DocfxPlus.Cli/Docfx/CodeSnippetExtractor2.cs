using System;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Helpers;

// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal static partial class CodeSnippetExtractor2
    {
        //Allow everything inside tag names
        [GeneratedRegex(@"^.+$", RegexOptions.IgnoreCase)]
        private static partial Regex TagnameFormat();

        public const string TagNamePlaceHolder = "{tagname}";

        public static bool MatchTag(string line, string template, out string tagName, bool containTagName = true)
        {
            tagName = string.Empty;
            if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(template)) return false;

            var splittedTemplate = template.Split(TagNamePlaceHolder);
            var beforeTagName = splittedTemplate[0];
            var afterTagName = splittedTemplate.Length == 2 ? splittedTemplate[1] : string.Empty;

            int column = 0;
            int index = 0;

            // match before
            while (column < line.Length && index < beforeTagName.Length)
            {
                if (!CharHelper.IsWhitespace(line[column]))
                {
                    if (char.ToLower(line[column]) != beforeTagName[index]) return false;
                    index++;
                }
                column++;
            }

            if (index != beforeTagName.Length) return false;

            //match tagname
            var sb = new StringBuilder();
            while (column < line.Length /*&& (afterTagName == string.Empty || line[column] != afterTagName[0])*/)
            {
                sb.Append(line[column]);
                column++;
            }

            //Fixed: CodeSnippetExtractor.MatchTag calls ToLower() on found tag name instead of ToLowerInvariant()
            //Even if result dictionary is OrdinalIgnoreCase, ToLower() causes turkish-I problem
            //so remove ToLower() here so that the tag is found is result dictionary.
            tagName = sb.ToString();
            var afterTagIndex = tagName.LastIndexOf(afterTagName, StringComparison.OrdinalIgnoreCase);
            if (afterTagIndex != -1)
            {
                tagName = tagName.Substring(0, afterTagIndex);
                column -= afterTagName.Length;
            }

            tagName = tagName.Trim();

            //match after tagname
            index = 0;
            while (column < line.Length && index < afterTagName.Length)
            {
                if (!CharHelper.IsWhitespace(line[column]))
                {
                    if (char.ToLower(line[column]) != afterTagName[index]) return false;
                    index++;
                }
                column++;
            }

            if (index != afterTagName.Length) return false;
            while (column < line.Length && CharHelper.IsWhitespace(line[column])) column++;

            return column == line.Length && (!containTagName || TagnameFormat().IsMatch(tagName));
        }
    }
}
