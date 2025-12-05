
namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal class DocfxProjectOptions
    {
        public string ProjectFileName { get; init; } = "docfx.json";

        public string DocsLocation { get; init; } = "docs";

        public string ImagesLocation { get; init; } = "images";

        public string ApiLocation { get; init; } = "api";

        public string OverwritesLocation { get; init; } = "overwrites";

        public bool RebaseContent { get; init; } = true;

        public bool RebaseImages { get; init; } = true;
   }
}
