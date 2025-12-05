using System;
using System.Collections.Generic;
using System.Linq;
using DotMake.DocfxPlus.Cli.Util;
using HtmlAgilityPack;
using ReverseMarkdown;
using ReverseMarkdown.Converters;
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class TableConverter : ConverterBase
    {
        public TableConverter(Converter converter) : base(converter)
        {
            Converter.Register("table", this);
        }

        public override string Convert(HtmlNode node)
        {
            if (Converter.Config.SlackFlavored)
            {
                throw new Exception(node.Name);
            }

            //support nested tables
            if (node.Ancestors("table").Any())
                return HtmlNodeUtil.GetOuterHtmlWithCustomInnerHtml(node, TreatChildren(node).Chomp(all: true));

            var title = string.Empty;
            var titleNode = node.Element("title");
            if (titleNode != null)
            {
                title = $"{TreatChildren(titleNode).Chomp(all: true)}{Environment.NewLine}{Environment.NewLine}";
                titleNode.Remove();
            }

            // if table does not have a header row , add empty header row if set in config
            var useEmptyRowForHeader = Converter.Config.TableWithoutHeaderRowHandling ==
                                       Config.TableWithoutHeaderRowHandlingOption.EmptyRow;

            var emptyHeaderRow = HasNoTableHeaderRow(node) && useEmptyRowForHeader
                ? EmptyHeader(node)
                : string.Empty;

            return $"{Environment.NewLine}{Environment.NewLine}{title}{emptyHeaderRow}{TreatChildren(node)}{Environment.NewLine}";
        }

        private static bool HasNoTableHeaderRow(HtmlNode node)
        {
            var thNode = node.SelectNodes("./tr/th")?.FirstOrDefault();
            return thNode == null;
        }

        private static string EmptyHeader(HtmlNode node)
        {
            var firstRow = node.SelectNodes("./tr")?.FirstOrDefault();

            if (firstRow == null)
            {
                return string.Empty;
            }

            var colCount = firstRow.ChildNodes.Count(n => n.Name == "td" || n.Name == "th");

            var headerRowItems = new List<string>();
            var underlineRowItems = new List<string>();

            for (var i = 0; i < colCount; i++)
            {
                headerRowItems.Add("<!---->");
                underlineRowItems.Add("---");
            }

            var headerRow = $"| {string.Join(" | ", headerRowItems)} |{Environment.NewLine}";
            var underlineRow = $"| {string.Join(" | ", underlineRowItems)} |{Environment.NewLine}";

            return headerRow + underlineRow;
        }
    }
}
