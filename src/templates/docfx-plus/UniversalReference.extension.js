// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var templateCommon = require('./template.common.js');

/**
 * This method will be called at the start of exports.transform in UniversalReference.html.primary.js
 */
exports.preTransform = function (model) {
  templateCommon.overrideTemplateMetadata(model);

  return model;
}

/**
 * This method will be called at the end of exports.transform in UniversalReference.html.primary.js
 */
exports.postTransform = function (model) {
  return model;
}
