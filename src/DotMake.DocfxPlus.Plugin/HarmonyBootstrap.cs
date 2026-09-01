using System.Runtime.CompilerServices;
using HarmonyLib;
#pragma warning disable CA2255

namespace DotMake.DocfxPlus.Plugin
{
    public static class HarmonyBootstrap
    {
        [ModuleInitializer]
        public static void Init()
        {
            var harmony = new Harmony("build.dotmake.docfxpluginpatch");
            harmony.PatchAll(); // Applies all [HarmonyPatch] classes in your assembly
        }
    }
}
