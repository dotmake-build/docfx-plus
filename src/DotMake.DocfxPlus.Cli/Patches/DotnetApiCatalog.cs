using System.IO;
using HarmonyLib;

namespace DotMake.DocfxPlus.Cli.Patches
{
    internal static class DotnetApiCatalog
    {
        internal static void ConvertConfig(object configModel, string configDirectory, string outputDirectory)
        {
            var configModelType = configModel.GetType();
            var getCodeSourceBasePath = AccessTools.PropertyGetter(configModelType, "CodeSourceBasePath");
            var setCodeSourceBasePath = AccessTools.PropertySetter(configModelType, "CodeSourceBasePath");

            var codeSourceBasePath = getCodeSourceBasePath.Invoke(configModel, null) as string;
            if (codeSourceBasePath != null && !Path.IsPathFullyQualified(codeSourceBasePath))
            {
                codeSourceBasePath = Path.Combine(configDirectory, codeSourceBasePath);
                setCodeSourceBasePath.Invoke(configModel, [codeSourceBasePath]);
            }
        }
    }
}
