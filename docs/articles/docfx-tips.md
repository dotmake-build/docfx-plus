# DocFx Tips:

## Using README.md in your docs:
  
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
