using System;
using System.Linq;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class ListConverter : ConverterBase
    {
        public ListConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("list".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            // Lists inside tables are not supported as markdown, so leave as HTML
            if (node.Ancestors("table").Any())
            {
                return node.OuterHtml;
            }

            var prefixSuffix = Environment.NewLine;

            // Prevent blank lines being inserted in nested lists
            var parentName = node.ParentNode.Name.ToLowerInvariant();
            if (parentName == "list")
            {
                prefixSuffix = "";
            }

            return $"{prefixSuffix}{TreatChildren(node)}{prefixSuffix}";
        }
    }
}
