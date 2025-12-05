using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using DotMake.DocfxPlus.Cli.Util;
using HarmonyLib;

namespace DotMake.DocfxPlus.Cli.Docfx
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
            (?:""(?<name>[^""]*)""|(?<name>.*?))
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
                <[^/\s](?<name>.*?)>
                |
                \#region\s*
                (?:""(?<name>[^""]*)""|(?<name>.*?))
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
                </(?<name>.*?)>
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
                            && xText.Parent?.Name.LocalName.ToLowerInvariant() != "code")
                        {
                            var lines = StringUtil.ReadLines(xText.Value)
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
                    tabSize = CodeLanguageUtil.GetTabSize(node.Attribute("source")?.Value);

                var (lang, sourceLines) = ResolveCodeSource(node, context);
                //var trimEachLine = AccessTools.Method(XmlCommentPatch.XmlCommentType, "TrimEachLine");
                //value = (string)trimEachLine.Invoke(null, [value ?? node.Value, indent]);
                if (sourceLines != null)
                    sourceLines = StringUtil.TrimEachLine(sourceLines, indent, tabSize);
                else
                    (lang, sourceLines) = ResolveCodeContent(node, context, indent, tabSize);

                //var code = new XElement("code", value);
                var code = CreateCodeTag(sourceLines);

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
                    var value = StringUtil.TrimTrailingNonNewlineWhitespace(subXText.Value);
                    var contentLines = StringUtil.ReadLines(value).ToList();
                    sourceLines.AddRange(StringUtil.TrimEachLine(contentLines, indent, tabSize, trimEmptyLines: false));
                }
                else if (subNode is XElement subXElement && subXElement.Name.LocalName.ToLowerInvariant() == "code")
                {
                    var (lang, nestedSourceLines) = ResolveCodeSource(subXElement, context);
                    if (firstLang == null)
                        firstLang = lang;

                    sourceLines.AddRange(StringUtil.TrimEachLine(nestedSourceLines, indent, tabSize));
                }
            }

            return (firstLang, StringUtil.TrimEmptyLines(sourceLines));
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
            var sourceLines = StringUtil.ReadLines(sourceText).ToList();

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
                //new
                case ".ASPX":
                case ".CSPROJ":
                case ".SLNX":
                case ".CONFIG":
                    return (XmlRegionRegex(), XmlEndRegionRegex());
            }

            return (RegionRegex(), EndRegionRegex());
        }

        private static XElement CreateCodeTag(List<string> lines)
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
    }
}
