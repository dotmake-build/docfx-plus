using System;
using System.IO;
using System.Linq;
using DotMake.CommandLine;
using DotMake.DocfxPlus.Cli.Docfx;
using DotMake.DocfxPlus.Cli.Shfb;
using Microsoft.Extensions.Logging;

namespace DotMake.DocfxPlus.Cli.Commands
{
    [CliCommand(
        Description = """"
                      Convert an existing SHFB (Sandcastle Help File Builder) project to a DocFx project.
                      - Project file (`.shfbproj`) will be converted to `docfx.json`
                      - Content Layout files (`.content`) will be converted to `toc.yml`
                      - MAML Topic files (`.aml`) will be converted to Markdown files (`.md`)
                      - Namespace summaries will be converted to overwrite files (`.md`)
                      - Other content files like images will be copied
                      - By default `content` subfolder will be rebased to `docs`  
                        and `icons`, `media` subfolders will be rebased to `images`
                        to match `docfx` conventions.
                      """"
    )]
    internal class ConvertCliCommand : ICliRunWithContextAndReturn
    {
        private readonly ILogger logger;

        public ConvertCliCommand(ILogger logger)
        {
            this.logger = logger;
        }

        [CliArgument(
            Description =
                "The path to the SHFB project file (`.shfbproj`). By default, the first found `.shfbproj` file in current directory is used",
            ValidationRules = CliValidationRules.LegalPath | CliValidationRules.ExistingFileOrDirectory
        )]
        public string ShfbProjectFile { get; set; } = "";

        [CliOption(
            Description = "The output base directory to write converted DocFx project files.",
            ValidationRules = CliValidationRules.LegalPath
        )]
        public string Output { get; set; }

        [CliOption(Description = "The subfolder under DocFx project, to use for markdown (`.md`) files.")]
        public string DocsLocation { get; set; } = "docs";

        [CliOption(Description = "The subfolder under DocFx project, to use for image files.")]
        public string ImagesLocation { get; set; } = "images";

        [CliOption(Description = "The subfolder under DocFx project, to use for generated API metadata (`.yml`) files.")]
        public string ApiLocation { get; set; } = "api";

        [CliOption(Description = "The subfolder under DocFx project, to use for overwrite (`.md` or `.yml`) files.")]
        public string OverwritesLocation { get; set; } = "overwrites";

        [CliOption(Description = "Whether to rebase `content` subfolder from SHFB to `docs` location when converting.")]
        public bool RebaseContent { get; set; } = true;

        [CliOption(Description = "Whether to rebase `icons` and `media` subfolders from SHFB to `images` location when converting.")]
        public bool RebaseImages { get; set; } = true;

        public int Run(CliContext cliContext)
        {
            var (inputParentPath, inputFileName) = GetParentPath(ShfbProjectFile);
            var (outputParentPath, outputFileName) = GetParentPath(Output);

            if (string.IsNullOrEmpty(inputFileName))
            {
                inputFileName = Directory.EnumerateFiles(inputParentPath, "*.shfbproj")
                    .Select(Path.GetFileName)
                    .FirstOrDefault();

                if (inputFileName == null)
                {
                    if (inputParentPath.Length == 0)
                        throw new FileNotFoundException("Cannot find a .shfbproj file in current directory !");

                    throw new FileNotFoundException($"Cannot find a .shfbproj file in directory `{inputParentPath}` !");
                }
            }

            if (ArePathsEqual(inputParentPath, outputParentPath))
                throw new FileNotFoundException("Output folder cannot be the same as the input SHFB project directory !");

            var shfbProject = new ShfbProject(Path.Combine(inputParentPath, inputFileName), logger);

            var docfxProjectOptions = new DocfxProjectOptions
            {
                ProjectFileName = (outputFileName.Length > 0) ? outputFileName : "docfx.json",
                DocsLocation = DocsLocation,
                ImagesLocation = ImagesLocation,
                ApiLocation = ApiLocation,
                OverwritesLocation = OverwritesLocation,
                RebaseContent = RebaseContent,
                RebaseImages = RebaseImages
            };
            var docfxProject = new DocfxProject(outputParentPath, docfxProjectOptions, logger);

            docfxProject.ConvertFrom(shfbProject);

            return 0;
        }

        private (string parentPath, string fileName) GetParentPath(string path)
        {
            path = Path.GetFullPath(path, Environment.CurrentDirectory);
            
            if (!string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                return (Path.GetDirectoryName(path), Path.GetFileName(path));
            }

            return (path, string.Empty);
        }

        private bool ArePathsEqual(string path1, string path2)
        {
            var norm1 = NormalizePath(path1);
            var norm2 = NormalizePath(path2);

            return string.Equals(norm1, norm2,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal);
        }

        private string NormalizePath(string path)
        {
            var full = Path.GetFullPath(path);
            return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
