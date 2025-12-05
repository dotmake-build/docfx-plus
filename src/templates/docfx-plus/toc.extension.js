// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/**
 * This method will be called at the start of exports.transform in toc.html.js and toc.json.js
 */
exports.preTransform = function (model) {

  //Fix urls starting with app relative path ~/
  //This is because we can't use e.g. ./ for href in toc.yml, we get CircularTocInclusion error
  //because it tries to load itself at ./toc.yml
  //This way we can use ~/. as a workaround in toc.yml (we want to use clean directory url and avoid using index.html)
  //Only for non-api pages, e.g. for toc.json, toc.html
  //console.log(JSON.stringify(model));
  if (!("memberLayout" in model))
    fixTildeHref(model.items);

  return model;
}

/**
 * This method will be called at the end of exports.transform in toc.html.js and toc.json.js
 */
exports.postTransform = function (model) {
  //Remove root node "Namespaces" which causes unnecessary nesting and display all namespaces as root
  if (model.memberLayout === 'SeparatePages') {
    if (model.items && model.items.length === 1 && model.items[0].name === model.__global.namespacesInSubtitle)
      model.items = model.items[0].items;
  }

  return model;
}

function fixTildeHref(items) {
  items?.forEach((item) => {
    if (item.href?.startsWith("~/"))
      item.href = "./" + item.href.substring(2);

    fixTildeHref(item.items);
  });
}
