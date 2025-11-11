using System;
using System.Reflection;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Patches
{
    internal static class DotnetApiCatalogPatch
    {
        public static Type Type = Assemblies.DocfxDotnetAssembly.GetType("Docfx.Dotnet.DotnetApiCatalog", true);

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
}
