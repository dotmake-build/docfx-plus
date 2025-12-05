using System;
using System.Reflection;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx.Patches
{
    internal static class CodeSnippetExtractorPatch
    {
        public static Type Type = PatchAssemblies.DocfxMarkdigEngineExtensions.GetType("Docfx.MarkdigEngine.Extensions.CodeSnippetExtractor", true);

        [HarmonyPatch]
        internal static class MatchTag
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(MatchTag));

            internal static bool Prefix(ref bool __result, string line, string template, out string tagName, bool containTagName = true)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ReadFile)} Prefix is run!");
                __result = CodeSnippetExtractor2.MatchTag(line, template, out tagName, containTagName);
                return false; //don't run original method
            }
        }
    }
}
