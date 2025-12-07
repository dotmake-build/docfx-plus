using DotMake.CommandLine;
using DotMake.DocfxPlus.Cli;
using DotMake.DocfxPlus.Cli.Commands;
using DotMake.DocfxPlus.Cli.Docfx.Patches;
using DotMake.DocfxPlus.Cli.Logging;
using DotMake.DocfxPlus.Cli.Util;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

HarmonyBootstrap.Init();

Cli.Ext.ConfigureServices(services =>
{
    services.AddLogging(builder => 
        builder
            .AddConsoleFormatter<SimpleConsoleFormatterEx, SimpleConsoleFormatterOptions>()
            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
            })
    );

    services.AddSingleton<ILogger>(svc => svc.GetRequiredService<ILogger<Program>>());
});

var serviceProvider = Cli.Ext.GetServiceProviderOrDefault();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation($"{ExecutableInfo.AssemblyInfo.Product}: Patches for DocFx are applied."
                      + $" Version: {ExecutableInfo.AssemblyInfo.Version},"
                      + $" DocFx Version: {new AssemblyInfo(PatchAssemblies.Docfx).Version}");

var docfxMainMethod = AccessTools.Method(PatchAssemblies.Docfx.GetType("Docfx.Program"), "Main");

#if DEBUG
//var commandLine = @"..\..\..\..\..\\docs\docfx.json --serve --debug";

//args = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
#endif

var parser = Cli.GetParser<RootCliCommand>(new CliSettings { EnableDefaultExceptionHandler = true });
var result = parser.Parse(args);

if (result.Contains<ConvertCliCommand>())
    return parser.Run(args);

var docfxResult = docfxMainMethod.Invoke(null, [args]);

return docfxResult != null ? (int)docfxResult : 0;
