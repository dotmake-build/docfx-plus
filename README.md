![DotMake Docfx-Plus Logo](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/logo-wide.svg "DotMake Docfx-Plus Logo")

# DotMake Docfx-Plus

A template and a tool for enhancing [DocFx](https://github.com/dotnet/docfx).

This project includes two parts:

- The `docfx-plus` **template** which extends DocFx's `modern` template to fix many UI problems and behaviors. 
  It looks and feels more similar to Microsoft's Learn site.

- The `docfx-plus` **tool** which is a wrapper around `docfx` tool, which at runtime patches the internals to fix some problems;
  currently mainly for advanced support of XML Comments (xmldocs) `<code>` blocks.
  This wrapper is developed because these changes cannot be applied in the template (or in a plugin as it's too late for metadata (`.yml`) changes).

This project was mainly done for migrating our projects' docs from [SHFB (Sandcastle Help File Builder)](https://github.com/EWSoftware/SHFB) 
which is still very stable but its theme and architecture was outdated.
`SHFB` was used for many years mainly because of its excellent `<code>` block support and now we put these features into `docfx`.

[**Live Demo**](https://dotmake.build/command-line/api/) - API docs for our other project [DotMake Command-Line](https://github.com/dotmake-build/command-line).

[![Nuget](https://img.shields.io/nuget/v/docfx-plus?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/docfx-plus)

![docfx-plus-template-light](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-light.png)

![docfx-plus-template-light](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-dark.png)

## Getting started

Install the dotnet tool from [NuGet](https://www.nuget.org/).

```console
dotnet tool install --global docfx-plus
```

### Prerequisites

- .NET SDK 8.0 and later. The .NET CLI (`dotnet` command) is included with the [.NET SDK](https://learn.microsoft.com/en-us/dotnet/core/sdk).

## Usage

### Dotnet tool usage

Just use `docfx-plus` command instead of `docfx` command with same subcommands, arguments and options:

```console
docfx-plus init --yes

docfx-plus --serve

docfx-plus metadata

docfx-plus build
```

Refer to [DocFx Commandline Reference](https://dotnet.github.io/docfx/reference/docfx-cli-reference/overview.html) for more details.

### Template usage

Edit your `docfx.json` and update the template property so that you are able to use the theme:
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
Use these `metadata` settings for best results:
```json
"metadata": [
  {
    "memberLayout": "separatePages",
    "categoryLayout": "nested",
    //"codeSourceBasePath": "../src/",

    //not working for custom templates, should be mref or not set
    //"outputFormat": "apiPage"
  }
]
```
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

Refer to [DocFx Config Reference](https://dotnet.github.io/docfx/reference/docfx-json-reference.html) for more details.

## `docfx-plus` **tool** features

- Support region includes (`<code source="..." region="...">`) across more language files (e.g. `.vb`, `.js`, `.aspx`).
  This will be useful especially when migrating from `SHFB`.
  
  In `.vb` source files, regions defined like this can now be included:
  ```vb
  #Region "Name"

  #End Region
  ```
  
  In `.js` source files, regions defined like this can now be included:
  ```js
  //#region Name
  
  //#endregion
  ``` 
  
  In xml-based source files, regions defined like this can now be included (just like `SHFB`): 
  ```xml
  <!-- #region Name -->

  <!-- #endregion -->
  ```
  This is in addition to `docfx` default format:
  ```xml
   <!-- <Name> -->

   <!-- </Name> -->
  ```
  
  `.aspx`, `.csproj`, `.slnx` are also added to `docfx` default xml-based formats `.xml`, `.xaml`, `.html`, `.cshtml`, `.vbhtml`.
 
- Support `tabSize` attribute (`<code tabSize="2">`) which is used when replacing tab (`\t`) characters 
  to spaces in the included source file (default is `4`).

- Support `title` attribute (`<code title="Custom">`) which is used when showing as code tab title instead of the language.

- Consistent line break handling so that code is always rendered correctly in the html.

  To prevent Yaml multi-line problems (`.yml` generated by `docfx`), use `<br>` tags for line breaks.
  We can't use `\n` as Yaml goes crazy (converts whole `example:` block to a string with `\r\n`).
  For example if a line is empty or whitespace, in Yaml it's written without any indentation
  and when reading that Yaml, it splits the blocks, tries to close the previous `<pre><code>` and open a new one.
  And this caused weird rendering problems for most code.
  
  In the web page, we can simply fix `highlight.js` to support unescaped `<br>` tags in `main.js` `configureHljs` function.

- Convert tab (`\t`) characters to spaces to always ensure correct indentation.
  
  Some source files or `<code>` contents can contain tab characters which can cause inconsistent indentation.

- Support nested `<code>` blocks for merging code even from different regions. 
  
  In your XML Comments (xmldocs) you can use nested `<code>` blocks:
  ```xml
  <code language="cs">
    <code source="Class1.cs" region="Region1" />
    <code source="Class2.cs" region="Region2" />
  </code>
  ```

  Literal code can also be mixed inside the parent `<code>` blocks.
  ```xml
  <code language="cs">
    // ... Some stuff happens here ...
 
    <code source="Class1.cs" region="Region1" />

    // ... Some stuff happens here ...

    <code source="Class2.cs" region="Region2" />
 
    // ... Some stuff happens here ...
  </code>
  ```
  Indentation from source files and literal code will all be normalized.

## `docfx-plus` **template** features

- In namespace, class, enum or member pages use `Name Type` format as the title e.g. `CliContext Class` instead of `Class CliContext`.
  
  Display `Definition` heading after the title heading.
  
  Don't break the namespace into link parts next to `Namespace:` because e.g. the root namespace may not exist and cause a HTTP 404.
  
  Fix heading margins (`<h1>`, `<h2>` ...).
  
  ![docfx-plus-template-feature1](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature1.png)

- Display namespace members, class members and enum fields with tables with subtle borders.
  
  Display inherited members and extension methods together with other members (in their own member type group),
  instead of displaying them as a huge and isolated list under the class definition section.  
  Display `(Inherited from BaseClass)` note in the description.  
  Sort inherited members along with other members by name.

  ![docfx-plus-template-feature2](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature2.png)
 
- Elegant rendering of `<code>` blocks with white background and subtle borders.
  
  Group sibling `<code>` blocks which are for different languages (e.g. `vb` following `cs`), and display them as tabs.

  ```xml
  <code source="Class1.cs" />
  <code source="Class1.vb" />
  ```
  
  If there is text between `<code>` blocks or if they are for same language, they will not be grouped and will be displayed separately.

  Display always visible "code copy" button on the right-side of tabs.

  Add `cshtml-razor` grammar for `highlight.js`.
  
  ![docfx-plus-template-feature3](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature3.png)
 
- On member pages, display overloads with indentation separately from definition section and fix sub-heading levels (`<h2>`, `<h3>` ...).
  
  If no overloads, display single member.

  ![docfx-plus-template-feature4](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature4.png)

- Display values for `enum` fields along with name and description and sort them by values.

  ![docfx-plus-template-feature5](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature5.png)

- Display `Obsolete` badge if class, enum or member has `ObsoleteAttribute` attribute 
  and an in addition display an alert `div` under the badge if `ObsoleteAttribute` has a message.

  ![docfx-plus-template-feature6](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature6.png)

- Elegant rendering of TOC tree.

  Highlight current tree node.

  Remove root node `Namespaces` which causes unnecessary nesting and display all namespaces as root (though fixed in `v2.78.4` of `docfx`).

  ![docfx-plus-template-feature7](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature7.png)
 
- Add icon links to top toolbar via new template property `_appIconLinks` directly in `docfx.json` (no need to override `layout/_master.tmpl`):
 
  ```json
  "globalMetadata": {
      "_appIconLinks": [
        {
          "icon": "github",
          "href": "https://github.com/dotmake-build/command-line",
          "title": "GitHub"
        }
      ]
  }
  ```

  ![docfx-plus-template-feature8](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature8.png)

- Fix some URL issues:
 
  Use `./` instead of `index.html` for app logo by default to prevent canonical URL issues for search engines.
  
  Fix URLs starting with app relative path `~/` for TOC.  
  This is because we can't use e.g. `./` for `href` in `toc.yml`; we get `CircularTocInclusion` error
  as it tries to load itself at `./toc.yml`.  
  This way, we can use `~/.` as a workaround in `toc.yml` (we want to use clean directory URL and avoid using `index.html`)

## DocFx Tips:

### Using README.md in your docs:
  
- When you want to reference your repository's `README.md` file, you can create `index.md` next to `docfx.json` with contents:
  ```
  [!include [getting-started](../README.md)]
  ```
  Note that upper case `!INCLUDE` as noted in docs, may not work on some machines (probably due to turkish-I bug in docfx).  
  The path should be relative to the containing file.  
  `index.md` needs to exist anyway otherwise homepage (`/`) will not work so it's better to include `README.md` in this file 
  if there is nothing else to put on the homepage.
  
- Another way: if you define external .md files as content in `docfx.json` like this:
  ```
  "build": {
    "content": [
      {
        "files": ["README.md"],
        "src": "../",
        "dest": "docs"
      }
    ],
  ```
  dot-dot notation can not appear in `files` but it can appear in `src`.  
  `src` is relative to the `docfx.json` folder (config root).  
  The `dest` means the output subfolder (under `_site` by default) for the corresponding html file, e.g. `README.html`.
  
  Then in `toc.yml` you can reference the .md file:
  ```
  - name: Readme
    href: ~/../README.md
  - name: Prerequisites
    href: ~/../README.md#prerequisites
  ```
  Note that the path should match `src` and not `dest`. Using `~/` in case `toc.yml` is in a subfolder and not in config root.  
  You can also put the # fragment but the file will not be splitted, it will just be a fragment link.

### Publish to GitHub Pages:

Create a workflow file in your repository, e.g. `publish-docs.yml` file in `.github\workflows` folder with these contents:

```yml
# Your GitHub workflow file under .github/workflows/
# Trigger the action on push to main
on:
  push:
    branches:
      - main

# Sets permissions of the GITHUB_TOKEN to allow deployment to GitHub Pages
permissions:
  actions: read
  pages: write
  id-token: write

# Allow only one concurrent deployment, skipping runs queued between the run in-progress and latest queued.
# However, do NOT cancel in-progress runs as we want to allow these production deployments to complete.
concurrency:
  group: "pages"
  cancel-in-progress: false
  
jobs:
  publish-docs:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
    - name: Checkout
      uses: actions/checkout@v3
    - name: Dotnet Setup
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.x

    - run: dotnet tool update -g docfx-plus
    - run: docfx-plus docfx.json
      # use working-directory for docfx step otherwise codeSourceBasePath is not resolved correctly
      working-directory: docs

    - name: Upload artifact
      uses: actions/upload-pages-artifact@v3
      with:
        # Upload entire repository
        path: 'docs/_site'
    - name: Deploy to GitHub Pages
      id: deployment
      uses: actions/deploy-pages@v4
```

Then enable GitHub Actions for your repository's Pages settings as described here:  
[Publishing with a custom GitHub Actions workflow](https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site#publishing-with-a-custom-github-actions-workflow)

Now whenever you commit, your action will run automatically and publish your docs.

### Full `docfx.json` sample:

Refer to [docfx.json](https://github.com/dotmake-build/command-line/blob/main/docs/docfx.json) from our other project [DotMake Command-Line](https://github.com/dotmake-build/command-line).

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

