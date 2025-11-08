exports.overrideTemplateMetadata = overrideTemplateMetadata;

function overrideTemplateMetadata(model) {
  if (model._appIconLinks) {
    if (!Array.isArray(model._appIconLinks))
      model._appIconLinks = [model._appIconLinks];

    model._appIconLinks = JSON.stringify(model._appIconLinks);
  }
  
  model._appFooter = '<span>Made with <a href="https://dotmake.build/docfx-plus/">docfx-plus</a></span>';
}
