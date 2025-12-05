using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Docfx.MarkdigEngine.Extensions;
using DotMake.DocfxPlus.Cli.Util;
using HarmonyLib;
using Markdig.Renderers;

// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal class HtmlCodeSnippetRenderer
    {
        private static readonly object lockObj = new();
        private static bool staticStateChanged;

        private static Dictionary<string, string[]>  s_languageAlias;
        private static Dictionary<string, string> s_languageByFileExtension;
        private static HashSet<CodeSnippetExtractor> s_defaultExtractors;
        private static Dictionary<string, HashSet<CodeSnippetExtractor>> s_languageExtractors;

        // VB code snippet Region block: start -> # Region "snippetname", end -> # End Region
        private const string VBCodeSnippetRegionRegionStartLineTemplate = "#region\"{tagname}\"";
        private const string VBCodeSnippetRegionRegionEndLineTemplate = "#endregion";

        // Mainly for JS
        private const string CFamilyCodeSnippetCommentStartLineTemplate = "//#region{tagname}";
        private const string CFamilyCodeSnippetCommentEndLineTemplate = "//#endregion";

        private const string MarkupLanguageFamilyCodeSnippetCommentStartLineTemplate = "<!--#region{tagname}-->";
        private const string MarkupLanguageFamilyCodeSnippetCommentEndLineTemplate = "<!--#endregion-->";


        public static void Write(HtmlRenderer renderer, CodeSnippet codeSnippet)
        {
            codeSnippet.CodePath = WebUtility.UrlDecode(codeSnippet.CodePath);
            codeSnippet.TagName = WebUtility.UrlDecode(codeSnippet.TagName);
        }

        public static void GetContent(ref string content, CodeSnippet obj, object __instance)
        {
            lock (lockObj)
            {
                if (!staticStateChanged)
                {
                    var htmlCodeSnippetRendererType = __instance.GetType();
                    s_languageAlias = AccessTools.Field(htmlCodeSnippetRendererType, "s_languageAlias")
                        .GetValue(null) as Dictionary<string, string[]>;
                    s_languageByFileExtension = AccessTools.Field(htmlCodeSnippetRendererType, "s_languageByFileExtension")
                        .GetValue(null) as Dictionary<string, string>;
                    s_defaultExtractors = AccessTools.Field(htmlCodeSnippetRendererType, "s_defaultExtractors")
                        .GetValue(null) as HashSet<CodeSnippetExtractor>;
                    s_languageExtractors = AccessTools.Field(htmlCodeSnippetRendererType, "s_languageExtractors")
                        .GetValue(null) as Dictionary<string, HashSet<CodeSnippetExtractor>>;

                    s_languageAlias!["xml"] = s_languageAlias["xml"].Concat(["csproj", "slnx", "config"]).ToArray();
                    s_languageByFileExtension!.Clear();
                    BuildFileExtensionLanguageMap();

                    //Important last parameter for CodeSnippetExtractor should be false if end template does not contain tag
                    //For example when end template is "#endregion" and not "</{tagname}>"
                    AddExtractorItems(["vb"],
                        new CodeSnippetExtractor(VBCodeSnippetRegionRegionStartLineTemplate, VBCodeSnippetRegionRegionEndLineTemplate, false));
                    AddExtractorItems(["actionscript", "arduino", "assembly", "cpp", "csharp", "cshtml", "cuda", "d", "fsharp", "go", "java", "javascript", "objectivec", "pascal", "php", "processing", "react", "rust", "scala", "smalltalk", "swift", "typescript"],
                        new CodeSnippetExtractor(CFamilyCodeSnippetCommentStartLineTemplate, CFamilyCodeSnippetCommentEndLineTemplate, false));
                    AddExtractorItems(["xml", "xaml", "handlebars", "html", "cshtml", "php", "react", "ruby", "vbhtml"],
                        new CodeSnippetExtractor(MarkupLanguageFamilyCodeSnippetCommentStartLineTemplate, MarkupLanguageFamilyCodeSnippetCommentEndLineTemplate, false));

                    staticStateChanged = true;
                }
            }

            //Update: Need to patch CodeSnippetExtractor.MatchTag anyway for allowing spaces inside tag names so will path this also there
            //CodeSnippetExtractor.MatchTag calls ToLower() on found tag name instead of ToLowerInvariant()
            //Even if result dictionary is OrdinalIgnoreCase, ToLower() causes turkish-I problem
            //so mimic ToLower() before there so that the tag is found is result dictionary.
            //obj.TagName = obj.TagName?.ToLower();

            /*
            if (obj.Language == "vb")
            {
                var extractor = s_languageExtractors["vb"].Last();
                var allLines = StringUtil.ReadLines(content).ToArray();

                //var tagWithPrefix = extractor.TagPrefix + obj.TagName;
                HashSet<int> tagLines = [];
                var tagToCodeRangeMapping = extractor.GetAllTags(allLines, ref tagLines);
                if (tagToCodeRangeMapping.TryGetValue(obj.TagName, out var cr))
                {

                }
            }
            */

            //fix tabs before it calls GetCodeLines, or it's too late handle correct tabSize
            var tabSize = CodeLanguageUtil.GetTabSize(obj.CodePath);
            var tab = new string(' ', tabSize);
            content = content.Replace("\t", tab);
            /*
            var lines = StringUtil.ReadLines(content).ToList();
            lines = StringUtil.TrimEachLine(lines, tabSize: tabSize);
            content = string.Join(Environment.NewLine, lines);
            */
        }

        public static string GetContentPostFix(string content, CodeSnippet obj)
        {
            return content.Trim();
        }

        private static void BuildFileExtensionLanguageMap()
        {
            foreach (var (language, aliases) in s_languageAlias.Select(i => (i.Key, i.Value)))
            {
                Debug.Assert(!language.StartsWith('.'));

                s_languageByFileExtension.Add(language, language);
                s_languageByFileExtension.Add($".{language}", language);

                foreach (var alias in aliases)
                {
                    Debug.Assert(!alias.StartsWith('.'));

                    s_languageByFileExtension.Add(alias, language);
                    s_languageByFileExtension.Add($".{alias}", language);
                }
            }
        }

        private static void AddExtractorItems(string[] languages, CodeSnippetExtractor extractor)
        {
            s_defaultExtractors.Add(extractor);

            foreach (var language in languages)
            {
                AddExtractorItem(language, extractor);
                AddExtractorItem($".{language}", extractor);

                if (s_languageAlias.TryGetValue(language, out var aliases))
                {
                    foreach (var alias in aliases)
                    {
                        AddExtractorItem(alias, extractor);
                        AddExtractorItem($".{alias}", extractor);
                    }
                }
            }
        }

        private static void AddExtractorItem(string language, CodeSnippetExtractor extractor)
        {
            if (s_languageExtractors.TryGetValue(language, out var extractors))
            {
                extractors.Add(extractor);
            }
            else
            {
                s_languageExtractors[language] = [extractor];
            }
        }
    }
}
