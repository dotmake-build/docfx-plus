using System;
using System.Reflection;
using Docfx.MarkdigEngine.Extensions;
using HarmonyLib;
using Markdig.Renderers;

// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx.Patches
{
    internal static class QuoteSectionNoteRenderPatch
    {
        public static Type Type = PatchAssemblies.DocfxMarkdigEngineExtensions.GetType("Docfx.MarkdigEngine.Extensions.QuoteSectionNoteRender", true);

        [HarmonyPatch]
        internal static class WriteNote
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(WriteNote));

            internal static bool Prefix(object __instance, HtmlRenderer renderer, QuoteSectionNoteBlock obj)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ReadFile)} Prefix is run!");
                QuoteSectionNoteRender.WriteNote(__instance, renderer, obj);
                return false; //don't run original method
            }
        }
    }
}
