using System;
using System.Reflection;
using DotMake.DocfxPlus.Cli.Util;
using HarmonyLib;
#pragma warning disable CA2255

namespace DotMake.DocfxPlus.Cli
{
    public static class HarmonyBootstrap
    {
        public static Assembly DocfxAssembly = Assembly.Load("docfx");

        //[ModuleInitializer]
        public static void Init()
        {
            var harmony = new Harmony("build.dotmake.docfxpatch");
            harmony.PatchAll(); // Applies all [HarmonyPatch] classes in your assembly

            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: Patches for DocFx are applied."
                              + $" Version: {ExecutableInfo.AssemblyInfo.Version}, DocFx Version: {new AssemblyInfo(DocfxAssembly).Version}");
        }
    }
}
