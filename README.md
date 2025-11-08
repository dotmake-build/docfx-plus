![DotMake Docfx-Plus Logo](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/logo-wide.svg "DotMake Docfx-Plus Logo")

# DotMake Docfx-Plus

A template and a tool for enhancing [DocFx](https://github.com/dotnet/docfx).

The `docfx-plus` **template** extends DocFx's `modern` template to fix many UI problems and behaviors. It looks and feels more
similar to Microsoft's Learn site.

The `docfx-plus` **tool** is a wrapper around `docfx` tool, which at runtime patches the internals to fix some problems;
mainly advanced support of xmldocs `<code>` blocks; to support more languages and regions, consistent line break handling etc.
This wrapper is developed because these changes cannot be applied in the template.

This project was mainly done for migrating our projects' docs from [SHFB](https://github.com/EWSoftware/SHFB) 
which is still very stable but its theme and architecture was outdated.
It was used for many years mainly because of its excellent `<code>` block support and now we put these features into `docfx`.

[![Nuget](https://img.shields.io/nuget/v/docfx-plus?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/docfx-plus)

### (Docs are in progress and more details will be provided here soon)

## Getting started

Install the dotnet tool from [NuGet](https://www.nuget.org/).

```console
dotnet tool install --global docfx-plus
```

Then edit your `docfx.json` and update the template property so that you are able to use the theme:
```json
"template": [
    "default",
    "modern",
    "docfx-plus"
]
```
And ensure `outputFormat` is not set to a value other than `mref` (the default value which means ManagedReference). 
For example using `apiPage` will not make use of our theme because for that mode, 
`docfx` internally generates the HTML, most of which is not customizable in the template.

The template can also be used alone with regular `docfx` tool, however it's recommended to use `docfx-plus` tool
which already bundles the template and in addition provides important fixes for `<code>` blocks.

If you want to use the theme with the regular `docfx` tool, you can export it via:
```console
docfx-plus template export docfx-plus
```
This will export the bundled `docfx-plus` template to `_exported_templates` subfolder, which then can be consumed as:
```json
"template": [
    "default",
    "modern",
    "_exported_templates/docfx-plus"
]
```

### Prerequisites

- .NET SDK 8.0 and later. The .NET CLI (`dotnet` command) is included with the [.NET SDK](https://learn.microsoft.com/en-us/dotnet/core/sdk).

## Usage

### Dotnet tool usage

Just use `docfx-plus` command instead of `docfx` command with same arguments and options.

```console
docfx-plus init --yes

docfx-plus --serve

docfx-plus metadata

docfx-plus build
```

## Building

We provide some `.cmd` batch scripts in `build` folder for easier building:
```console
1. Build Cli.cmd
2. Build Nuget Packages.cmd
3. Build Api Docs WebSite.cmd         
```

Output results can be found in `publish` folder, for example:
```console
DotMake.DocfxPlus.Cli-net8.0

docfx-plus.1.0.0.nupkg
```

