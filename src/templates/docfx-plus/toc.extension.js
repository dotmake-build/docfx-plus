// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var templateCommon = require('./template.common.js');

/**
 * This method will be called at the start of exports.transform in toc.html.js and toc.json.js
 */
exports.preTransform = function (model) {

  //For offline mode always use index.html
  if (model._enableOfflineMode)
    model._useDirsAsIndex = false;
  

  //Fix urls starting with app relative path ~/
  //This is because we can't use e.g. ./ for href in toc.yml, we get CircularTocInclusion error
  //because it tries to load itself at ./toc.yml
  //This way we can use ~/. as a workaround in toc.yml (we want to use clean directory url and avoid using index.html)
  //Only for non-api pages, e.g. for toc.json, toc.html
  //console.log(JSON.stringify(model));
  //if (!("memberLayout" in model))
  fixItemsHref(model.items, model._useDirsAsIndex);

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

function fixItemsHref(items, useDirsAsIndex) {
  items?.forEach((item) => {
    item.href = templateCommon.fixHrefIndexHtml(item.href, useDirsAsIndex);

    fixItemsHref(item.items, useDirsAsIndex);
  });
}
