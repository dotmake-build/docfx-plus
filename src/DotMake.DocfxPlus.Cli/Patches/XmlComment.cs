using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using HarmonyLib;

namespace DotMake.DocfxPlus.Cli.Patches
{
    internal partial class XmlComment
    {
        //c# => #region Name
        //vb => #Region "Name"
        //js => //#region Name
        [GeneratedRegex(@"(?i)
            ^
            \s*
            (?://)?\s*
            \#region\s*
            (?:""(?<name>[^""]*)""|(?<name>\S*))
            \s*
            $
        ", RegexOptions.IgnorePatternWhitespace)]
        private static partial Regex RegionRegex();

        //c# => #endregion
        //vb => #End Region
        //js => //#endregion
        [GeneratedRegex(@"(?i)
            ^\s*
            (?://)?\s*
            \#end\s?region
            \s*
            $
        ", RegexOptions.IgnorePatternWhitespace)]
        private static partial Regex EndRegionRegex();

        //xml => <!-- <Name> -->
        //xml => <!-- #region Name -->
        [GeneratedRegex(@"(?i)
            ^
            \s*
            <!--
            \s*
            (?:
                <[^/\s](?<name>\S*)>
                |
                \#region\s*
                (?:""(?<name>[^""]*)""|(?<name>\S*))
            )
            \s*
            -->
            \s*
            $
        ", RegexOptions.IgnorePatternWhitespace)]
        private static partial Regex XmlRegionRegex();

        //xml => <!-- </Name> -->
        //xml => <!-- #endregion -->
        [GeneratedRegex(@"(?i)
            ^
            \s*
            <!--
            \s*
            (?:
                </(?<name>\S*)>
                |
                \#endregion
            )
            \s*
            -->
            \s*
            $
        ", RegexOptions.IgnorePatternWhitespace)]
        private static partial Regex XmlEndRegionRegex();

        internal static void ResolveCode(XDocument doc, object context, object instance)
        {
            //Trim start of new lines inside of parent elements (e.g. `<summary>`, `<example>` under `<member>`)
            //which may contain`<code>` tags.
            //So that text surrounding code blocks does not break YAML due to inconsistent indentation.
            if (doc.Root != null)
                foreach (var parentElement in doc.Root.Elements())
                {
                    foreach (var node in parentElement.DescendantNodes())
                    {
                        if (node is XText xText
                            && xText.Parent?.Name.LocalName.ToLowerInvariant() != "code"
                            && xText.Value.StartsWith('\n'))
                        {
                            var lines = ReadLines(xText.Value)
                                .Select(line => line.TrimStart());

                            xText.Value = string.Join("\n", lines);
                        }
                    }
                }

            foreach (var node in doc.XPathSelectElements("//code[not(ancestor::code)]").ToList())
            {
                if (node.Attribute("data-inline") is { } inlineAttribute)
                {
                    inlineAttribute.Remove();
                    continue;
                }

                /*
                Don't use indent of the <code> block itself (which was probably needed for Yaml,
                otherwise it converted whole example: block to a string with \r\n)
                */
                var indent = "";//new string(' ', ((IXmlLineInfo)node).LinePosition - 2);

                //Add support for tabsize attribute
                var tabSize = 0;
                if (node.Attribute("tabsize") is { } tabSizeAttribute
                    && int.TryParse(tabSizeAttribute.Value, out var tabSizeValue))
                    tabSize = tabSizeValue;
                if (tabSize == 0)
                    tabSize = 4;

                var (lang, sourceLines) = ResolveCodeSource(node, context);
                //var trimEachLine = AccessTools.Method(XmlCommentPatch.XmlCommentType, "TrimEachLine");
                //value = (string)trimEachLine.Invoke(null, [value ?? node.Value, indent]);
                if (sourceLines != null)
                    sourceLines = TrimEachLine(sourceLines, indent, tabSize);
                else
                    (lang, sourceLines) = ResolveCodeContent(node, context, indent, tabSize);

                //var code = new XElement("code", value);
                var code = FixCodeTagLineBreaks(sourceLines);

                //Store original lang from file (if source was set) for using as title on client-side tabs
                if (!string.IsNullOrWhiteSpace(lang))
                    code.SetAttributeValue("data-file-type", lang);

                if (node.Attribute("title") is { } titleAttribute
                    && !string.IsNullOrEmpty(titleAttribute.Value))
                    code.SetAttributeValue("data-title", titleAttribute.Value);

                //Override lang detected from source file extension, only if attribute has a value
                if (node.Attribute("language") is { } languageAttribute
                    && !string.IsNullOrWhiteSpace(languageAttribute.Value))
                {
                    lang = languageAttribute.Value.ToLowerInvariant();
                }

                if (string.IsNullOrWhiteSpace(lang))
                {
                    lang = "cs";
                }

                code.SetAttributeValue("class", $"lang-{lang}");

                var pre = new XElement("pre", code);

                if (node.PreviousNode is XElement)
                    node.ReplaceWith('\n', pre);
                else
                {
                    if (node.PreviousNode is XText xText && !xText.Value.EndsWith('\n'))
                        xText.Value += '\n';

                    node.ReplaceWith(pre);
                }

                /*
                
                if (node.PreviousNode is null
                    || node.PreviousNode is XText xText && xText.Value == $"\n{indent}")
                {
                    // Xml writer formats <pre><code> with unintended identation
                    // when there is no preceeding text node.
                    // Prepend a text node with the same indentation to force <pre><code>.
                    node.ReplaceWith($"\n{indent}", new XElement("pre", code));
                }
                else
                {
                    node.ReplaceWith(new XElement("pre", code));
                }
                */
            }
        }

        internal static (string lang, List<string> sourceLines) ResolveCodeContent(XElement node, object context, string indent, int tabSize)
        {
            var sourceLines = new List<string>();
            string firstLang = null;

            foreach (var subNode in node.Nodes())
            {
                if (subNode is XText subXText)
                {
                    //Trim trailing non-newline whitespace which may be the indentation of the following <code> tag to avoid extra new lines.
                    var value = TrimTrailingNonNewlineWhitespace(subXText.Value);
                    var contentLines = ReadLines(value).ToList();
                    sourceLines.AddRange(TrimEachLine(contentLines, indent, tabSize, trimEmptyLines: false));
                }
                else if (subNode is XElement subXElement && subXElement.Name.LocalName.ToLowerInvariant() == "code")
                {
                    var (lang, nestedSourceLines) = ResolveCodeSource(subXElement, context);
                    if (firstLang == null)
                        firstLang = lang;

                    sourceLines.AddRange(TrimEachLine(nestedSourceLines, indent, tabSize));
                }
            }

            return (firstLang, TrimEmptyLines(sourceLines));
        }

        internal static (string lang, List<string> sourceLines) ResolveCodeSource(XElement node, object context)
        {
            var source = node.Attribute("source")?.Value;
            if (string.IsNullOrEmpty(source))
                return default;

            //Fix backslashes in source path for linux
            source = source.Replace('\\', Path.DirectorySeparatorChar);

            var lang = Path.GetExtension(source).TrimStart('.').ToLowerInvariant();

            var resolveCode = AccessTools.PropertyGetter(context.GetType(), "ResolveCode")
                .Invoke(context, null) as Func<string, string>;
            var sourceText = resolveCode?.Invoke(source);
            if (sourceText is null)
                return (lang, null);

            //Always read source lines (even if no region) to handle inconsistent line breaks (\n or \r\n)
            var sourceLines = ReadLines(sourceText).ToList();

            var region = node.Attribute("region")?.Value;
            if (string.IsNullOrEmpty(region))
                return (lang, sourceLines);

            var (regionRegex, endRegionRegex) = GetRegionRegex(source);

            var regionSourceLines = new List<string>();
            var regionCount = 0;

            foreach (var line in sourceLines)
            {
                var match = regionRegex.Match(line);
                if (match.Success)
                {
                    //used a named group here instead of index because regex is cannot be done in a single group
                    var name = match.Groups["name"].Value.Trim();
                    if (name == region)
                    {
                        ++regionCount;
                        continue;
                    }
                    else if (regionCount > 0)
                    {
                        ++regionCount;
                    }
                }
                else if (regionCount > 0 && endRegionRegex.IsMatch(line))
                {
                    --regionCount;
                    if (regionCount == 0)
                    {
                        break;
                    }
                }

                if (regionCount > 0)
                {
                    regionSourceLines.Add(line);
                }
            }

            return (lang, regionSourceLines);
        }

        internal static IEnumerable<string> ReadLines(string text)
        {
            string line;
            using var sr = new StringReader(text);
            while ((line = sr.ReadLine()) != null)
            {
                yield return line;
            }
        }

        internal static (Regex, Regex) GetRegionRegex(string source)
        {
            var ext = Path.GetExtension(source);
            switch (ext.ToUpperInvariant())
            {
                case ".XML":
                case ".XAML":
                case ".HTML":
                case ".CSHTML":
                case ".VBHTML":
                case ".ASPX":
                case ".CSPROJ":
                case ".SLNX":
                case ".CONFIG":
                    return (XmlRegionRegex(), XmlEndRegionRegex());
            }

            return (RegionRegex(), EndRegionRegex());
        }

        internal static List<string> TrimEachLine(List<string> lines, string indent = null, int tabSize = 4, bool trimEmptyLines = true)
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
                else if(!string.IsNullOrEmpty(indent))
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

        private static List<string> TrimEmptyLines(List<string> lines)
        {
            var start = 0;
            while (start < lines.Count && string.IsNullOrWhiteSpace(lines[start]))
                start++;

            var end = lines.Count - 1;
            while (end >= start && string.IsNullOrWhiteSpace(lines[end]))
                end--;

            return lines.GetRange(start, end - start + 1);
        }

        private static XElement FixCodeTagLineBreaks(List<string> lines)
        {
            /*
            To prevent Yaml multi-line problems, use comment tags `<!-- -->` for line breaks
            We can't use \n as Yaml goes crazy (converts whole example: block to a string with \r\n)
            For example if a line is empty or whitespace, in Yaml it's written without any indent
            and when reading that Yaml it splits the blocks tries to close the previous <pre><code> and open a new one

            Previously we were using <br> tags which required to be replaced back in main.js but comment tags work better
            and does not need to processed in JS.
            */

            var code = new XElement("code");

            foreach (var line in lines)
            {
                if (line == null) continue;

                if (string.IsNullOrWhiteSpace(line))
                    code.Add(new XComment(" "));

                code.Add(new XText(line));

                code.Add(new XText("\n"));
            }

            return code;
        }

        private static string TrimTrailingNonNewlineWhitespace(string input)
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
