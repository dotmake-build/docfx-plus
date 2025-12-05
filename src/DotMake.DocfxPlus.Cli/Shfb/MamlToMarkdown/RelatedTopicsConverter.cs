using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class RelatedTopicsConverter : ConverterBase
    {
        public RelatedTopicsConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("relatedTopics".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            if (!node.ChildNodes.Any(n => n.NodeType == HtmlNodeType.Element))
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine();
            
            sb.AppendLine("## See Also");

            var codeEntityReferences = node.Elements("codeEntityReference".ToLowerInvariant()).ToArray();
            if (codeEntityReferences.Length > 0)
            {
                sb.AppendLine("#### Reference");
                foreach (var codeEntityReference in codeEntityReferences)
                    sb.AppendLine("- " + Treat(codeEntityReference));
            }

            var links = node.Elements("link".ToLowerInvariant()).ToArray();
            if (links.Length > 0)
            {
                sb.AppendLine("#### Concepts");
                foreach (var link in links)
                    sb.AppendLine("- " + Treat(link));
            }

            var externalLinks = node.Elements("externalLink".ToLowerInvariant()).ToArray();
            if (externalLinks.Length > 0)
            {
                sb.AppendLine("#### Other Resources");
                foreach (var externalLink in externalLinks)
                    sb.AppendLine("- " + Treat(externalLink));
            }
            
            sb.AppendLine();

            return sb.ToString();
        }

        private string Treat(HtmlNode node)
        {
            var converter = Converter.Lookup(node.Name);
            return converter.Convert(node);
        }
    }
}
