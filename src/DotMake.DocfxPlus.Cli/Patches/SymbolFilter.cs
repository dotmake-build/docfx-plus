using Microsoft.CodeAnalysis;

namespace DotMake.DocfxPlus.Cli.Patches
{
    internal static class SymbolFilter
    {
        internal static bool IncludeApi(bool result, ISymbol symbol)
        {
            //Classes that are Internal or marked with [System.Runtime.CompilerServices.CompilerGenerated] attribute,
            //will probably be excluded by default, but we want NamespaceDoc classes every time so that we can extract
            //xml comments from them and assign those xml comments to the containing namespace.
            //Later we will remove NamespaceDoc classes in SymbolVisitorAdapter.VisitNamespace
            //so themselves will not be included in the docs.
            if (!result)
                return (symbol is ITypeSymbol typeSymbol
                        && typeSymbol.TypeKind == TypeKind.Class
                        && typeSymbol.Name == "NamespaceDoc");

            return true;
        }
    }
}
