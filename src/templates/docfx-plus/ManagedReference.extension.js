// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var templateCommon = require('./template.common.js');

/**
 * This method will be called at the start of exports.transform in ManagedReference.html.primary.js
 */
exports.preTransform = function (model) {

//  if (model.name[0].value === "CliContext")
//    console.log(JSON.stringify(model.namespace));
  
  templateCommon.overrideTemplateMetadata(model);

  const modelType = getModelType(model);
  
  if (modelType.isClass && model.inheritedMembers) {
    const inheritedMembers = model.inheritedMembers
      .filter((x) => !x.isExternal)
      .map((x) => {
        if(!x.type) //to fix shouldHideTitleType
          x.type = x.name[0].value.lastIndexOf(")") > -1
            ? "method"
            : "property"; 
        
        //console.log(JSON.stringify(x));
        
        x._inherited = true;
        
        /*
        if (x.definition) {
          const lastDotIndex = x.definition.lastIndexOf(".");
          x.parentDefinition = x.definition.substring(0, lastDotIndex);
          console.log(x.parentDefinition);
        }
        */
        
        return x;
      });
    
    model.children = model.children
      ?.concat(inheritedMembers)
      .sort((a, b) => {
        const nameA = a.name[0].value.toUpperCase(); // ignore upper and lowercase
        const nameB = b.name[0].value.toUpperCase(); // ignore upper and lowercase
        if (nameA < nameB)
          return -1;
        
        if (nameA > nameB)
          return 1;
          
        // names must be equal
        return 0;
    });
  }

  if(modelType.isEnum && model.children) {
    model.children.forEach((item) => {
       const regex = /[\w\d]+\s?=\s?(\d+),?/gm;
       var m = regex.exec(item.syntax.content[0].value);
       if(m !== null)
        item._enumValue = parseInt(m[1]);
    });

    model.children = model.children
      .sort((a, b) => {
        if (a._enumValue < b._enumValue)
          return -1;
        
        if (a._enumValue > b._enumValue)
          return 1;
        
        return 0;
      });
  }
  
  checkAndSetObsoleted(model);
  //if (model.name[0].value === "LoadingMessage")
  //console.log(model.syntax?.content[0].value);

  //Strangely attributes are populated only for children when model is enum, but not when class, 
  //however for member model which also has children (self and overloads), it's populated (also syntax) 
  //so at least we can have obsolete badge on the individual member page.
  model.children?.forEach((item) => {
    checkAndSetObsoleted(item);
    //item.summary = "Obsolete " + item.summary;
    //if (item._obsoleted)
    //if (item.name[0].value === "LoadingMessage")
    //console.log(item.syntax?.content[0].value);
    //console.log(JSON.stringify(item));
    //return item;
  });
  
  //Mainly for determining if a member has overloads (show as standalone on separate member page)
  model._hasOverloads = (model.children?.length > 1 && !(modelType.isNamespace || modelType.isClass || modelType.isEnum));
  model.children?.forEach((item) => {
    item._headingLevel = modelType.isClass
                         ? 3
                         : model._hasOverloads ? 2 : 0;
    item._subheadingLevel = modelType.isClass
                            ? 4
                            : model._hasOverloads ? 3 : 2;
                         
  });
    

  return model;
}

/**
 * This method will be called at the end of exports.transform in ManagedReference.html.primary.js
 */
exports.postTransform = function (model) {
  return model;
}


function getModelType(model) {
  let modelType = {};
  
  switch (model.type?.toLowerCase()) {
    case 'namespace':
      modelType.isNamespace = true;
      break;
    case 'class':
    case 'interface':
    case 'struct':
    case 'delegate':
      modelType.isClass = true;
      break;
    case 'enum':
      modelType.isEnum = true;
      break;
    default:
      break;
  }
  
  return modelType;
}

function checkAndSetObsoleted(model) {
  const attr = model.attributes?.find((a) => a.type === "System.ObsoleteAttribute");
  
  model._obsoleted = attr ? true : false;
  model._obsoletedMessage = attr?.arguments?.[0].value;
}
