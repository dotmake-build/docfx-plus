using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.CodeAnalysis;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Patches
{
    internal static class SymbolFilterPatch
    {
        public static Type Type = Assemblies.DocfxDotnetAssembly.GetType("Docfx.Dotnet.SymbolFilter", true);

        [HarmonyPatch]
        internal static class IncludeApi
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(IncludeApi));

            internal static bool Postfix(bool result, ISymbol symbol, object __instance)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(IncludeApi)} Postfix is run!");
                return SymbolFilter.IncludeApi(result, symbol);
            }
        }
    }
}
