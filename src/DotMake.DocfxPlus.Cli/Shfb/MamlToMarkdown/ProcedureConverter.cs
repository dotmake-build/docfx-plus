using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class ProcedureConverter : ConverterBase
    {
        public ProcedureConverter(Converter converter)
            : base(converter)
        {
            Converter.Register("procedure".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var title = node.Element("title");
            if (title != null)
            {
                sb.Append("### ");
                sb.AppendLine(TreatChildren(title).Chomp(all: true));
            }

            var steps = node.Element("steps");
            if (steps != null)
            {
                var ordered = steps.GetAttributeValue("class", "").Trim().ToLowerInvariant() == "ordered";
                var index = 1;

                foreach (var step in steps.ChildNodes.Where(n => n.Name.ToLowerInvariant() == "step"))
                {
                    var contentEl = step.Element("content");
                    if (contentEl != null)
                    {
                        var content = TreatChildren(contentEl).Chomp();
                        var prefix = (ordered ? $"{index++}." : Converter.Config.ListBulletChar.ToString()) + ' ';

                        var firstLine = true;
                        foreach (var line in content.ReadLines())
                        {
                            if (firstLine)
                            {
                                sb.Append(prefix);

                                firstLine = false;
                            }
                            else
                            {
                                sb.Append(new string(' ', prefix.Length));
                            }

                            sb.AppendLine(line);
                        }
                        sb.AppendLine();
                    }
                }
            }

            var conclusion = node.Element("conclusion");
            if (conclusion != null)
            {
                // ReSharper disable once ConstantNullCoalescingCondition
                sb.AppendLine(TreatChildren(conclusion.Element("content") ?? conclusion));
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
