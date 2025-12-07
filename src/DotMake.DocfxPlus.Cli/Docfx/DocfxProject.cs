using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotMake.DocfxPlus.Cli.Shfb;
using DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown;
using DotMake.DocfxPlus.Cli.Util;
using Microsoft.Extensions.Logging;

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal class DocfxProject
    {
        private readonly string docfxOutputPath;
        private readonly ILogger logger;
        private readonly DocfxProjectOptions options;


        public DocfxProject(string docfxOutputPath, DocfxProjectOptions options, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(docfxOutputPath))
                throw new ArgumentNullException(nameof(docfxOutputPath));

            this.docfxOutputPath = Path.GetFullPath(docfxOutputPath);
            this.options = options;
            this.logger = logger;
        }

        public string[] MetadataFiles { get; private set; }

        public string CodeSourceBasePath { get; private set; }

        public string[] ResourceFiles { get; private set; }

        public string AppName { get; private set; }

        public string AppLogoPath { get; private set; }

        public string AppFaviconPath { get; private set; }

        public string AppFooter { get; private set; }

        private class FileMap
        {
            public Dictionary<string, string> TopicFiles { get; init; }

            public Dictionary<string, string> ContentFiles { get; init; }

            public Dictionary<string, string> ImageFiles { get; init; }
        }

        public void ConvertFrom(ShfbProject shfbProject)
        {
            var topicCollections = shfbProject.GetTopicCollections(out var topicFiles);

            var fileMap = new FileMap
            {
                //Rebase files in "content/" subfolder, we want to serve them from "docs/"
                TopicFiles = topicFiles.ToDictionary(
                    kvp => kvp.Value.FilePath,
                    kvp => $"{options.DocsLocation}/" +
                           (options.RebaseContent
                               ? PathUtil.RebaseRelativePath(kvp.Value.FilePathForUrl, ["content"], "")
                               : kvp.Value.FilePathForUrl),
                    StringComparer.OrdinalIgnoreCase),
                ContentFiles = shfbProject.ContentFiles.ToDictionary(
                    f => f,
                    f => options.RebaseImages
                        ? PathUtil.RebaseRelativePath(f, ["icons", "media"], options.ImagesLocation)
                        : f,
                    StringComparer.OrdinalIgnoreCase),
                ImageFiles = shfbProject.ImageFiles.ToDictionary(
                    i => i.FilePath,
                    i => options.RebaseImages
                        ? PathUtil.RebaseRelativePath(i.FilePath, ["icons", "media"], options.ImagesLocation)
                        : i.FilePath,
                    StringComparer.OrdinalIgnoreCase)
            };

            //Change the path of files to be relative to the docfx project.
            MetadataFiles = shfbProject.DocumentationSources
                .Select(f => PathUtil.NormalizeRelativePath(f, shfbProject.BasePath, docfxOutputPath))
                .ToArray();
            CodeSourceBasePath = PathUtil.NormalizeRelativePath(shfbProject.CodeBlockComponentBasePath, shfbProject.BasePath, docfxOutputPath);
            ResourceFiles = fileMap.ContentFiles.Values
                .Concat(fileMap.ImageFiles.Values)
                .ToArray();

            AppName = shfbProject.HelpTitle;
            AppLogoPath = fileMap.ContentFiles.Values
                .FirstOrDefault(f => Path.GetFileName(f).Equals(shfbProject.LogoFile, StringComparison.OrdinalIgnoreCase));
            AppFaviconPath = fileMap.ContentFiles.Values
                .FirstOrDefault(f => Path.GetFileName(f).Equals("favicon.ico", StringComparison.OrdinalIgnoreCase));
            //_appTitle = name,
            AppFooter = string.Join("<br>", new[] { shfbProject.FooterText, shfbProject.CopyrightText }
                .Where(s => !string.IsNullOrWhiteSpace(s)));


            var defaultTopic = CreateTocFiles(topicCollections, fileMap);

            ConvertTopicFiles(shfbProject, topicFiles, fileMap);

            CreateIndexMd(defaultTopic, fileMap);

            CopyResourceFiles(shfbProject, fileMap);

            CreateOverwriteFiles(shfbProject);

            Save();
        }

        private Topic CreateTocFiles(List<TopicCollection> topicCollections, FileMap fileMap)
        {
            const string apiTitle = "API Reference";
            var apiTocHref = $"../{options.ApiLocation}/toc.yml";
            Directory.CreateDirectory(Path.Combine(docfxOutputPath, options.DocsLocation));

            var noApi = (MetadataFiles.Length == 0) ? "# " : "";

            using (var tocFileWriter = new StreamWriter(Path.Combine(docfxOutputPath, "toc.yml")))
            {
                tocFileWriter.WriteLine(
                    $"""
                     - name: Docs
                       href: {options.DocsLocation}/
                     {noApi}- name: API
                     {noApi}  href: {options.ApiLocation}/
                     """);
            }
            logger?.LogInformation("Created TOC file \"toc.yml\"");

            Topic defaultTopic = null;
            var apiContentInsertionPoint = topicCollections
                .Select(tc => tc.GetApiContentInsertionPoint())
                .FirstOrDefault(t => t != null);

            using (var tocFileWriter = new StreamWriter(Path.Combine(docfxOutputPath, options.DocsLocation, "toc.yml")))
            {
                var lastLevel = 0;

                var visibleTopicsWithLevel = topicCollections
                    .SelectMany(topicCollection => topicCollection.GetAllTopics());

                foreach (var topicWithLevel in visibleTopicsWithLevel)
                {
                    var topic = topicWithLevel.Item1;
                    var level = topicWithLevel.Item2;

                    var indent = new string(' ', level * 2);
                    var lines = new List<string>();

                    if (level > lastLevel)
                        tocFileWriter.WriteLine($"{indent}items:");

                    if (topic == apiContentInsertionPoint && topic!.ApiParentMode == ApiParentMode.InsertBefore)
                    {
                        lines.Add($"name: {apiTitle}");
                        lines.Add($"href: {apiTocHref}");
                        lines.Add(null); //to force new item
                    }

                    lines.Add($"name: \"{topic.DisplayTitle}\"");
                    if (topic == apiContentInsertionPoint && topic!.ApiParentMode == ApiParentMode.InsertAsChild)
                        lines.Add($"href: {apiTocHref}");
                    else if (topic.TopicFile != null)
                        lines.Add($"href: ~/{fileMap.TopicFiles[topic.TopicFile.FilePath]}.md");
                    //Ignore this as SHFB docs say "Used by the editor for binding in the tree view."
                    //if (topic.IsExpanded)
                    //    lines.Add("expanded: true");

                    if (topic == apiContentInsertionPoint && topic!.ApiParentMode == ApiParentMode.InsertAfter)
                    {
                        lines.Add(null); //to force new item
                        lines.Add($"name: {apiTitle}");
                        lines.Add($"href: {apiTocHref}");
                    }

                    var first = true;
                    foreach (var line in lines)
                    {
                        if (line == null)
                        {
                            first = true;
                            continue;
                        }

                        tocFileWriter.Write(indent);

                        if (first)
                        {
                            tocFileWriter.Write("- ");
                            first = false;
                        }
                        else
                            tocFileWriter.Write("  ");

                        tocFileWriter.WriteLine(line);
                    }

                    lastLevel = level;

                    if (defaultTopic == null || topic.IsDefault)
                    {
                        defaultTopic = topic;
                    }
                }
            }
            logger?.LogInformation($"Created TOC file \"{options.DocsLocation}/toc.yml\"");

            return defaultTopic;
        }

        private void ConvertTopicFiles(ShfbProject shfbProject, Dictionary<string, TopicFile> topicFiles, FileMap fileMap)
        {
            var converterContext = new MamlToMarkdownConverterContext
            {
                XlinkHrefMap = topicFiles.ToDictionary(
                    kvp => kvp.Key,
                    kvp => $"~/{fileMap.TopicFiles[kvp.Value.FilePath]}.md",
                    StringComparer.OrdinalIgnoreCase),
                XlinkTitleMap = topicFiles.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.DisplayTitle,
                    StringComparer.OrdinalIgnoreCase),
                ImageXlinkHrefMap = shfbProject.ImageFiles.ToDictionary(
                    imageFile => imageFile.Id,
                    imageFile => "~/" + fileMap.ImageFiles[imageFile.FilePath],
                    StringComparer.OrdinalIgnoreCase),
                ImageXlinkAltMap = shfbProject.ImageFiles.ToDictionary(
                    imageFile => imageFile.Id,
                    imageFile => imageFile.AlternateText,
                    StringComparer.OrdinalIgnoreCase),
                NormalizeCodePath = (path) => NormalizeCodePath(path, shfbProject, CodeSourceBasePath, docfxOutputPath)
            };
            var mamlToMarkdownConverter = new MamlToMarkdownConverter(converterContext);

            var uniqueTopicFiles = topicFiles.Values
                .DistinctBy(topicFile => topicFile.FileFullPath, StringComparer.OrdinalIgnoreCase);

            foreach (var topicFile in uniqueTopicFiles)
            {
                var markdownFileRelative = fileMap.TopicFiles[topicFile.FilePath] + ".md";
                var markdownFile = Path.Combine(docfxOutputPath, markdownFileRelative);

                try
                {
                    var parent = Path.GetDirectoryName(markdownFile);
                    if (!string.IsNullOrEmpty(parent))
                        Directory.CreateDirectory(parent);

                    var maml = File.ReadAllText(topicFile.FileFullPath);
                    var markdown = mamlToMarkdownConverter.Convert(maml);

                    using (var markdownFileWriter = new StreamWriter(markdownFile))
                    {
                        markdownFileWriter.WriteLine($"# {topicFile.DisplayTitle}");
                        markdownFileWriter.WriteLine();

                        markdownFileWriter.WriteLine(markdown);
                    }

                    logger?.LogInformation($"Converted topic file \"{topicFile.FilePath}\" to \"{markdownFileRelative}\"");
                }
                catch (Exception ex)
                {
                    logger?.LogWarning($"{markdownFileRelative} -> {ex.Message}");
                }
            }
        }

        private void CreateIndexMd(Topic defaultTopic, FileMap fileMap)
        {
            using (var fileWriter = new StreamWriter(Path.Combine(docfxOutputPath, "index.md")))
            {
                if (defaultTopic != null && defaultTopic.TopicFile != null)
                    fileWriter.WriteLine(
                        $"""
                         ---
                         redirect_url: {fileMap.TopicFiles[defaultTopic.TopicFile.FilePath]}.html
                         ---
                         """);
                else
                    fileWriter.WriteLine("No default topic file found.");
            }

            logger?.LogInformation("Created index file \"index.md\"");
        }

        private void CreateOverwriteFiles(ShfbProject shfbProject)
        {
            foreach (var kvp in shfbProject.NamespaceSummaries)
            {
                var name = kvp.Key;
                var summary = kvp.Value;

                var overwriteRelativeFile = $"{options.OverwritesLocation}/{name}.md";
                var overwriteFile = Path.Combine(docfxOutputPath, overwriteRelativeFile);

                var parent = Path.GetDirectoryName(overwriteFile);
                if (!string.IsNullOrEmpty(parent))
                    Directory.CreateDirectory(parent);

                using (var fileWriter = new StreamWriter(overwriteFile))
                {
                    fileWriter.WriteLine(
                        $"""
                         ---
                         uid: {name}
                         summary: *content
                         ---
                         {summary}
                         """);
                }

                logger?.LogInformation($"Created overwrite file \"{overwriteRelativeFile}\"");
            }
        }


        private void CopyResourceFiles(ShfbProject shfbProject, FileMap fileMap)
        {
            var uniqueContentFiles = fileMap.ContentFiles
                .Concat(fileMap.ImageFiles)
                .DistinctBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in uniqueContentFiles)
            {
                var contentFile = kvp.Key;
                var destFileRelative = kvp.Value;

                var destFile = Path.Combine(docfxOutputPath, destFileRelative);

                var parent = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(parent))
                    Directory.CreateDirectory(parent);

                File.Copy(Path.Combine(shfbProject.BasePath, contentFile), destFile, true);

                logger?.LogInformation($"Copied content file \"{contentFile}\" to \"{destFileRelative}\"");
            }
        }

        private void Save()
        {
            var docfxConfig = new Dictionary<string, object>
            {
                { "$schema", "https://raw.githubusercontent.com/dotnet/docfx/main/schemas/docfx.schema.json" },
                {
                    "metadata", new[]
                    {
                        new
                        {
                            src = MetadataFiles.Select(s => new
                            {
                                src = Path.GetDirectoryName(s),
                                files = Path.GetFileName(s)
                            }),
                            dest = options.ApiLocation,
                            codeSourceBasePath = CodeSourceBasePath,
                            memberLayout = "separatePages",
                            categoryLayout = "nested"
                        }
                    }
                },
                {
                    "build", new
                    {
                        content = new[]
                        {
                            new
                            {
                                files = new[] { "**/*.{md,yml}" },
                                exclude = new[] { "_site/**", $"{options.OverwritesLocation}/**" }
                            }
                        },
                        resource = new[]
                        {
                            new
                            {
                                files =  new [] { "images/**" }
                                    .Concat(ResourceFiles.Select(path =>
                                    {
                                        var parts = path.Split('/');
                                        return parts.Length > 1 ? parts[0] + "/**" : path;
                                    }))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                            }
                        },
                        overwrite = new[]
                        {
                            new { files = new[] { $"{options.OverwritesLocation}/*.{{md,yml}}" } }
                        },
                        xref = new[]
                        {
                            "https://learn.microsoft.com/en-us/dotnet/.xrefmap.json"
                        },
                        output = "_site",
                        template = new[] { "default", "modern", "docfx-plus" },
                        globalMetadata = new
                        {
                            _appName = AppName,
                            _appLogoPath = AppLogoPath,
                            _appFaviconPath = AppFaviconPath,
                            //_appTitle = name,
                            _appFooter = AppFooter,
                            _enableSearch = true,
                            //pdf,
                        }
                    }
                }
            };


            using (var stream = File.Create(Path.Combine(docfxOutputPath, options.ProjectFileName)))
            {
                JsonSerializer.Serialize(stream, docfxConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
            }
            logger?.LogInformation("Created config file \"docfx.json\"");
        }

        private static string NormalizeCodePath(string path, ShfbProject shfbProject, string codeSourceBasePath, string docfxOutputPath)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (!string.IsNullOrEmpty(shfbProject.CodeBlockComponentBasePath))
            {
                path = PathUtil.NormalizeRelativePath(
                    path,
                    Path.GetFullPath(shfbProject.CodeBlockComponentBasePath, shfbProject.BasePath),
                    Path.GetFullPath(codeSourceBasePath, docfxOutputPath)
                );

                if (!Path.IsPathFullyQualified(path))
                    path = "~~/" + path;
            }
            else
            {
                path = PathUtil.NormalizeRelativePath(path, shfbProject.BasePath, docfxOutputPath);

                if (!Path.IsPathFullyQualified(path))
                    path = "~/" + path;
            }

            return path;
        }
    }
}
