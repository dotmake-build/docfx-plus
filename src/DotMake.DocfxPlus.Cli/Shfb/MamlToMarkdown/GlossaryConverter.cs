using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class GlossaryConverter : ConverterBase
    {
        public GlossaryConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("glossary".ToLowerInvariant(), this);
            Converter.Register("glossaryDiv".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            var level = node.Ancestors("glossary").Count();

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

            foreach (var glossaryEntry in node.ChildNodes.Where(n => n.Name.ToLowerInvariant() == "glossaryEntry".ToLowerInvariant()))
            {
                var terms = glossaryEntry.Element("terms");
                if (terms != null)
                {
                    var termList = new List<string>();
                    var termIdList = new List<string>();
                    foreach (var term in terms.Elements("term"))
                    {
                        termList.Add(term.InnerText.Chomp(all: true));
                        var id = term.GetAttributeValue("termId", "");
                        if (!string.IsNullOrWhiteSpace(id))
                            termIdList.Add(id);
                    }

                    var heading = new string('#', level + 3);
                    sb.Append(heading);
                    sb.Append(' ');

                    foreach (var termId in termIdList)
                        sb.Append($"<a id=\"{termId}\"></a> ");

                    sb.AppendLine(string.Join(", ", termList));
                }

                var definition = glossaryEntry.Element("definition");
                if (definition != null)
                {
                    sb.AppendLine(TreatChildren(definition));
                }

                var relatedEntryLinks = new List<string>();
                foreach (var relatedEntry in glossaryEntry.Elements("relatedEntry".ToLowerInvariant()))
                {
                    var id = relatedEntry.GetAttributeValue("termId", "");
                    if (!string.IsNullOrWhiteSpace(id))
                        relatedEntryLinks.Add($"<a href=\"#{id}\">{id}</a>");
                }
                if (relatedEntryLinks.Count > 0)
                {
                    sb.Append("See Also: ");
                    sb.AppendLine(string.Join(", ", relatedEntryLinks));
                }
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
