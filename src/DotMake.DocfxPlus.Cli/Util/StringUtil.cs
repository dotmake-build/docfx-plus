using System;
using System.Collections.Generic;
using System.IO;

namespace DotMake.DocfxPlus.Cli.Util
{
    internal static class StringUtil
    {
        public static IEnumerable<string> ReadLines(string text)
        {
            string line;
            using var sr = new StringReader(text);
            while ((line = sr.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static List<string> TrimEachLine(List<string> lines, string indent = null, int tabSize = 4, bool trimEmptyLines = true)
        {
            var minLeadingWhitespace = int.MaxValue;
            var tab = new string(' ', tabSize);

            // Trim leading and trailing empty lines
            if (trimEmptyLines)
                lines = TrimEmptyLines(lines);

            for (var i = 0; i < lines.Count; i++)
            {
                //Convert tabs to spaces to always ensure correct indentation
                var line = lines[i] = lines[i].Replace("\t", tab);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var leadingWhitespace = 0;
                while (leadingWhitespace < line.Length && char.IsWhiteSpace(line[leadingWhitespace]))
                    leadingWhitespace++;

                minLeadingWhitespace = Math.Min(minLeadingWhitespace, leadingWhitespace);
            }

            var newLines = new List<string>();

            // Apply indentation to all lines except the first,
            // since the first new line in <pre></code> is significant
            var firstLine = true;

            foreach (var line in lines)
            {
                var newLine = "";

                if (firstLine)
                    firstLine = false;
                else if (!string.IsNullOrEmpty(indent))
                    newLine = indent;

                if (string.IsNullOrWhiteSpace(line))
                {
                    newLines.Add(newLine);
                    continue;
                }

                newLine += line.Substring(minLeadingWhitespace);
                newLines.Add(newLine);
            }

            return newLines;
        }

        public static List<string> TrimEmptyLines(List<string> lines)
        {
            var start = 0;
            while (start < lines.Count && string.IsNullOrWhiteSpace(lines[start]))
                start++;

            var end = lines.Count - 1;
            while (end >= start && string.IsNullOrWhiteSpace(lines[end]))
                end--;

            return lines.GetRange(start, end - start + 1);
        }

        public static string TrimTrailingNonNewlineWhitespace(string input)
        {
            var end = input.Length;

            while (end > 0)
            {
                var c = input[end - 1];
                if (c == '\n' || c == '\r') break;
                if (!char.IsWhiteSpace(c)) break;
                end--;
            }

            return input.Substring(0, end);
        }

    }
}
