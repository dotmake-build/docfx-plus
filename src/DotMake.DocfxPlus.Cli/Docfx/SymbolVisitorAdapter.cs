using System.Collections.Generic;
using System.Linq;
using Docfx.DataContracts.ManagedReference;
using HarmonyLib;
using Microsoft.CodeAnalysis;

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal static class SymbolVisitorAdapter
    {
        internal static void VisitNamespace(object result, INamespaceSymbol symbol)
        {
            //Find NamespaceDoc classes in namespaces, extract xml comments from them
            //and assign those xml comments to the containing namespaces.
            //Later we remove NamespaceDoc classes so themselves will not be included in the docs.
            //We ensure NamespaceDoc classes are always included in SymbolFilter.IncludeApi
            //so that we have the xml comments loaded for them.

            var metadataItemType = result.GetType();
            var getItems = AccessTools.PropertyGetter(metadataItemType, "Items");
            var getType = AccessTools.PropertyGetter(metadataItemType, "Type");
            var getName = AccessTools.PropertyGetter(metadataItemType, "Name");
            var getSummary = AccessTools.PropertyGetter(metadataItemType, "Summary");
            var setSummary = AccessTools.PropertySetter(metadataItemType, "Summary");
            var getRemarks = AccessTools.PropertyGetter(metadataItemType, "Remarks");
            var setRemarks = AccessTools.PropertySetter(metadataItemType, "Remarks");

            var items = getItems.Invoke(result, null);
            if (items == null)
                return;

            var namespaceDocItem = (items as IEnumerable<object>)?.FirstOrDefault(item =>
            {
                var type = getType.Invoke(item, null) as MemberType?;
                var name = getName.Invoke(item, null) as string;

                return type == MemberType.Class && name != null && name.EndsWith(".NamespaceDoc");
            });

            if (namespaceDocItem is null)
                return;

            setSummary.Invoke(result, [getSummary.Invoke(namespaceDocItem, null)]);
            setRemarks.Invoke(result, [getRemarks.Invoke(namespaceDocItem, null)]);

            var remove = AccessTools.Method(items.GetType(), "Remove");
            remove.Invoke(items, [namespaceDocItem]);
        }
    }
}
