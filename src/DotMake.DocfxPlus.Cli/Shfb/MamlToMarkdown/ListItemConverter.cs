using System;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable ConstantNullCoalescingCondition
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable ConvertTypeCheckPatternToNullCheck

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class ListItemConverter : ConverterBase
    {
        public ListItemConverter(Converter converter) : base(converter)
        {
            Converter.Register("listItem".ToLowerInvariant(), this);
        }

        public override string Convert(HtmlNode node)
        {
            // Standardize whitespace before inner lists so that the following are equivalent
            //   <li>Foo<ul><li>...
            //   <li>Foo\n    <ul><li>...
            /*foreach (var innerList in node.SelectNodes("//list") ?? Enumerable.Empty<HtmlNode>())
            {
                if (innerList.PreviousSibling?.NodeType == HtmlNodeType.Text)
                {
                    innerList.PreviousSibling.InnerHtml = innerList.PreviousSibling.InnerHtml.Chomp();
                }
            }*/

            var content = ContentFor(node).Chomp();
            //var indentation = IndentationFor(node);
            var prefix = PrefixFor(node);

            var sb = new StringBuilder();
            var firstLine = true;
            foreach (var line in content.ReadLines())
            {
                //sb.Append(indentation);

                if (firstLine)
                {
                    sb.Append(prefix);

                    firstLine = false;
                }
                else
                {
                    sb.Append(new string(' ', prefix.Length));
                    //if (indentation.Length > prefix.Length)
                    //    sb.Append(new string(' ', indentation.Length - prefix.Length));
                }

                sb.AppendLine(line);
            }
            sb.AppendLine();

            //return $"{indentation}{prefix}{content.Chomp()}{Environment.NewLine}";
            return sb.ToString();
        }

        private string PrefixFor(HtmlNode node)
        {
            if (node.ParentNode != null && node.ParentNode.Name == "list"
                && node.ParentNode.GetAttributeValue("class", "").ToLowerInvariant() == "ordered")
            {
                var start = node.ParentNode.GetAttributeValue("start", 0);
                if (start > 0)
                    start--;
                // index are zero based hence add one
                var index = start + node.ParentNode.SelectNodes("./listitem").IndexOf(node) + 1;
                return $"{index}. ";
            }
            else
            {
                return $"{Converter.Config.ListBulletChar} ";
            }
        }

        private string ContentFor(HtmlNode node)
        {
            if (!Converter.Config.GithubFlavored)
                return TreatChildren(node);

            var content = new StringBuilder();

            if (node.FirstChild is HtmlNode childNode
                && childNode.Name == "input"
                && childNode.GetAttributeValue("type", "").Equals("checkbox", StringComparison.OrdinalIgnoreCase))
            {
                content.Append(childNode.Attributes.Contains("checked")
                    ? $"[x]"
                    : $"[ ]");

                node.RemoveChild(childNode);
            }

            content.Append(TreatChildren(node));
            return content.ToString();
        }

        /*
        private string IndentationFor(HtmlNode node)
        {
            var length = 0;
            var parent = node.Ancestors("listitem").FirstOrDefault();
            if (parent != null)
                length = PrefixFor(parent).Length;

            
            //var length = node.Ancestors("listitem")?
            //    .Sum(l => PrefixFor(l).Length) ?? 0;

            // li not required to have a parent ol/ul
            if (length == 0)
                return string.Empty;

            return new string('x', length);
        }
        */
    }
}
