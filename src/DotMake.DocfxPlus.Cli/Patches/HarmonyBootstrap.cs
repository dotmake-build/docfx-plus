using System;
using DotMake.DocfxPlus.Cli.Util;
using HarmonyLib;
#pragma warning disable CA2255

namespace DotMake.DocfxPlus.Cli.Patches
{
    public static class HarmonyBootstrap
    {
        //[ModuleInitializer]
        public static void Init()
        {
            var harmony = new Harmony("build.dotmake.docfxpatch");
            harmony.PatchAll(); // Applies all [HarmonyPatch] classes in your assembly

            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: Patches for DocFx are applied."
                              + $" Version: {ExecutableInfo.AssemblyInfo.Version}, DocFx Version: {new AssemblyInfo(Assemblies.DocfxAssembly).Version}");
        }
    }
}
