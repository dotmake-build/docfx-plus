using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode

namespace DotMake.DocfxPlus.Cli.Util
{
    internal class HtmlNodeUtil
    {
        public static void UnindentChildren(HtmlNode node)
        {
            var ignoreParentTags = new[] { "code", "pre" };

            foreach (var childNode in node.Descendants())
            {
                if (childNode.NodeType == HtmlNodeType.Text
                    && !ignoreParentTags.Contains(childNode.ParentNode.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var lines = StringUtil.ReadLines(childNode.InnerHtml)
                        .Select(line => line.TrimStart());

                    childNode.InnerHtml = string.Join("\n", lines);
                }
            }
        }

        public static void ReplaceMamlTags(HtmlNode node)
        {
            var doc = node.OwnerDocument;

            // Map of MAML tags to standard HTML tags
            var tagMap = new Dictionary<string, string>
            {
                { "tableHeader", "thead" },
                { "row", "tr" },
                { "entry", "td" }
            };

            foreach (var kvp in tagMap)
            {
                var childNodes = node.SelectNodes($".//{kvp.Key.ToLowerInvariant()}");
                if (childNodes == null)
                    continue;

                foreach (var childNode in childNodes)
                {
                    // Create new node with target tag name
                    var newNode = doc.CreateElement(kvp.Value);

                    // Copy attributes
                    foreach (var attr in childNode.Attributes)
                        newNode.Attributes.Add(attr.Name, attr.Value);

                    // Move children
                    foreach (var child in childNode.ChildNodes)
                        newNode.AppendChild(child);

                    // Replace old node
                    childNode.ParentNode.ReplaceChild(newNode, childNode);
                }
            }
        }

        public static string GetOuterHtmlWithCustomInnerHtml(HtmlNode node, string innerHtml = "")
        {
            var tagName = node.Name;
            var attributes = string.Join(" ", node.Attributes.Select(a => $"{a.Name}=\"{a.Value}\""));
            var openTag = string.IsNullOrEmpty(attributes) ? $"<{tagName}>" : $"<{tagName} {attributes}>";
            var closeTag = $"</{tagName}>";

            return openTag + innerHtml + closeTag;
        }
    }
}
