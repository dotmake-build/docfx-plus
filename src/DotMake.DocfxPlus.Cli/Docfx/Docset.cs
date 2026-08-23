using System.Collections.Generic;
using System.IO;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal static class Docset
    {
        internal static Dictionary<string, string> CurrentConfig = new();

        internal static void GetConfig(object __result)
        {
            //object config, string configDirectory
            var type = __result.GetType();
            var config = type.GetField("Item1")!.GetValue(__result)!;
            var configDirectory = type.GetField("Item2")!.GetValue(__result) as string;

            var configType = config.GetType();

            var getMetadataConfig = AccessTools.PropertyGetter(configType, "metadata");
            var metadataConfig = getMetadataConfig.Invoke(config, null) as IEnumerable<object>;
            if (metadataConfig == null)
                return;

            foreach (var metadataItemConfig in metadataConfig)
            {
                var metadataItemConfigType = metadataItemConfig.GetType();

                var getCodeSourceBasePath = AccessTools.PropertyGetter(metadataItemConfigType, "CodeSourceBasePath");
                var setCodeSourceBasePath = AccessTools.PropertySetter(metadataItemConfigType, "CodeSourceBasePath");

                //Fix codeSourceBasePath should be relative to configDirectory
                var codeSourceBasePath = getCodeSourceBasePath.Invoke(metadataItemConfig, null) as string;
                if (string.IsNullOrWhiteSpace(codeSourceBasePath))
                    codeSourceBasePath = configDirectory;
                else if (!Path.IsPathFullyQualified(codeSourceBasePath))
                {
                    codeSourceBasePath = Path.GetFullPath(codeSourceBasePath, configDirectory);
                }

                setCodeSourceBasePath.Invoke(metadataItemConfig, [codeSourceBasePath]);
               
                CurrentConfig.TryAdd("CodeSourceBasePath", codeSourceBasePath);
            }
        }

        /*
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
        */
    }
}
