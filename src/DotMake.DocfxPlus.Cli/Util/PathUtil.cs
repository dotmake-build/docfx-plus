using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DotMake.DocfxPlus.Cli.Util
{
    internal static class PathUtil
    {
        public static string NormalizeRelativePath(string path, string basePath)
        {
            if (path == null)
                return null;

            path = Path.GetFullPath(path, basePath);
            path = Path.GetRelativePath(basePath, path);

            //Fix backslashes in source path for linux
            return path.Replace('\\', '/');
        }

        public static string NormalizeRelativePath(string path, string oldBasePath, string newBasePath)
        {
            if (path == null || newBasePath == null)
                return path;

            path = Path.GetFullPath(path, oldBasePath);
            path = Path.GetRelativePath(newBasePath, path);

            //Fix backslashes in source path for linux
            return path.Replace('\\', '/');
        }

        public static string RebaseRelativePath(string path, string[] oldBases, string newBase)
        {
            foreach (var oldBase in oldBases)
            {
                if (path.StartsWith(oldBase + '/', StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith(oldBase + '\\', StringComparison.OrdinalIgnoreCase))
                    return string.IsNullOrWhiteSpace(newBase)
                        ? path.Substring(oldBase.Length + 1) //also remove slash
                        : newBase + path.Substring(oldBase.Length); //keep slash
            }

            return path;
        }

        public static string PathToSlug(string filePath)
        {
            // Remove drive letters (C:/ etc.)
            var root = Path.GetPathRoot(filePath);
            var relativePath = string.IsNullOrEmpty(root)
                ? filePath
                : filePath.Substring(root.Length);

            // Normalize separators to '/'
            relativePath = relativePath.Replace("\\", "/");

            // Split into segments
            var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Slugify each segment
            for (var i = 0; i < segments.Length; i++)
            {
                segments[i] = Slugify(segments[i]);
            }

            // Recombine into URL path
            return string.Join("/", segments);
        }

        private static string Slugify(string input)
        {
            input = input.ToLowerInvariant();

            // Replace spaces/underscores with dash
            input = Regex.Replace(input, @"[\s_]+", "-");

            // Keep only URL-safe characters: a-z, 0-9, '-', '.', '_', '~'
            input = Regex.Replace(input, @"[^a-z0-9\-._~]", "");

            // Collapse multiple dashes
            input = Regex.Replace(input, @"-+", "-");

            return input.Trim('-', '.');
        }
    }
}
