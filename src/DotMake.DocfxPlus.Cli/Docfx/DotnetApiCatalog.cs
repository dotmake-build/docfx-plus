using System.Collections.Generic;
using System.IO;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal static class DotnetApiCatalog
    {
        internal static Dictionary<string, string> CurrentConfig = new();

        internal static void ConvertConfig(object configModel, string configDirectory, string outputDirectory)
        {
            var configModelType = configModel.GetType();
            var getCodeSourceBasePath = AccessTools.PropertyGetter(configModelType, "CodeSourceBasePath");
            var setCodeSourceBasePath = AccessTools.PropertySetter(configModelType, "CodeSourceBasePath");

            //Fix codeSourceBasePath should be relative to configDirectory
            var codeSourceBasePath = getCodeSourceBasePath.Invoke(configModel, null) as string;
            if (string.IsNullOrWhiteSpace(codeSourceBasePath))
                codeSourceBasePath = configDirectory;
            else if (!Path.IsPathFullyQualified(codeSourceBasePath))
            {
                codeSourceBasePath = Path.GetFullPath(codeSourceBasePath, configDirectory);
            }

            setCodeSourceBasePath.Invoke(configModel, [codeSourceBasePath]);

            CurrentConfig.Add("CodeSourceBasePath", codeSourceBasePath);
        }

        /*
        internal static void GetConfig(object __result)
        {
            //object config, string configDirectory
            var type = __result.GetType();
            var config = type.GetField("Item1")!.GetValue(__result)!;
            var configDirectory = type.GetField("Item2")!.GetValue(__result) as string;

            var configType = config.GetType();
            //todo: need to get config.config here
            var getCodeSourceBasePath = AccessTools.PropertyGetter(configType, "CodeSourceBasePath");
            var setCodeSourceBasePath = AccessTools.PropertySetter(configType, "CodeSourceBasePath");

            //Fix codeSourceBasePath should be relative to configDirectory
            var codeSourceBasePath = getCodeSourceBasePath.Invoke(config, null) as string;
            if (string.IsNullOrWhiteSpace(codeSourceBasePath))
                codeSourceBasePath = configDirectory;
            else if (!Path.IsPathFullyQualified(codeSourceBasePath))
            {
                codeSourceBasePath = Path.GetFullPath(codeSourceBasePath, configDirectory);
            }

            setCodeSourceBasePath.Invoke(config, [codeSourceBasePath]);

            CurrentConfig = new Dictionary<string, string>
            {
                { "CodeSourceBasePath", codeSourceBasePath }
            };
        }
        */
    }
}
