![DotMake Docfx-Plus Logo](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/logo-wide.svg "DotMake Docfx-Plus Logo")

# DotMake Docfx-Plus

A template and a tool for enhancing [DocFx](https://github.com/dotnet/docfx).

This project includes two parts:

- The `docfx-plus` **template** which extends DocFx's `modern` template to fix many UI problems and behaviors. 
  It looks and feels more similar to Microsoft's Learn site.

- The `docfx-plus` **tool** which is a wrapper around `docfx` tool, which at runtime patches the internals to fix some problems;
  currently mainly for advanced support of XML Comments (xmldocs) `<code>` blocks.
  This wrapper is developed because these changes cannot be applied in the template (or in a plugin as it's too late for metadata (`.yml`) changes).
  The tool can also convert/migrate your existing `SHFB` projects completely to `docfx` projects.

This project was mainly done for migrating our projects' docs from [SHFB (Sandcastle Help File Builder)](https://github.com/EWSoftware/SHFB) 
which is still very stable but its theme and architecture was outdated.
`SHFB` was used for many years mainly because of its excellent `<code>` block support and now we put these features into `docfx`.

[**Live Demo**](https://dotmake.build/command-line/api/) - API docs for our other project [DotMake Command-Line](https://github.com/dotmake-build/command-line).

[![Nuget](https://img.shields.io/nuget/v/docfx-plus?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/docfx-plus)

![docfx-plus-template-light](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-light.png)

![docfx-plus-template-light](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-dark.png)
