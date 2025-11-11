using System.Reflection;

namespace DotMake.DocfxPlus.Cli.Patches
{
    internal static class Assemblies
    {
        public static Assembly DocfxAssembly => docfxAssembly ??= Assembly.Load("docfx");
        private static Assembly docfxAssembly;

        public static Assembly DocfxDotnetAssembly => docfxDotnetAssembly ??= Assembly.Load("Docfx.Dotnet");
        private static Assembly docfxDotnetAssembly;
    }
}
