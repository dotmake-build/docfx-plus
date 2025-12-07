using System;
using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable ConstantConditionalAccessQualifier

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class AlertConverter : ConverterBase
    {
        public AlertConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("alert".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var alertClass = node.Attributes["class"]?.Value?.ToLowerInvariant();
            var title = node.Attributes["title"]?.Value.Chomp(all: true);
            string alertType;

            switch (alertClass)
            {
                case "note":
                case "implement":
                case "caller":
                case "inherit":
                case "todo":
                    alertType = "note";
                    break;

                case "tip":
                    alertType = "tip";
                    break;

                case "important":
                    alertType = "important";
                    break;

                case "caution":
                case "security":
                case "security note":
                    alertType = "caution";
                    break;

                case "warning":
                    alertType = "warning";
                    break;

                default:
                    alertType = "note";
                    break;
            }

            sb.AppendLine($"> [!{alertType}]");

            if (string.IsNullOrWhiteSpace(title)
                && !alertClass.Equals(alertType, StringComparison.OrdinalIgnoreCase))
                title = alertClass;

            if (!string.IsNullOrWhiteSpace(title))
                sb.AppendLine($"> **{title}**");

            var content = TreatChildren(node).Chomp();
            // get the lines based on carriage return and prefix "> " to each line
            foreach (var line in content.ReadLines())
            {
                sb.Append("> ");
                sb.AppendLine(line);
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
