using System;
using System.Collections.Generic;
using System.IO;

namespace DotMake.DocfxPlus.Cli.Util
{
    internal static class CodeLanguageUtil
    {
        private static readonly IReadOnlyDictionary<string, CodeLanguageInfo> CodeLanguageInfos =
            new Dictionary<string, CodeLanguageInfo>
        {
            { "actionscript", new CodeLanguageInfo { Extensions = ["as"], TabSize = 4 } },
            { "arduino",      new CodeLanguageInfo { Extensions = ["ino"], TabSize = 2 } },
            { "assembly",     new CodeLanguageInfo { Extensions = ["nasm", "asm"], TabSize = 4 } },
            { "batchfile",    new CodeLanguageInfo { Extensions = ["bat", "cmd"], TabSize = 4 } },
            { "css",          new CodeLanguageInfo { Extensions = [], TabSize = 2 } },
            { "cpp",          new CodeLanguageInfo { Extensions = ["c", "c++", "objective-c", "obj-c", "objc", "objectivec", "h", "hpp", "cc", "m"], TabSize = 4 } },
            { "csharp",       new CodeLanguageInfo { Extensions = ["cs"], TabSize = 4 } },
            { "cuda",         new CodeLanguageInfo { Extensions = ["cu", "cuh"], TabSize = 4 } },
            { "d",            new CodeLanguageInfo { Extensions = ["dlang"], TabSize = 4 } },
            { "everything",   new CodeLanguageInfo { Extensions = ["example"], TabSize = 4 } }, // catch-all
            { "erlang",       new CodeLanguageInfo { Extensions = ["erl"], TabSize = 2 } },
            { "fsharp",       new CodeLanguageInfo { Extensions = ["fs", "fsi", "fsx"], TabSize = 4 } },
            { "go",           new CodeLanguageInfo { Extensions = ["golang"], TabSize = 8 } },
            { "handlebars",   new CodeLanguageInfo { Extensions = ["hbs"], TabSize = 2 } },
            { "haskell",      new CodeLanguageInfo { Extensions = ["hs"], TabSize = 2 } },
            { "html",         new CodeLanguageInfo { Extensions = ["jsp", "asp", "aspx", "ascx"], TabSize = 2 } },
            { "cshtml",       new CodeLanguageInfo { Extensions = ["aspx-cs", "aspx-csharp"], TabSize = 2 } },
            { "vbhtml",       new CodeLanguageInfo { Extensions = ["aspx-vb"], TabSize = 2 } },
            { "java",         new CodeLanguageInfo { Extensions = ["gradle"], TabSize = 4 } },
            { "javascript",   new CodeLanguageInfo { Extensions = ["js", "node", "json"], TabSize = 2 } },
            { "lisp",         new CodeLanguageInfo { Extensions = ["lsp"], TabSize = 2 } },
            { "lua",          new CodeLanguageInfo { Extensions = [], TabSize = 2 } },
            { "matlab",       new CodeLanguageInfo { Extensions = [], TabSize = 4 } },
            { "pascal",       new CodeLanguageInfo { Extensions = ["pas"], TabSize = 2 } },
            { "perl",         new CodeLanguageInfo { Extensions = ["pl"], TabSize = 4 } },
            { "php",          new CodeLanguageInfo { Extensions = [], TabSize = 4 } },
            { "powershell",   new CodeLanguageInfo { Extensions = ["posh", "ps1"], TabSize = 4 } },
            { "processing",   new CodeLanguageInfo { Extensions = ["pde"], TabSize = 2 } },
            { "python",       new CodeLanguageInfo { Extensions = ["py"], TabSize = 4 } },
            { "r",            new CodeLanguageInfo { Extensions = [], TabSize = 2 } },
            { "react",        new CodeLanguageInfo { Extensions = ["tsx"], TabSize = 2 } },
            { "ruby",         new CodeLanguageInfo { Extensions = ["ru", "erb", "rb"], TabSize = 2 } },
            { "rust",         new CodeLanguageInfo { Extensions = ["rs"], TabSize = 4 } },
            { "scala",        new CodeLanguageInfo { Extensions = [], TabSize = 2 } },
            { "shell",        new CodeLanguageInfo { Extensions = ["sh", "bash"], TabSize = 2 } },
            { "smalltalk",    new CodeLanguageInfo { Extensions = ["st"], TabSize = 2 } },
            { "sql",          new CodeLanguageInfo { Extensions = [], TabSize = 2 } },
            { "swift",        new CodeLanguageInfo { Extensions = [], TabSize = 4 } },
            { "typescript",   new CodeLanguageInfo { Extensions = ["ts"], TabSize = 2 } },
            { "xaml",         new CodeLanguageInfo { Extensions = [], TabSize = 2 } },
            { "xml",          new CodeLanguageInfo { Extensions = ["xsl", "xslt", "xsd", "wsdl", "csdl", "edmx"], TabSize = 2 } },
            { "vb",           new CodeLanguageInfo { Extensions = ["vbnet", "vbscript", "bas", "vbs", "vba"], TabSize = 4 } },
            { "csproj",           new CodeLanguageInfo { Extensions = ["slnx", "config"], TabSize = 2 } }
        };

        private static readonly Dictionary<string, CodeLanguageInfo> FileExtensionMap = new(StringComparer.OrdinalIgnoreCase);

        static CodeLanguageUtil()
        {
            foreach (var kvp in CodeLanguageInfos)
            {
                var family = kvp.Key;
                var codeLanguageInfo = kvp.Value;
                codeLanguageInfo.Family = family;

                FileExtensionMap.TryAdd(family, codeLanguageInfo);
                foreach (var extension in codeLanguageInfo.Extensions)
                    FileExtensionMap.TryAdd(extension, codeLanguageInfo);
            }
        }

        public static int GetTabSize(string fileNameOrExtension)
        {
            fileNameOrExtension = Path.GetExtension(fileNameOrExtension)?.TrimStart('.');

            if (!string.IsNullOrEmpty(fileNameOrExtension)
                && FileExtensionMap.TryGetValue(fileNameOrExtension, out var codeLanguageInfo))
                return codeLanguageInfo.TabSize;

            return 4;
        }

        private class CodeLanguageInfo
        {
            public string Family { get; set; }

            public string[] Extensions { get; init; } = [];

            public int TabSize { get; init; } = 4; // sensible default
        }
    }
}
