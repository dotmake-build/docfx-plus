// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var templateCommon = require('./template.common.js');

exports.transform = function (model) {
  //For offline mode always use index.html
  if (model._enableOfflineMode)
    model._useDirsAsIndex = false;

  if (model.redirect_url)
    model.redirect_url = templateCommon.fixHrefIndexHtml(model.redirect_url, model._useDirsAsIndex);
  
  return model;
}
