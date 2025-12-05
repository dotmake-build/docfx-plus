using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class InlineConverterBase : ConverterBase
    {
        private readonly string[] elements;
        private readonly string emphasis;
        private readonly string surround;

        public InlineConverterBase(Converter converter, string[] elements, string emphasis = null, string surround = null)
            : base(converter)
        {
            this.elements = elements.Select(e => e.ToLowerInvariant())
                .ToArray();
            this.emphasis = emphasis;
            this.surround = surround;

            foreach (var element in this.elements)
            {
                Converter.Register(element, this);
            }
        }

        public override string Convert(HtmlNode node)
        {
            var content = TreatChildren(node);
            if (string.IsNullOrEmpty(content) || AlreadySelf(node))
            {
                return content;
            }
            
            return InlineWithWhitespaceGuard(node, content, emphasis, surround);
        }

        private bool AlreadySelf(HtmlNode node)
        {
            return node.Ancestors().Any(a => elements.Contains(a.Name, StringComparer.OrdinalIgnoreCase));
        }

        internal static string InlineWithWhitespaceGuard(HtmlNode node, string content, string emphasis = null, string surround = null)
        {
            var (leadingSpaces, trailingSpaces) = GetWhitespace(content);
            if (leadingSpaces.Length == 0)
                leadingSpaces = GetNodeSpacePrefix(node);
            if (trailingSpaces.Length == 0)
                trailingSpaces = GetNodeSpaceSuffix(node);

            return $"{leadingSpaces}{emphasis}{surround}{content.Chomp(all: true)}{surround}{emphasis}{trailingSpaces}";
        }

        internal static (string leading, string trailing) GetWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (string.Empty, string.Empty);

            // Leading whitespace
            var leading = new string(input.TakeWhile(char.IsWhiteSpace).ToArray());

            // Trailing whitespace
            var trailing = new string(input.Reverse().TakeWhile(char.IsWhiteSpace).Reverse().ToArray());

            return (leading, trailing);
        }
        
        internal static string GetNodeSpacePrefix(HtmlNode node)
        {
            return node.PreviousSibling is HtmlTextNode textNode
                   && textNode.Text.Length > 0
                   && !char.IsWhiteSpace(textNode.Text[^1]) && !GroupStarters.Contains(textNode.Text[^1])
                ? " "
                : "";
        }

        internal static string GetNodeSpaceSuffix(HtmlNode node)
        {
            return node.NextSibling is HtmlTextNode textNode
                && textNode.Text.Length > 0
                && !char.IsWhiteSpace(textNode.Text[0]) && !SentenceEnders.Contains(textNode.Text[0])
                    //|| elements.Contains(node.NextSibling?.Name, StringComparer.OrdinalIgnoreCase)
                    ? " "
                    : "";
        }

        private static readonly HashSet<char> GroupStarters = ['(', '[', '{'];

        private static readonly HashSet<char> SentenceEnders = ['.', '!', '?', ',', ':', ';', ')', ']', '}'];
    }

    internal class StrongConverter : InlineConverterBase
    {
        public StrongConverter(Converter converter)
            : base(converter, ["application", "database", "hardware", "legacyBold",
                "ui", "unmanagedCodeEntityReference", "userInput", "system"], "**")
        {
        }
    }

    internal class EmConverter : InlineConverterBase
    {
        public EmConverter(Converter converter)
            : base(converter, ["errorInline", "fictitiousUri", "foreignPhrase",
                "legacyItalic", "localUri", "math", "newTerm", "phrase", "placeholder", "replaceable"], "*")
        {
        }
    }

    internal class LineBreakConverter : InlineConverterBase
    {
        public LineBreakConverter(Converter converter)
            : base(converter, ["lineBreak", "br"])
        {
        }
        public override string Convert(HtmlNode node)
        {
            var parentName = node.ParentNode.Name.ToLowerInvariant();
            var parentList = new[] { "strong", "b", "em", "i" };
            if (parentList.Contains(parentName))
            {
                return "";
            }

            return $"  {Environment.NewLine}";
        }
    }

    internal class UnderlineConverter : InlineConverterBase
    {
        public UnderlineConverter(Converter converter)
            : base(converter, ["legacyUnderline", "ins", "u"], "++")
        {
        }
    }

    internal class SubscriptConverter : InlineConverterBase
    {
        public SubscriptConverter(Converter converter)
            : base(converter, ["subscript", "sub"], "~")
        {
        }
    }

    internal class QuoteInlineConverter : InlineConverterBase
    {
        public QuoteInlineConverter(Converter converter)
            : base(converter, ["quoteInline"], "~", "\"")
        {
        }
    }

    internal class CiteConverter : InlineConverterBase
    {
        public CiteConverter(Converter converter)
            : base(converter, ["cite"], "*")
        {
        }
    }

    internal class LinkConverter : InlineConverterBase
    {
        // In HTML, <link>  is defined as a void element (like <br>, <img>).
        // Void elements must not have closing tags and are always treated as self‑closing.
        // So ReverseMarkdown returns <link> as self-closing tag,
        // so we will use a new tag <internalLink> for which have closing tags
        public LinkConverter(Converter converter)
            : base(converter, ["link", "internalLink"])
        {
        }

        public override string Convert(HtmlNode node)
        {
            var xlink = ParseLinkHash(node.GetAttributeValue("xlink:href", "").Chomp(), out var linkHash);

            var linkText = TreatChildren(node).Chomp(all: true);

            var converterContext = MamlToMarkdownConverter.ConverterContexts[Converter];
            converterContext.XlinkHrefMap.TryGetValue(xlink, out var linkUri);
            linkUri ??= "";
            converterContext.XlinkTitleMap.TryGetValue(xlink, out var linkTitle);
            linkTitle ??= "";

            if (string.IsNullOrEmpty(linkText))
                linkText = linkTitle;

            linkText = StringUtils.EscapeLinkText(linkText);
            linkUri = ExternalLinkConverter.EscapeLinkUri(linkUri);
            linkHash = ExternalLinkConverter.EscapeLinkUri(linkHash);

            return InlineWithWhitespaceGuard(node, $"[{linkText}]({linkUri}{linkHash})");
        }

        internal static string ParseLinkHash(string link, out string linkHash)
        {
            linkHash = "";

            var index = link.LastIndexOf('#');
            if (index == -1)
                return link;

            linkHash = link.Substring(index, link.Length - index);

            return link.Substring(0, index);
        }
    }

    internal class ExternalLinkConverter : InlineConverterBase
    {
        public ExternalLinkConverter(Converter converter)
            : base(converter, ["externalLink"])
        {
        }

        public override string Convert(HtmlNode node)
        {
            string linkText = "", linkUri = "", title = "";

            var linkTextEl = node.Element("linkText".ToLowerInvariant());
            if (linkTextEl != null)
            {
                linkText = TreatChildren(linkTextEl).Chomp(all: true);
                linkText = StringUtils.EscapeLinkText(linkText);

                if (string.IsNullOrEmpty(linkText))
                    return linkUri;
            }

            var linkUriEl = node.Element("linkUri".ToLowerInvariant());
            if (linkUriEl != null)
            {
                linkUri = linkUriEl.InnerText.Chomp(all: true);
                linkUri = EscapeLinkUri(linkUri);
            }

            var titleEl = node.Element("linkAlternateText".ToLowerInvariant());
            if (titleEl != null)
            {
                title = titleEl.InnerText.Chomp(all: true);
                title = !string.IsNullOrEmpty(title)
                    ? $" \"{title}\""
                    : "";
            }

            //var linkTarget = node.Element("linkTarget".ToLowerInvariant());

            return InlineWithWhitespaceGuard(node, $"[{linkText}]({linkUri}{title})");
        }

        internal static string EscapeLinkUri(string uri)
        {
            return uri
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace(" ", "%20");
        }
    }

    internal class CodeEntityReferenceConverter : InlineConverterBase
    {
        public CodeEntityReferenceConverter(Converter converter)
            : base(converter, ["codeEntityReference"])
        {
        }

        public override string Convert(HtmlNode node)
        {
            var xref = node.InnerText.Chomp();
            if (xref.Length > 1 && xref[1] == ':')
                xref= xref.Substring(2);

            var linkText = node.GetAttributeValue("linkText".ToLowerInvariant(), "").Chomp();

            //There is also altProperty=name but if we add it uses name even if we use nameWithType so ignore it
            //%26 is escape for & so %26altProperty=name was used add additional query
            var displayProperty = "nameWithType";
            var qualifyHint = node.GetAttributeValue("qualifyHint".ToLowerInvariant(), "");
            if (qualifyHint.Trim().ToLowerInvariant() == "true")
                displayProperty = "fullName";

            linkText = StringUtils.EscapeLinkText(linkText);
            xref = ExternalLinkConverter.EscapeLinkUri(xref);
            //to allow symbols like .#ctor
            xref = xref.Replace("#", "%23");

            return InlineWithWhitespaceGuard(node, $"[{linkText}](xref:{xref}?displayProperty={displayProperty})");
        }

        /// <summary>
        /// Extracts the member name (method, property, type) from an xref string.
        /// Examples:
        ///   "M:System.IO.FileStream.Write(System.Byte[],System.Int32,System.Int32)" → "Write"
        ///   "T:System.String" → "String"
        ///   "P:System.IO.FileStream.Length" → "Length"
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private static string GetXrefMemberName(string xref)
        {
            if (string.IsNullOrWhiteSpace(xref))
                return string.Empty;

            // Remove parameter list if present
            var beforeParen = xref.Split('(')[0];

            // Split by '.' and take the last part
            var lastPart = beforeParen.Split('.').Last();

            return lastPart;
        }
    }

    internal class AutoOutlineConverter : InlineConverterBase
    {
        public AutoOutlineConverter(Converter converter)
            : base(converter, ["autoOutline"])
        {
        }
        public override string Convert(HtmlNode node)
        {
            return "";
        }
    }

}
