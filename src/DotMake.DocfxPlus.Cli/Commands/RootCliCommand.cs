using DotMake.CommandLine;

namespace DotMake.DocfxPlus.Cli.Commands
{
    [CliCommand(
        Children =
        [
            typeof(ConvertCliCommand)
        ]
    )]
    internal class RootCliCommand
    {
    }
}
