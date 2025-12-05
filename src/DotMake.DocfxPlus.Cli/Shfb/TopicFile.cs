using System;
using System.IO;
using System.Xml;
using DotMake.DocfxPlus.Cli.Util;

namespace DotMake.DocfxPlus.Cli.Shfb
{
    internal class TopicFile
    {
        public TopicFile(string topicFile, string basePath)
        {
            FilePath = topicFile;
            FileFullPath = (basePath == null)
                ? topicFile
                : Path.Combine(basePath, topicFile);

            try
            {
                if (!File.Exists(FileFullPath))
                    throw new FileNotFoundException("File does not exist!");

                var settings = new XmlReaderSettings
                {
                    CloseInput = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true
                };

                using var xr = XmlReader.Create(FileFullPath, settings);

                xr.MoveToContent();

                while (!xr.EOF)
                {
                    if (xr.NodeType != XmlNodeType.Element)
                        xr.Read();
                    else
                    {
                        switch (xr.Name)
                        {
                            case "topic":
                                // If a <topic> element is found, parse the ID
                                // and revision number from it.
                                var attrValue = xr.GetAttribute("id");

                                // The ID is required
                                if (!string.IsNullOrWhiteSpace(attrValue))
                                    Id = attrValue;
                                else
                                    throw new XmlException("<topic> element " +
                                        "is missing the 'id' attribute");

                                // This is optional
                                attrValue = xr.GetAttribute("revisionNumber");

                                if (attrValue != null && int.TryParse(attrValue, out var rev))
                                    RevisionNumber = rev;

                                xr.Read();
                                break;

                            case "developerConceptualDocument":
                            case "developerErrorMessageDocument":
                            case "developerGlossaryDocument":
                            case "developerHowToDocument":
                            case "developerOrientationDocument":
                            case "codeEntityDocument":
                            case "developerReferenceWithSyntaxDocument":
                            case "developerReferenceWithoutSyntaxDocument":
                            case "developerSampleDocument":
                            case "developerSDKTechnologyOverviewArchitectureDocument":
                            case "developerSDKTechnologyOverviewCodeDirectoryDocument":
                            case "developerSDKTechnologyOverviewOrientationDocument":
                            case "developerSDKTechnologyOverviewScenariosDocument":
                            case "developerSDKTechnologyOverviewTechnologySummaryDocument":
                            case "developerTroubleshootingDocument":
                            case "developerUIReferenceDocument":
                            case "developerWalkthroughDocument":
                            case "developerWhitePaperDocument":
                            case "developerXmlReference":
                                DocumentType = xr.Name;
                                xr.Read();
                                break;

                            default:    // Ignore it
                                xr.Skip();
                                break;
                        }
                    }
                }

                //If Id is not an ugly Guid, use it for file name
                FilePathForUrl = Guid.TryParse(Id, out _)
                    ? Path.GetFileName(FilePath)
                    : Id;

                if (FilePathForUrl != null)
                {
                    var parent = Path.GetDirectoryName(FilePath);
                    if (parent != null)
                        FilePathForUrl = Path.Combine(parent, FilePathForUrl);

                    FilePathForUrl = PathUtil.PathToSlug(FilePathForUrl);
                    var extension = Path.GetExtension(FilePathForUrl);
                    if (extension?.Length > 0)
                        FilePathForUrl = FilePathForUrl.Substring(0, FilePathForUrl.Length - extension.Length);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{FilePath} -> {ex.Message}";
            }
        }

        public string FilePath { get; }

        public string FileFullPath { get; }

        public string Id { get; }

        public int RevisionNumber { get; }

        public string DocumentType { get; }

        public string ErrorMessage { get; }

        public string FilePathForUrl { get; }

        public Topic Topic { get; set; }

        public string DisplayTitle
        {
            get
            {
                if (Topic != null)
                {
                    if (!string.IsNullOrWhiteSpace(Topic.Title))
                        return Topic.Title;

                    if (!string.IsNullOrWhiteSpace(Topic.TocTitle))
                        return Topic.TocTitle;
                }

                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }
    }
}
