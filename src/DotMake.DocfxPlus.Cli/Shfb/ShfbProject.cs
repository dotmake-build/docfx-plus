using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DotMake.DocfxPlus.Cli.Util;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;

namespace DotMake.DocfxPlus.Cli.Shfb
{
    internal class ShfbProject
    {
        private readonly Project project;
        //private readonly XNamespace ns;
        private readonly ILogger logger;

        public ShfbProject(string projectFile, ILogger logger = null)
        {
            BasePath = Path.GetDirectoryName(Path.GetFullPath(projectFile));
            this.logger = logger;

            project = new Project(
                projectFile,
                null,
                null,
                new ProjectCollection(),
                ProjectLoadSettings.IgnoreMissingImports
            );

            //Update: no more needed as we remove namespaces in GetPropertyValueAsXml for consistent handling
            //Find the namespace of project XML, it's usually "http://schemas.microsoft.com/developer/msbuild/2003"
            //but just in case. Unfortunately project.Xml does not provide access to namespace so we need to parse the whole xml
            //var root = XElement.Parse(project.Xml.RawXml);
            //ns = root.Name.Namespace;

            DocumentationSources = ParseDocumentationSources();
            HelpTitle = project.GetPropertyValue("HelpTitle");
            CopyrightText = project.GetPropertyValue("CopyrightText");
            FooterText = project.GetPropertyValue("FooterText");
            OutputPath = PathUtil.NormalizeRelativePath(project.GetPropertyValue("OutputPath"), BasePath);
            CodeBlockComponentBasePath = ParseCodeBlockComponentBasePath() ?? string.Empty;
            LogoFile = ParseLogoFile();
            ContentLayoutFiles = ParseContentLayoutFiles();
            NoneFiles = project.GetItems("None")
                .Select(contentLayout => NormalizeProjectItemPath(contentLayout, BasePath))
                .ToArray();
            ContentFiles = project.GetItems("Content")
                .Select(contentLayout => NormalizeProjectItemPath(contentLayout, BasePath))
                .ToArray();
            ImageFiles = ParseImageFiles();
            NamespaceSummaries = ParseNamespaceSummaries();

            logger?.LogInformation($"Loaded SHFB project file \"{projectFile}\"");
        }

        public string BasePath { get; }

        public string[] DocumentationSources { get; }

        public string HelpTitle { get; }

        public string CopyrightText { get; }

        public string FooterText { get; }

        public string OutputPath { get; }

        public string CodeBlockComponentBasePath { get; }

        public string LogoFile { get; }

        public string[] ContentLayoutFiles { get; }

        public string[] NoneFiles { get; }

        public string[] ContentFiles { get; }

        public ImageFile[] ImageFiles { get; }

        public Dictionary<string, string> NamespaceSummaries { get; }

        public List<TopicCollection> GetTopicCollections(out Dictionary<string, TopicFile> topicFiles)
        {
            topicFiles = new Dictionary<string, TopicFile>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in NoneFiles)
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();

                if (ext == ".aml")
                {
                    var topicFile = new TopicFile(file, BasePath);

                    if (topicFile.ErrorMessage != null)
                        logger?.LogWarning(topicFile.ErrorMessage);
                    else if (topicFile.Id != null)
                        topicFiles.TryAdd(topicFile.Id, topicFile);
                }
            }

            var topicCollections = new List<TopicCollection>();

            foreach (var file in ContentLayoutFiles)
            {
                var topicCollection = new TopicCollection(file, BasePath);

                topicCollection.SetTopicFiles(topicFiles);

                topicCollections.Add(topicCollection);
            }

            return topicCollections;
        }

        private string[] ParseDocumentationSources()
        {
            var documentationSources = new List<string>();

            var element = GetPropertyValueAsXml("DocumentationSources");

            // Now parse DocumentationSources with the correct namespace
            foreach (var source in element.Elements("DocumentationSource"))
            {
                var sourceFile = source.Attribute("sourceFile")?.Value;
                documentationSources.Add(PathUtil.NormalizeRelativePath(sourceFile, BasePath));
            }

            return documentationSources.ToArray();
        }

        private string[] ParseContentLayoutFiles()
        {
            var contentLayouts = project.GetItems("ContentLayout")
                .Select(contentLayout => new {
                    File = NormalizeProjectItemPath(contentLayout, BasePath),
                    SortOrder = int.TryParse(contentLayout.GetMetadataValue("SortOrder"), out var result)
                        ? result
                        : 0
                })
                .ToList();

            contentLayouts.Sort((x, y) =>
            {
                if (x.SortOrder < y.SortOrder)
                    return -1;

                if (x.SortOrder > y.SortOrder)
                    return 1;

                return string.Compare(x.File, y.File, StringComparison.OrdinalIgnoreCase);
            });

            return contentLayouts.Select(arg => arg.File).ToArray();
        }

        private string ParseCodeBlockComponentBasePath()
        {
            var element = GetPropertyValueAsXml("ComponentConfigurations");

            var componentConfig = element
                .Elements("ComponentConfig")
                .FirstOrDefault(c =>
                    string.Equals(c.Attribute("id")?.Value, "Code Block Component", StringComparison.OrdinalIgnoreCase));

            var component = componentConfig?
                .Elements("component")
                .FirstOrDefault(c =>
                    string.Equals(c.Attribute("id")?.Value, "Code Block Component", StringComparison.OrdinalIgnoreCase));

            var basePathValue = component?
                .Element("basePath")?
                .Attribute("value")?.Value;

            basePathValue = basePathValue?.Replace("{@HtmlEncProjectFolder}", ".\\");

            return PathUtil.NormalizeRelativePath(basePathValue, BasePath);
        }

        private string ParseLogoFile()
        {
            var element = GetPropertyValueAsXml("TransformComponentArguments");

            var argument = element
                .Elements("Argument")
                .FirstOrDefault(c => string.Equals(c.Attribute("Key")?.Value, "logoFile", StringComparison.OrdinalIgnoreCase));

            return argument?.Attribute("Value")?.Value;
        }

        private ImageFile[] ParseImageFiles()
        {
            return project.GetItems("Image")
                .Select(image => new ImageFile
                {
                    FilePath = NormalizeProjectItemPath(image, BasePath),
                    Id = image.GetMetadataValue("ImageId"),
                    AlternateText = image.GetMetadataValue("AlternateText")
                })
                .ToArray();
        }

        private Dictionary<string, string> ParseNamespaceSummaries()
        {
            var namespaceSummaries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var element = GetPropertyValueAsXml("NamespaceSummaries");

            // Now parse DocumentationSources with the correct namespace
            foreach (var item in element.Elements("NamespaceSummaryItem"))
            {
                var isDocumented = item.Attribute("isDocumented")?.Value.ToLowerInvariant() == "true";
                if (!isDocumented)
                    continue;

                var name = item.Attribute("name")?.Value
                    .Replace("(Group)", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var content = item.Value;
                if (string.IsNullOrWhiteSpace(content))
                    continue;

                namespaceSummaries.TryAdd(name, content);
            }

            return namespaceSummaries;
        }

        private XElement GetPropertyValueAsXml(string name)
        {
            var rawXml = project.GetPropertyValue(name);

            var wrapped = $"<{name}>{rawXml}</{name}>";

            var element = XElement.Parse(wrapped);

            return RemoveAllNamespaces(element);
        }

        private XElement RemoveAllNamespaces(XElement xml)
        {
            return new XElement(xml.Name.LocalName,
                xml.HasElements
                    ? xml.Elements().Select(RemoveAllNamespaces)
                    : xml.Value,
                xml.Attributes().Where(a => !a.IsNamespaceDeclaration)
            );
        }

        private static string NormalizeProjectItemPath(ProjectItem projectItem, string basePath)
        {
            var path = projectItem.EvaluatedInclude;
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (IsOutsideBasePath(path, basePath))
            {
                path = PathUtil.NormalizeRelativePath(path, basePath);

                var fileName = Path.GetFileName(path);

                var firstParent = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(firstParent))
                    firstParent = Path.GetFileName(firstParent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                path = !string.IsNullOrEmpty(firstParent) && firstParent != ".."
                    ? Path.Combine(firstParent, fileName)
                    : fileName;

                //Fix backslashes in source path for linux
                return path.Replace('\\', '/');
            }

            return PathUtil.NormalizeRelativePath(path, basePath);
        }

        private static bool IsOutsideBasePath(string path, string basePath)
        {
            // Normalize project root
            var baseFullPath = Path.GetFullPath(basePath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Resolve the candidate path relative to project root
            var fullPath = Path.GetFullPath(path, baseFullPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Compare with normalized project root
            return !fullPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
