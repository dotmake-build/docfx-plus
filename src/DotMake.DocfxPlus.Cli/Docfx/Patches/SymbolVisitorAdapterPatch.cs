using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.CodeAnalysis;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx.Patches
{
    internal static class SymbolVisitorAdapterPatch
    {
        public static Type Type = PatchAssemblies.DocfxDotnet.GetType("Docfx.Dotnet.SymbolVisitorAdapter", true);

        [HarmonyPatch]
        internal static class VisitNamespace
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(VisitNamespace));

            internal static object Postfix(object result, INamespaceSymbol symbol)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(VisitNamespace)} Postfix is run!");
                SymbolVisitorAdapter.VisitNamespace(result, symbol);
                return result;
            }
        }
    }
}
