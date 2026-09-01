using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Docfx.Plugins;

namespace DotMake.DocfxPlus.Plugin
{
    [Export(nameof(DocfxPlus), typeof(IPostProcessor))]
    internal class DocfxPlusPostProcessor : IPostProcessor
    {
        //paths are case-insensitive in Windows and OSX but case-sensitive in Linux
        private static readonly StringComparison PathComparison = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        private bool enableOfflineMode;
        private string appLogoPath;
        private string appFaviconPath;

        public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
        {
            //Console.WriteLine($"{nameof(DocfxPlusPostProcessor)}.{nameof(PrepareMetadata)} is run!");

            if (metadata.TryGetValue("_enableOfflineMode", out var value)
                && value is true)
                enableOfflineMode = true;

            if (metadata.TryGetValue("_appLogoPath", out value)
                && value is string)
                appLogoPath = (string)value;

            if (metadata.TryGetValue("_appFaviconPath", out value)
                && value is string)
                appFaviconPath = (string)value;

            return metadata;
        }

        public Manifest Process(Manifest manifest, string outputFolder, CancellationToken cancellationToken)
        {
            //Console.WriteLine($"{nameof(DocfxPlusPostProcessor)}.{nameof(Process)} is run!");

            if (!"logo.svg".Equals(appLogoPath, PathComparison))
                DeleteUnusedFile(outputFolder, "logo.svg");
            if (!"favicon.ico".Equals(appFaviconPath, PathComparison))
                DeleteUnusedFile(outputFolder, "favicon.ico");

            DeleteUnusedFiles(outputFolder, @"(^|/)toc\.html$");

            if (enableOfflineMode)
            {
                DeleteUnusedFiles(outputFolder, @"^public/[^/]+\.js(\.map)?$", @"^public/common\.js");

                ConvertToJsFile(
                    outputFolder,
                    "search-stopwords.json",
                    content => string.Concat(
                        """
                        window.docfx = window.docfx || {};

                        window.docfx.stopWordsJson = 
                        """,
                        content,
                        """
                        ;
                        """
                    )
                );

                ConvertToJsFile(
                    outputFolder,
                    "public/offline/search-worker.min.js",
                    content => string.Concat(
                        """
                        window.docfx = window.docfx || {};

                        window.docfx.searchWorkerCode = `
                        """,
                        content
                            // 1. Escape backslashes first so we don't accidentally double-escape later additions
                            .Replace(@"\", @"\\")
                            // 2. Escape backticks so they don't break the template literal container boundary
                            .Replace("`", @"\`"),
                        """
                        `;
                        """
                    )
                );
            }
            else
            {
                DeleteUnusedFolder(outputFolder, "public/offline");

                DeleteUnusedFile(outputFolder, "search-stopwords.json");
            }


            if (manifest.Files != null)
            {
                foreach (var manifestItem in manifest.Files)
                {
                    foreach (var output in manifestItem.Output.Values)
                    {
                        var relativePath = output.RelativePath;

                        //Console.WriteLine($"Post processor DocfxPlus: \"{relativePath}\"");

                        if (!enableOfflineMode)
                            continue;

                        if (relativePath.Equals("toc.json", PathComparison)
                            || relativePath.EndsWith("/toc.json", PathComparison))
                        {
                            ConvertToJsFile(
                                outputFolder,
                                relativePath,
                                content => string.Concat(
                                    """
                                    window.docfx = window.docfx || {};
                                    window.docfx.tocList = window.docfx.tocList || [];

                                    window.docfx.tocList.push(
                                    """,
                                    content,
                                    """
                                    );
                                    """
                                )
                            );

                            continue;
                        }

                        if (relativePath.Equals("index.json", PathComparison))
                        {
                            ConvertToJsFile(
                                outputFolder,
                                relativePath,
                                content => string.Concat(
                                    """
                                    window.docfx = window.docfx || {};

                                    window.docfx.indexJson = 
                                    """,
                                    content,
                                    """
                                    ;
                                    """
                                )
                            );

                            continue;
                        }
                    }
                }
            }

            return manifest;
        }

        private static void DeleteUnusedFile(string outputFolder, string relativePath)
        {
            var fileInfo = new FileInfo(Path.Combine(outputFolder, relativePath));

            if (!fileInfo.Exists)
                return;

            relativePath = relativePath.Replace('\\', '/');

            Console.WriteLine($"Post processor DocfxPlus: Deleting unused file \"{relativePath}\"");

            fileInfo.Delete();
        }

        private static void DeleteUnusedFiles(
            string outputFolder,
            [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
            [StringSyntax(StringSyntaxAttribute.Regex)] string negativePattern = null)
        {
            var list = new List<(FileInfo, string)>();

            outputFolder = Path.GetFullPath(outputFolder);
            foreach (var fileInfo in new DirectoryInfo(outputFolder).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(outputFolder, fileInfo.FullName)
                    .Replace('\\', '/');

                if (!Regex.IsMatch(relativePath, pattern))
                    continue;

                if (negativePattern != null && Regex.IsMatch(relativePath, negativePattern))
                    continue;

                list.Add((fileInfo, relativePath));
            }

            Console.WriteLine($"Post processor DocfxPlus: Deleting {list.Count} unused files which match \"{pattern}\" and not \"{negativePattern}\"");

            foreach (var (fileInfo, relativePath) in list)
                fileInfo.Delete();
        }

        private static void DeleteUnusedFolder(string outputFolder, string relativePath)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(outputFolder, relativePath));

            if (!directoryInfo.Exists)
                return;

            relativePath = relativePath.Replace('\\', '/');

            Console.WriteLine($"Post processor DocfxPlus: Deleting unused folder \"{relativePath}\"");

            directoryInfo.Delete(true);
        }

        private static void ConvertToJsFile(string outputFolder, string relativePath, Func<string, string> convertFunc)
        {
            var fileInfo = new FileInfo(Path.Combine(outputFolder, relativePath));

            if (!fileInfo.Exists)
                return;

            var newRelativePath = Path.ChangeExtension(relativePath, ".js");

            Console.WriteLine($"Post processor DocfxPlus: Converting \"{relativePath}\" to \"{newRelativePath}\"");

            File.WriteAllText(
                Path.Combine(outputFolder, newRelativePath),
                convertFunc(File.ReadAllText(fileInfo.FullName))
            );

            if (!newRelativePath.Equals(relativePath, PathComparison))
                fileInfo.Delete();
        }
    }
}

