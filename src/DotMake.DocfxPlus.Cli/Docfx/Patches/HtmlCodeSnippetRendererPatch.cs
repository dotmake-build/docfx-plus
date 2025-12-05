using System;
using System.Reflection;
using Docfx.MarkdigEngine.Extensions;
using HarmonyLib;
using Markdig.Renderers;

// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx.Patches
{
    internal static class HtmlCodeSnippetRendererPatch
    {
        public static Type Type = PatchAssemblies.DocfxMarkdigEngineExtensions.GetType("Docfx.MarkdigEngine.Extensions.HtmlCodeSnippetRenderer", true);

        [HarmonyPatch]
        internal static class Write
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(Write), [typeof(HtmlRenderer), typeof(CodeSnippet)]);

            internal static bool Prefix(HtmlRenderer renderer, CodeSnippet codeSnippet, object __instance)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(GetContent)} Prefix is run!");
                HtmlCodeSnippetRenderer.Write(renderer, codeSnippet);
                return true; //also run original method
            }
        }

        [HarmonyPatch]
        internal static class GetContent
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(GetContent));

            internal static bool Prefix(ref string content, CodeSnippet obj, object __instance)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(GetContent)} Prefix is run!");
                HtmlCodeSnippetRenderer.GetContent(ref content, obj, __instance);
                return true; //also run original method
            }

            internal static void Postfix(ref string __result, CodeSnippet obj)
            {
                __result = HtmlCodeSnippetRenderer.GetContentPostFix(__result, obj);
            }
        }
    }
}
