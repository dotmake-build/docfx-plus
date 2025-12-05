using HarmonyLib;
#pragma warning disable CA2255

namespace DotMake.DocfxPlus.Cli
{
    public static class HarmonyBootstrap
    {
        //[ModuleInitializer]
        public static void Init()
        {
            var harmony = new Harmony("build.dotmake.docfxpatch");
            harmony.PatchAll(); // Applies all [HarmonyPatch] classes in your assembly
        }
    }
}
