using System.IO;
using HarmonyLib;
using Markdig.Syntax;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal static class MarkdigMarkdownService
    {
        public static void ReadFile(ref string path, MarkdownObject origin, object __instance)
        {
            var markdigMarkdownServiceType= __instance.GetType();
            var _parameters= AccessTools.Field(markdigMarkdownServiceType, "_parameters")
                .GetValue(__instance);
            var parametersType = _parameters!.GetType();
            var getBasePath = AccessTools.PropertyGetter(parametersType, "BasePath");
            var basePath = getBasePath.Invoke(_parameters, null) as string;

            if (DotnetApiCatalog.CurrentConfig.TryGetValue("CodeSourceBasePath", out var codeSourceBasePath)
                && !string.IsNullOrEmpty(codeSourceBasePath))
                codeSourceBasePath = Path.GetRelativePath(basePath!, codeSourceBasePath);
            else
                codeSourceBasePath = basePath!;

            if (path.StartsWith("~~/"))
                path = Path.Combine("~", codeSourceBasePath, path.Substring(3));
        }
    }
}
