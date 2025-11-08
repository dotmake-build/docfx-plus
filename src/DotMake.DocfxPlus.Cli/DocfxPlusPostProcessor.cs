/*using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Docfx.Plugins;
using DotMake.DocfxPlus.Cli.Util;

namespace DotMake.DocfxPlus.Cli
{
    //To ensure we have a reference to Docfx.Plugins (otherwise plugin will not be loaded)
    //even if we don't have a IPostProcessor implementation

    [Export(nameof(DocfxPlus), typeof(IPostProcessor))]
    internal class DocfxPlusPostProcessor : IPostProcessor
    {
        public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
        {
            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(IPostProcessor)}.{nameof(PrepareMetadata)} is run!");

            return metadata;
        }

        public Manifest Process(Manifest manifest, string outputFolder, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(IPostProcessor)}.{nameof(Process)} is run!");

            return manifest;
        }
    }

    [Export(typeof(IDocumentProcessor))]
    public class DocfxPlusDocumentProcessor : IDocumentProcessor
    {
        public ProcessingPriority GetProcessingPriority(FileAndType file)
        {
            Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(IDocumentProcessor)}.{nameof(GetProcessingPriority)} is run!");

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

        public string Name { get; } = nameof(DocfxPlusDocumentProcessor);
        public IEnumerable<IDocumentBuildStep> BuildSteps { get; } = new IDocumentBuildStep[0];
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
}
*/
