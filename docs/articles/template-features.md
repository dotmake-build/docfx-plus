# `docfx-plus` **template** features

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

  Fix header nav brand logo, title and buttons wrapping on phone and tablet sizes.
  
  ![docfx-plus-template-feature8](https://raw.githubusercontent.com/dotmake-build/docfx-plus/master/images/docfx-plus-template-feature8.png)

- Fix some URL issues:
 
  We fix URLs according to online or offline mode automatically when generating docs.
  
  When `_enableOfflineMode` set to `true`, we always use `index.html` in TOC hrefs, logo url and redirection pages
  (`redirect_url` metadata in markdown files) because directory links do not work when browsing offline html files.

  When we are building for online mode (web server mode), we remove `index.html` and use `./`
  to prevent canonical URL issues for search engines (we want to use clean directory URL and avoid using `index.html`).

  We also introduce a new theme property `_useDirsAsIndex`, which when set to `false`, can force to use `index.html`
  even for online mode (if you don't care about nice online URLs).
  For offline mode this is always set to `false` automatically.
  
  For example, you use `index.md` for `href` in `toc.yml`
  (you need to use `index.md` because if you use `./` for `href` in `toc.yml`; you get `CircularTocInclusion` error
  as it tries to load itself at `./toc.yml`):
  ```
  - name: "Overview"
    href: index.md
  ```
  For online mode, TOC and NavBar will have this link generated for href `./` automatically.
  For offline mode, it will use `index.html`.

  For example, you have a markdown file like this:
  ```
  ---
  redirect_url: articles/
  ---
  ```
  For online mode, the generated HTML will have a redirect meta tag with href `articles/` (same kept).
  For offline mode, the href will be automatically converted to `articles/index.html`.

  For example, you have a markdown file like this:
  ```
  ---
  redirect_url: articles/index.html
  ---
  ```
  For online mode, the generated HTML will have a redirect meta tag with href `articles/` (converted automatically).
  For offline mode, the href will be kept same as `articles/index.html`.

  The same logic applies for theme property `_appLogoUrl`.
