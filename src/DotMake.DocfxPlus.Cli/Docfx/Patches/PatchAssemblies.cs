using System.Reflection;

namespace DotMake.DocfxPlus.Cli.Docfx.Patches
{
    internal static class PatchAssemblies
    {
        public static Assembly Docfx => docfx ??= Assembly.Load("docfx");
        private static Assembly docfx;

        public static Assembly DocfxApp => docfxApp ??= Assembly.Load("Docfx.App");
        private static Assembly docfxApp;

        public static Assembly DocfxDotnet => docfxDotnet ??= Assembly.Load("Docfx.Dotnet");
        private static Assembly docfxDotnet;

        public static Assembly DocfxMarkdigEngine => docfxMarkdigEngine ??= Assembly.Load("Docfx.MarkdigEngine");
        private static Assembly docfxMarkdigEngine;

        public static Assembly DocfxMarkdigEngineExtensions => docfxMarkdigEngineExtensions ??= Assembly.Load("Docfx.MarkdigEngine.Extensions");
        private static Assembly docfxMarkdigEngineExtensions;
    }
}
