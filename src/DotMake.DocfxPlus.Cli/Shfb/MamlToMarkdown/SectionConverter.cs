using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class SectionConverter : ConverterBase
    {
        public SectionConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("section".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            var level = node.Ancestors("sections").Count();

            var sb = new StringBuilder();
            sb.AppendLine();

            var address = node.GetAttributeValue("address", "");
            var title = node.Element("title");
            if (title != null)
            {
                var heading = new string('#', level + 2);
                sb.Append(heading);
                sb.Append(' ');

                if (!string.IsNullOrWhiteSpace(address))
                    sb.Append($"<a id=\"{address}\"></a> ");

                if (title != null)
                    sb.AppendLine(TreatChildren(title).Chomp(all: true));
            }
            // ReSharper disable HeuristicUnreachableCode
            else if (!string.IsNullOrWhiteSpace(address))
                sb.AppendLine($"<a id=\"{address}\"></a>");
            // ReSharper restore HeuristicUnreachableCode
            
            var content = node.Element("content");
            if (content != null)
            {
                sb.AppendLine(TreatChildren(content));
            }

            var sections = node.Element("sections");
            if (sections != null)
            {
                foreach (var section in sections.ChildNodes.Where(n => n.Name.ToLowerInvariant() == "section"))
                    sb.AppendLine(Convert(section));
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
