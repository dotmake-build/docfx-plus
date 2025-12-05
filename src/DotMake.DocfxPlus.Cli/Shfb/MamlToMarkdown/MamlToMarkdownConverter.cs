using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DotMake.DocfxPlus.Cli.Util;
using ReverseMarkdown;

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class MamlToMarkdownConverter
    {
        private readonly Converter converter;
        internal static Dictionary<Converter, MamlToMarkdownConverterContext> ConverterContexts = new();

        public MamlToMarkdownConverter(MamlToMarkdownConverterContext converterContext)
        {
            var config = new Config
            {
                UnknownTags = Config.UnknownTagsOption.Bypass,
                GithubFlavored = true
            };

            converter = new Converter(config, typeof(MamlToMarkdownConverter).Assembly);
            ConverterContexts[converter] = converterContext;

            //These tags are normally Bypass converters, but we want to UnindentChildren in custom RootConverter
            var rootConverter = new RootConverter(converter);
            converter.Register("#document", rootConverter);
            converter.Register("html", rootConverter);
            converter.Register("body", rootConverter);

            //block elements
            converter.Register("para".ToLowerInvariant(), converter.Lookup("p"));
            converter.Register("quote".ToLowerInvariant(), converter.Lookup("blockquote"));
            converter.Register("definitionTable".ToLowerInvariant(), converter.Lookup("dl"));
            converter.Register("definedTerm".ToLowerInvariant(), converter.Lookup("dt"));
            converter.Register("definition".ToLowerInvariant(), converter.Lookup("dd"));
            //introduction -> ignored
            //bibliography -> ignored

            //bold
            /*
            var strongConverter = converter.Lookup("strong");
            converter.Register("application".ToLowerInvariant(), strongConverter);
            converter.Register("database".ToLowerInvariant(), strongConverter);
            converter.Register("hardware".ToLowerInvariant(), strongConverter);
            converter.Register("legacyBold".ToLowerInvariant(), strongConverter);
            converter.Register("ui".ToLowerInvariant(), strongConverter);
            converter.Register("unmanagedCodeEntityReference".ToLowerInvariant(), strongConverter);
            converter.Register("userInput".ToLowerInvariant(), strongConverter);
            converter.Register("system".ToLowerInvariant(), strongConverter);
            */

            /*
            //italic
            var emphasisConverter = converter.Lookup("em");
            converter.Register("errorInline".ToLowerInvariant(), emphasisConverter);
            converter.Register("fictitiousUri".ToLowerInvariant(), emphasisConverter);
            converter.Register("foreignPhrase".ToLowerInvariant(), emphasisConverter);
            converter.Register("legacyItalic".ToLowerInvariant(), emphasisConverter);
            converter.Register("localUri".ToLowerInvariant(), emphasisConverter);
            converter.Register("math".ToLowerInvariant(), emphasisConverter);
            converter.Register("newTerm".ToLowerInvariant(), emphasisConverter);
            converter.Register("phrase".ToLowerInvariant(), emphasisConverter);
            converter.Register("placeholder".ToLowerInvariant(), emphasisConverter);
            converter.Register("replaceable".ToLowerInvariant(), emphasisConverter);
            */

            //misc
            converter.Register("superscript".ToLowerInvariant(), converter.Lookup("sup"));
        }

        public string Convert(string maml)
        {
            maml = NormalizeCodeBlocks(maml);
            maml = NormalizeLinkTags(maml);

            return converter.Convert(maml);
        }

        public static string NormalizeCodeBlocks(string html, int tabSize = 4)
        {
            //- Strip All CDATA blocks inside <code> blocks e.g. "<![CDATA[<appSettings>]]>"
            //  This is because HtmlAgilityPack treats CDATA as comment.
            //  First node is comment starting with <![CDATA[ and second node is text ending with ]]>
            //  So it's not possible to use code.innerHtml or code.innerText properly
            //  In addition if code has a </script> closing tag, it's treated literally so not included in code text
            //  For these reasons extract text from inside CDATA and html encode it.
            //- Replace tabs with spaces and normalize indentation.
            //  This should be done before converter.Convert because it calls Cleaner.PreTidy which replaces all \n\t
            //  in the html which loses tabs inside <code> blocks

            return Regex.Replace(
                html,
                @"(?<openTag><(?<tag>code|codeInline|c)\b[^>]*?(?<!/)>)(?:\s*<!\[CDATA\[(?<cdata>.*?)\]\]>\s*|(?<raw>.*?))</\k<tag>>",
                m =>
                {
                    var openTag = m.Groups["openTag"].Value;
                    var tag = m.Groups["tag"].Value;
                    var isCdata = m.Groups["cdata"].Success;
                    var content = isCdata ? m.Groups["cdata"].Value : m.Groups["raw"].Value;

                    // Replace tabs with spaces and normalize indentation
                    var sourceLines = StringUtil.ReadLines(content).ToList();
                    sourceLines = StringUtil.TrimEachLine(sourceLines);
                    content = string.Join("\n", sourceLines);

                    // HTML-encode if it was inside CDATA
                    if (isCdata)
                        content = WebUtility.HtmlEncode(content);

                    return $"{openTag}{content}</{tag}>";
                },
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );
        }

        public static string NormalizeLinkTags(string html)
        {
            // In HTML, <link>  is defined as a void element (like <br>, <img>).
            // Void elements must not have closing tags and are always treated as self‑closing.
            // So ReverseMarkdown returns <link> as self-closing tag,
            // so we will use a new tag <internalLink> for which have closing tags

            return Regex.Replace(
                html,
                @"<link(?<attributes>\b[^>]*?)(?<!/)>(?<content>.*?)</link>",
                m =>
                {
                    var attributes = m.Groups["attributes"].Value;
                    var content = m.Groups["content"].Value;

                    return $"<internalLink{attributes}>{content}</internalLink>";
                },
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );
        }
    }
}
