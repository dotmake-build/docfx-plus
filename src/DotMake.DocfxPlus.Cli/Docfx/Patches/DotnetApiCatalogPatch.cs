using System;
using System.Reflection;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx.Patches
{
    internal static class DotnetApiCatalogPatch
    {
        public static Type Type = PatchAssemblies.DocfxDotnet.GetType("Docfx.Dotnet.DotnetApiCatalog", true);

        [HarmonyPatch]
        internal static class ConvertConfig
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(ConvertConfig));

            internal static bool Prefix(object configModel, string configDirectory, string outputDirectory)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ConvertConfig)} Prefix is run!");
                DotnetApiCatalog.ConvertConfig(configModel, configDirectory, outputDirectory);
                return true; //also run original method
            }
        }
    }

    /*
    internal static class DocsetPatch
    {
        public static Type Type = PatchAssemblies.DocfxApp.GetType("Docfx.Docset", true);

        [HarmonyPatch]
        internal static class GetConfig
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(GetConfig));

            internal static void Postfix(ref object __result, string configFile)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ConvertConfig)} Prefix is run!");
                DotnetApiCatalog.GetConfig(__result);
            }
        }
    }
    */
}
