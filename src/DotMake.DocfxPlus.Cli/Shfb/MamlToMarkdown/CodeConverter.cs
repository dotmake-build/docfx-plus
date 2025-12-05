using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable ConstantNullCoalescingCondition

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class CodeConverter : Code
    {
        public CodeConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("code".ToLowerInvariant(), this);

            //inline code
            Converter.Register("codeInline".ToLowerInvariant(), this);
            Converter.Register("c".ToLowerInvariant(), this);
            Converter.Register("codeReference".ToLowerInvariant(), this);
            Converter.Register("computerOutputInline".ToLowerInvariant(), this);
            Converter.Register("environmentVariable".ToLowerInvariant(), this);
            Converter.Register("command".ToLowerInvariant(), this);
            Converter.Register("languageKeyword".ToLowerInvariant(), this);
            Converter.Register("literal".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            var name = node.Name.ToLowerInvariant();

            var language = "";
            if (name == "code")
            {
                var source = node.Attributes["source"]?.Value;
                var region = node.Attributes["region"]?.Value;
                language = (node.Attributes["language"] ?? node.Attributes["lang"])?.Value
                    ?.ToLowerInvariant()
                    ?.Trim();
                var title = node.Attributes["title"]?.Value.Chomp(all: true);

                if (source != null)
                {
                    var converterContext = MamlToMarkdownConverter.ConverterContexts[Converter];
                    source = converterContext.NormalizeCodePath(source);
                    source = ExternalLinkConverter.EscapeLinkUri(source);

                    if (region != null)
                        region = "#" + ExternalLinkConverter.EscapeLinkUri(region);
                    if (string.IsNullOrEmpty(language))
                        language = "txt";
                    else if (language == "c#")
                        language = "cs";

                    return $"[!code-{language}[{title}]({source}{region})]{Environment.NewLine}";
                }
            }

            /*
            if (name == "command")
            {
                //may support inner <system> and <replaceable> but bold and italic is not possible inside code block 
            }
            */
            
            var content = (name == "command")
                ? TreatChildren(node)
                : WebUtility.HtmlDecode(node.InnerHtml);

            //Find max backticks so that we can surround with max + 1 to be able to escape code
            var matches = Regex.Matches(content, "`+");
            var maxRun = (matches.Count == 0)
                ? 0
                :matches.Max(m => m.Value.Length);

            if (name == "code")
            {
                var backTicks = new string('`', Math.Max(3, maxRun + 1));
                return $"{backTicks}{language}{Environment.NewLine}{content}{Environment.NewLine}{backTicks}";
            }
            else
            {
                var backTicks = new string('`', Math.Max(1, maxRun + 1));
                return InlineConverterBase.InlineWithWhitespaceGuard(node, $"{backTicks}{content}{backTicks}");
            }
        }
    }
}
