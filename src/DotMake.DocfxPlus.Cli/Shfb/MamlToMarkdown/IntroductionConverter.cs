using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class IntroductionConverter : ConverterBase
    {
        public IntroductionConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("introduction".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            var sb = new StringBuilder();

            var address = node.GetAttributeValue("address", "");
            if (!string.IsNullOrWhiteSpace(address))
                sb.AppendLine($"<a id=\"{address}\"></a> ");

            var content = TreatChildren(node);
            if (!string.IsNullOrWhiteSpace(content))
                sb.AppendLine(content);

            return sb.ToString();
        }
    }
}
