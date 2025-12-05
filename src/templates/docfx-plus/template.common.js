exports.overrideTemplateMetadata = overrideTemplateMetadata;

function overrideTemplateMetadata(model) {
  if (model._appIconLinks) {
    if (!Array.isArray(model._appIconLinks))
      model._appIconLinks = [model._appIconLinks];

    model._appIconLinks = JSON.stringify(model._appIconLinks);
  }

  if (model._appFooter)
    model._appFooter = replaceBuildDate(model._appFooter);
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

