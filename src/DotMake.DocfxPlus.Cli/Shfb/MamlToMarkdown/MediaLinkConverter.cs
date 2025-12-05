using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class MediaLinkConverter : ConverterBase
    {
        public MediaLinkConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("mediaLink".ToLowerInvariant(), this);
            Converter.Register("mediaLinkInline".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            var image = node.Element("image");
            string imagePlacement = "", imageAlt = "", imageUri = "", imageMdSyntax = "", linkHash = "";
            if (image != null)
            {
                var xlink = LinkConverter.ParseLinkHash(image.GetAttributeValue("xlink:href", "").Trim(), out linkHash);
                imagePlacement = image.GetAttributeValue("placement", "").Trim().ToLowerInvariant();

                var converterContext = MamlToMarkdownConverter.ConverterContexts[Converter];
                converterContext.ImageXlinkHrefMap.TryGetValue(xlink, out imageUri);
                imageUri ??= "";
                converterContext.ImageXlinkAltMap.TryGetValue(xlink, out imageAlt);
                imageAlt ??= "";

                var isInline = (node.Name == "mediaLinkInline".ToLowerInvariant());

                if (isInline || (imagePlacement != "center" && imagePlacement != "far"))
                    imageMdSyntax = $"![{StringUtils.EscapeLinkText(imageAlt)}]({ExternalLinkConverter.EscapeLinkUri(imageUri)}{ExternalLinkConverter.EscapeLinkUri(linkHash)})";

                if (isInline)
                    return InlineConverterBase.InlineWithWhitespaceGuard(node, imageMdSyntax);
            }

            var caption = node.Element("caption");
            string captionPlacement = "", captionValue = "";
            if (caption != null)
            {
                captionPlacement = caption.GetAttributeValue("placement", "").Trim().ToLowerInvariant();
                var captionLead = caption.GetAttributeValue("lead", "");

                captionValue = TreatChildren(caption).Chomp();
                if (!string.IsNullOrEmpty(captionLead))
                    captionValue = captionLead + ": " + captionValue;
            }


            var sb = new StringBuilder();
            sb.AppendLine();

            if (imagePlacement == "center" || imagePlacement == "far")
            {
                var align = imagePlacement == "far" ? "right" : imagePlacement;

                sb.AppendLine($"<p align=\"{align}\">");

                if (captionPlacement != "after" && !string.IsNullOrEmpty(captionValue))
                    sb.AppendLine($"<em>{captionValue}</em><br>");

                sb.AppendLine($"<img alt=\"{imageAlt}\" src=\"{imageUri}{linkHash}\">");

                if (captionPlacement == "after" && !string.IsNullOrEmpty(captionValue))
                    sb.AppendLine($"<br><em>{captionValue}</em>");

                sb.AppendLine("</p>");
            }
            else
            {
                if (captionPlacement != "after" && !string.IsNullOrEmpty(captionValue))
                    sb.AppendLine($"*{captionValue}*\n");

                sb.AppendLine(imageMdSyntax);

                if (captionPlacement == "after" && !string.IsNullOrEmpty(captionValue))
                    sb.AppendLine($"\n*{captionValue}*");
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
