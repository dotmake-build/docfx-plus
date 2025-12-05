using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DotMake.DocfxPlus.Cli.Shfb
{
    internal class TopicCollection : Dictionary<string, Topic>
    {
        public TopicCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        public TopicCollection(string contentLayoutFile, string basePath)
            : this()
        {
            FilePath = contentLayoutFile;
            FileFullPath = (basePath == null)
                ? contentLayoutFile
                : Path.Combine(basePath, contentLayoutFile);

            using (var xr = XmlReader.Create(FileFullPath, new XmlReaderSettings { CloseInput = true }))
            {
                xr.MoveToContent();

                while (!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
                {
                    if (xr.NodeType == XmlNodeType.Element && xr.Name == "Topic")
                    {
                        var topic = new Topic(xr);

                        TryAdd(topic.Id, topic);
                    }

                    xr.Read();
                }
            }
        }

        public string FilePath { get; }

        public string FileFullPath { get; }

        public void SetTopicFiles(Dictionary<string, TopicFile> topicFiles)
        {
            foreach (var topic in Values)
            {
                if (topic.Id != null
                    && topicFiles.TryGetValue(topic.Id, out var topicFile))
                {
                    topic.TopicFile = topicFile;

                    //Assign only the first topic that references this file
                    if (topicFile.Topic == null)
                        topicFile.Topic = topic;
                }

                if(topic.SubTopics.Count != 0)
                    topic.SubTopics.SetTopicFiles(topicFiles);
            }
        }

        public IEnumerable<Tuple<Topic, int>> GetAllTopics(bool includeInvisibleItems = false, int level = 0)
        {
            foreach (var topic in Values)
            {
                if (!topic.Visible && !includeInvisibleItems)
                    continue;

                yield return Tuple.Create(topic, level);

                foreach (var subTopic in topic.SubTopics.GetAllTopics(includeInvisibleItems, level + 1))
                    yield return subTopic;
            }
        }

        public Topic GetApiContentInsertionPoint()
        {
            foreach (var topic in Values)
            {
                if (topic.ApiParentMode != ApiParentMode.None)
                    return topic;

                var childInsertionPoint = topic.SubTopics.GetApiContentInsertionPoint();

                if (childInsertionPoint != null)
                    return childInsertionPoint;
            }

            return null;
        }
    }
}
