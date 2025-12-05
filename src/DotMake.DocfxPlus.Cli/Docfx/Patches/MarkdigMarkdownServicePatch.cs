using System;
using System.Reflection;
using HarmonyLib;
using Markdig.Syntax;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx.Patches
{
    internal static class MarkdigMarkdownServicePatch
    {
        public static Type Type = PatchAssemblies.DocfxMarkdigEngine.GetType("Docfx.MarkdigEngine.MarkdigMarkdownService", true);

        [HarmonyPatch]
        internal static class ReadFile
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(ReadFile));

            internal static bool Prefix(ref string path, MarkdownObject origin, object __instance)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ReadFile)} Prefix is run!");
                MarkdigMarkdownService.ReadFile(ref path, origin, __instance);
                return true; //also run original method
            }
        }
    }
}
