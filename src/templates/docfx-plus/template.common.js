exports.preTransform = preTransform;
exports.postTransform = postTransform;
exports.fixHrefIndexHtml = fixHrefIndexHtml;

function preTransform(model) {
}

function postTransform(model) {
  if (model._enableOfflineMode) {
    model._navJsRel = model._navRel.replace(/.html$/gi, '.js');
    model._tocJsRel = model._tocRel.replace(/.html$/gi, '.js');

    //For offline mode always use index.html
    model._useDirsAsIndex = false;
  }

  if (model.redirect_url)
    model.redirect_url = fixHrefIndexHtml(model.redirect_url, model._useDirsAsIndex);

  if (!model._appLogoUrl)
    model._appLogoUrl = model._rel;
  model._appLogoUrl = fixHrefIndexHtml(model._appLogoUrl, model._useDirsAsIndex)

  if (model._appIconLinks) {
    if (!Array.isArray(model._appIconLinks))
      model._appIconLinks = [model._appIconLinks];

    model._appIconLinks = JSON.stringify(model._appIconLinks);
  }

  if (model._appFooter)
    model._appFooter = replaceBuildDate(model._appFooter);
}


function fixHrefIndexHtml(href, useDirsAsIndex) {
  if (!href)
    return href;

  href = href.trim();

  if (!href)
    return href;

  if (/^https?:\/\//i.test(href))
    return href;

  if (useDirsAsIndex === false) {
    if (/\.html?$/i.test(href))
      return href;

    if (href === "." || href.endsWith("/."))
      href = href.slice(0, -1);

    if (href == "./")
      href = "";

    return href.endsWith("/") || (href.length === 0)
      ? href + "index.html"
      : href + "/index.html";
  }

  // index.html is represented by the directory itself.
  return href.replace(/(^|\/)index\.html?$/i, (match, p1) => {
    return (p1.length === 0) ? "./" : p1;
  });
}

function replaceBuildDate(input) {
  // "{%40BuildDate}." → "12/03/2025, 21:32:10"
  // "{%40BuildDate:MM/dd/yyyy H:mm:ss}" → "12/03/2025 21:29:45"
  const regex = /\{@BuildDate(?::([^}]+))?\}/;

  return input.replace(regex, (_, fmt) => formatDate(new Date(), fmt));
}

// Simple formatter that maps .NET-style tokens to JS values
function formatDate(date, netFormat) {
  // Default format if none specified
  if (!netFormat)
    return date.toLocaleString();
  
  const pad = (n, w = 2) => String(n).padStart(w, '0');
  return netFormat
    .replace(/yyyy/g, date.getFullYear())
    .replace(/yy/g, String(date.getFullYear()).slice(-2))
    .replace(/MM/g, pad(date.getMonth() + 1))
    .replace(/M/g, date.getMonth() + 1)
    .replace(/dd/g, pad(date.getDate()))
    .replace(/d/g, date.getDate())
    .replace(/HH/g, pad(date.getHours()))
    .replace(/H/g, date.getHours())
    .replace(/hh/g, pad(date.getHours() % 12 || 12))
    .replace(/h/g, date.getHours() % 12 || 12)
    .replace(/mm/g, pad(date.getMinutes()))
    .replace(/ss/g, pad(date.getSeconds()));
}

