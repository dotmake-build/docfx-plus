using System.Reflection;

namespace DotMake.DocfxPlus.Plugin.Patches
{
    internal static class PatchAssemblies
    {
        public static Assembly DocfxBuild => docfxBuild ??= Assembly.Load("Docfx.Build");
        private static Assembly docfxBuild;
    }
}
