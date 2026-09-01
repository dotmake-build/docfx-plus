using System;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Reflection;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Plugin.Patches
{
    internal static class PostProcessorsManagerPatch
    {
        public static Type Type = PatchAssemblies.DocfxBuild.GetType("Docfx.Build.Engine.PostProcessorsManager", true);

        [HarmonyPatch]
        internal static class GetPostProcessor
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(GetPostProcessor));

            internal static bool Prefix(CompositionHost container, ref ImmutableArray<string> processors)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(Ctor)} Prefix is run!");
                PostProcessorsManager.GetPostProcessor(container, ref processors);
                return true; //also run original method
            }
        }
    }
}
