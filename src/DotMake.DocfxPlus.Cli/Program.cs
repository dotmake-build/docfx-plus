using System;
using DotMake.DocfxPlus.Cli.Patches;
using HarmonyLib;

HarmonyBootstrap.Init();

var mainMethod = AccessTools.Method(Assemblies.DocfxAssembly.GetType("Docfx.Program"), "Main");

#if DEBUG
var commandLine = @"..\..\..\..\..\..\command-line\docs\docfx.json --serve --debug";
args = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
#endif

var result = mainMethod.Invoke(null, [args]);

return result != null ? (int)result : 0;

