using System;
using System.Collections.Generic;

namespace DotMake.DocfxPlus.Cli.Shfb.MamlToMarkdown
{
    internal class MamlToMarkdownConverterContext
    {
        public Dictionary<string, string> XlinkHrefMap { get; set; }

        public Dictionary<string, string> XlinkTitleMap { get; set; }

        public Dictionary<string, string> ImageXlinkHrefMap { get; set; }

        public Dictionary<string, string> ImageXlinkAltMap { get; set; }

        public Func<string, string> NormalizeCodePath { get; set; }
    }
}
