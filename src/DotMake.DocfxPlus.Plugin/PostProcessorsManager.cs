using System;
using System.Collections.Immutable;
using System.Composition.Hosting;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Plugin
{
    internal static class PostProcessorsManager
    {
        public static void GetPostProcessor(CompositionHost container, ref ImmutableArray<string> postProcessorNames)
        {
            //Console.WriteLine("PostProcessorsManager is instantiated!");

            if (!postProcessorNames.Contains(nameof(DocfxPlus)))
                postProcessorNames = postProcessorNames.Add(nameof(DocfxPlus));
        }
    }
}
