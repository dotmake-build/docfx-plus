using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class MarkupConverter : ConverterBase
    {
        public MarkupConverter(Converter converter) : base(converter)
        {
            Converter.Register("markup", this);
        }

        public override string Convert(HtmlNode node)
        {
            return node.InnerHtml;
        }
    }
}
