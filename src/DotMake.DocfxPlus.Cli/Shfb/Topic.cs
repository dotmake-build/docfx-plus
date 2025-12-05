using System;
using System.IO;
using System.Xml;

namespace DotMake.DocfxPlus.Cli.Shfb;

internal class Topic
{
    public Topic(XmlReader xr)
    {
        var guid = xr.GetAttribute("id");

        if (!string.IsNullOrWhiteSpace(guid))
            Id = guid;
        else
            Id = Guid.NewGuid().ToString();

        Title = xr.GetAttribute("title");
        TocTitle = xr.GetAttribute("tocTitle");
        LinkText = xr.GetAttribute("linkText");

        if (bool.TryParse(xr.GetAttribute("noFile"), out var attrValue))
            NoFile = attrValue;

        if (bool.TryParse(xr.GetAttribute("visible"), out attrValue))
            Visible = attrValue;

        if (bool.TryParse(xr.GetAttribute("isDefault"), out attrValue))
            IsDefault = attrValue;

        if (bool.TryParse(xr.GetAttribute("isExpanded"), out attrValue))
            IsExpanded = attrValue;

        if (bool.TryParse(xr.GetAttribute("isSelected"), out attrValue))
            IsSelected = attrValue;

        if (Enum.TryParse(typeof(ApiParentMode), xr.GetAttribute("apiParentMode"), true, out var enumValue))
            ApiParentMode = (ApiParentMode)enumValue;

        if (!xr.IsEmptyElement)
            while (!xr.EOF)
            {
                xr.Read();

                if (xr.NodeType == XmlNodeType.EndElement && xr.Name == "Topic")
                    break;

                if (xr.NodeType == XmlNodeType.Element && xr.Name == "Topic")
                {
                    var subTopic = new Topic(xr);

                    SubTopics.TryAdd(subTopic.Id, subTopic);
                }
            }
    }

    public string Id { get; }

    public string Title { get; }

    public string TocTitle { get; }

    public string LinkText { get; }

    public bool NoFile { get; }

    public bool Visible { get; } = true;

    public bool IsDefault { get; }

    public bool IsExpanded { get; }

    public bool IsSelected { get; }

    public ApiParentMode ApiParentMode { get; }

    public TopicCollection SubTopics { get; } = new ();

    public TopicFile TopicFile { get; set; }

    public string DisplayTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(TocTitle))
                return TocTitle;

            if (!string.IsNullOrWhiteSpace(Title))
                return Title;

            if (TopicFile != null)
                return Path.GetFileNameWithoutExtension(TopicFile.FilePath);

            return "(Unknown topic)";
        }
    }
}
