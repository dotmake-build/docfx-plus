using DotMake.DocfxPlus.Cli.Util;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class RootConverter : ConverterBase
    {
        public RootConverter(Converter converter)
            : base(converter)
        {
        }

        public override string Convert(HtmlNode node)
        {
            if (node.OwnerDocument.DocumentNode.GetAttributeValue("nodeFixesCompleted", "") != "true")
            {
                HtmlNodeUtil.UnindentChildren(node);
                HtmlNodeUtil.ReplaceMamlTags(node);

                node.OwnerDocument.DocumentNode.SetAttributeValue("nodeFixesCompleted", "true");
            }

            return TreatChildren(node);
        }
    }
}
