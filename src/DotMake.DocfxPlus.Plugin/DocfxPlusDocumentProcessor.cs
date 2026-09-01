using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Docfx.Plugins;

namespace DotMake.DocfxPlus.Plugin
{
    //To ensure we have a reference to Docfx.Plugins (otherwise plugin will not be loaded)
    //even if we don't have a IPostProcessor implementation

    [Export(typeof(IDocumentProcessor))]
    public class DocfxPlusDocumentProcessor : IDocumentProcessor
    {
        public DocfxPlusDocumentProcessor()
        {
            /*
                IDocumentProcessor classes are instantiated in DocumentBuilder constructor via
                    _container.SatisfyImports(this);
                just before
                    _postProcessorsManager = new PostProcessorsManager(_container, postProcessorNames);

                So we hook here just to add our IPostProcessor dynamically so that we don't need to specify it in config:
                
                    "build": {
                      ...
                      "postProcessors": ["OutputPDF", "BeautifyHTML", "OutputPDF"]
                    }
               
                https://github.com/dotnet/docfx/blob/main/src/Docfx.Build/DocumentBuilder.cs#L15
            */

            //Console.WriteLine("DocfxPlus Plugin is loaded!");

            //Use module initializer instead otherwise it does not work in Release build!
            //HarmonyBootstrap.Init();
        }

        public ProcessingPriority GetProcessingPriority(FileAndType file)
        {
            return ProcessingPriority.NotSupported;
        }

        public FileModel Load(FileAndType file, ImmutableDictionary<string, object> metadata)
        {
            throw new NotImplementedException();
        }

        public SaveResult Save(FileModel model)
        {
            throw new NotImplementedException();
        }

        public void UpdateHref(FileModel model, IDocumentBuildContext context)
        {
            throw new NotImplementedException();
        }

        public string Name => nameof(DocfxPlus);

        public IEnumerable<IDocumentBuildStep> BuildSteps { get; } = [];
    }


    /*
    [Export(typeof(IInputMetadataValidator))]
    internal class InputMetadataValidator : IInputMetadataValidator
    {
        public InputMetadataValidator()
        {
            Console.WriteLine("Plugin is loaded!");
        }

        public void Validate(string sourceFile, ImmutableDictionary<string, object> metadata)
        {
        }
    }



    //[Export(nameof(DocfxPlus), typeof(IDocumentBuildStep))]
    public class DocfxPlusBuildStep //: IDocumentBuildStep
    {
        public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(IDocumentBuildStep)}.{nameof(Prebuild)} is run!");

            return models;
        }

        public void Build(FileModel model, IHostService host)
        {
            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(IDocumentBuildStep)}.{nameof(Build)} is run!");
        }

        public void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(IDocumentBuildStep)}.{nameof(Postbuild)} is run!");
        }

        public string Name { get; }

        public int BuildOrder { get; }
    }
    */
}

