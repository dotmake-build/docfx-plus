using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            foreach (var node in doc.XPathSelectElements("//code[not(ancestor::code)]").ToList())
            {
                if (node.Attribute("data-inline") is { } inlineAttribute)
                {
                    inlineAttribute.Remove();
                    continue;
                }

                /*
                As we will use <br> tags for line breaks, don't use indents
                (which was probably needed for Yaml, otherwise it converted whole example: block to a string with \r\n)
                */
                var indent = "";//new string(' ', ((IXmlLineInfo)node).LinePosition - 2);
                //Add support for tabsize attribute
                var tabSize = 0;
                if (node.Attribute("tabsize") is { } tabSizeAttribute
                    && int.TryParse(tabSizeAttribute.Value, out var tabSizeValue))
                    tabSize = tabSizeValue;
                if (tabSize == 0)
                    tabSize = 4;

                var (lang, value) = ResolveCodeSource(node, context);
                //var trimEachLine = AccessTools.Method(XmlCommentPatch.XmlCommentType, "TrimEachLine");
                //value = (string)trimEachLine.Invoke(null, [value ?? node.Value, indent]);
                if (value != null)
                    value = TrimEachLine(value, indent, tabSize);
                else
                    (lang, value) = ResolveCodeContent(node, context, indent, tabSize);


                //var code = new XElement("code", value);
                var code = new XElement("code");
                foreach (var line in ReadLines(value))
                {
                    code.Add(new XText(line));

                    /*
                    To prevent Yaml multi-line problems, use <br> tags for line breaks
                    We can't use \n as Yaml goes crazy (converts whole example: block to a string with \r\n)
                    For example if a line is empty or whitespace, in Yaml it's written without any indent
                    and when reading that Yaml it splits the blocks tries to close the previous <pre><code> and open a new one

                    In the web page, we can simply fix highlight.js to support unescaped <br> tags in main.js:
                        hljs.addPlugin({ 
                          "before:highlightElement": ({ el }) => { el.textContent = el.innerText } });

                    */
                    code.Add(new XElement("br"));
                }

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
            }
        }

        internal static (string lang, string code) ResolveCodeContent(XElement node, object context, string indent, int tabSize)
        {
            var code = new StringBuilder();
            string firstLang = null;

            foreach (var subNode in node.Nodes())
            {
                string codePart = null;

                if (subNode is XText subXText)
                    codePart = subXText.Value;
                if (subNode is XElement subXElement && subXElement.Name == "code")
                {
                    var (lang, value) = ResolveCodeSource(subXElement, context);
                    if (firstLang == null)
                        firstLang = lang;
                    codePart = value;
                }

                if (codePart != null)
                {
                    code.AppendLine(TrimEachLine(codePart, indent, tabSize));
                }
            }

            return (firstLang, code.ToString().Trim());
        }

        internal static (string lang, string code) ResolveCodeSource(XElement node, object context)
        {
            var source = node.Attribute("source")?.Value;
            if (string.IsNullOrEmpty(source))
                return default;

            var lang = Path.GetExtension(source).TrimStart('.').ToLowerInvariant();

            var resolveCode = AccessTools.PropertyGetter(context.GetType(), "ResolveCode")
                .Invoke(context, null) as Func<string, string>;
            var code = resolveCode?.Invoke(source);
            if (code is null)
                return (lang, null);

            var region = node.Attribute("region")?.Value;
            if (region is null)
                return (lang, code);

            var (regionRegex, endRegionRegex) = GetRegionRegex(source);

            var builder = new StringBuilder();
            var regionCount = 0;

            foreach (var line in ReadLines(code))
            {
                if (!string.IsNullOrEmpty(region))
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
                        builder.AppendLine(line);
                    }
                }
                else
                {
                    builder.AppendLine(line);
                }
            }

            return (lang, builder.ToString());
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

        internal static string TrimEachLine(string text, string indent, int tabSize)
        {
            var minLeadingWhitespace = int.MaxValue;
            var lines = ReadLines(text).ToList();
            var tab = new string(' ', tabSize);

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                //Convert tabs to spaces to always ensure correct indentation
                line = lines[i] = lines[i].Replace("\t", tab);

                var leadingWhitespace = 0;
                while (leadingWhitespace < line.Length && char.IsWhiteSpace(line[leadingWhitespace]))
                    leadingWhitespace++;

                minLeadingWhitespace = Math.Min(minLeadingWhitespace, leadingWhitespace);
            }

            var builder = new StringBuilder();

            // Trim leading empty lines
            var trimStart = true;

            // Apply indentation to all lines except the first,
            // since the first new line in <pre></code> is significant
            var firstLine = true;

            foreach (var line in lines)
            {
                if (trimStart && string.IsNullOrWhiteSpace(line))
                    continue;

                if (firstLine)
                    firstLine = false;
                else
                    builder.Append(indent);

                if (string.IsNullOrWhiteSpace(line))
                {
                    builder.AppendLine();
                    continue;
                }

                trimStart = false;
                builder.AppendLine(line.Substring(minLeadingWhitespace));
            }

            return builder.ToString().TrimEnd();
        }
    }
}
