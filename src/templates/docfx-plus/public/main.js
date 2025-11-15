import { default as hljsRazor } from "./cshtml-razor.es.min.js";
let globalHljs;

export default {
  start: () => {
    groupCodeBlocks();
  },  
  configureHljs: (hljs) => {
    globalHljs = hljs;
    
    hljs.registerLanguage("cshtml-razor", hljsRazor);
    
    // use innerText as the content source...
    hljs.addPlugin({
      "before:highlightElement": ({ el }) => {
        //update:  no more required as we don't need delayed highlighting 
        //because  the problem was caused by el.textContent = el.innerText
        //If element is not displayed (display: none), e.g. it's inside a hidden tab panel,
        //prevent highlighting as it will be wrongly formatted.
        //we will highlight those when tab is shown (display: block)
        /*if (!el.offsetParent) {
          el.dataset.originalClass = el.className
          el.className = "no-highlight";
          return;
        }
        */

        //UPDATE: No more needed, we fix Yaml without <br> tags
        //Convert unescaped <br> tags which we use instead of \n
        //to fix multi-line problems for <pre><code> tags in Yaml
        /*
        el.querySelectorAll('br').forEach(
          br => br.replaceWith(document.createTextNode('\n'))
        );
        */
        //don't use innerText because when display: none it will not preserve line breaks
        //el.textContent = el.innerText;
        //DOM replace above will be usually faster for long text, so don't use regex replace
        //el.innerHTML = el.innerHTML.replace(/<br />/g, "");
        
        //We will have our own code-copy button inside tabs so remove docfx added one
        //We need to remove here because code-action is added in renderMarkdown() 
        //which is called after main.js (start event) but before highlight()
        el.parentElement?.querySelector("a.code-action")?.remove();
      }
    });
  },
  iconLinks: docfx.iconLinks
}

function groupCodeBlocks() {
  const allPreCodeEls = Array.from(document.querySelectorAll("pre code"));
  const visited = new Set();
  let groupId = 0;

  allPreCodeEls.forEach(codeEl => {
    const preEl = codeEl.parentElement;
    if (visited.has(preEl)) return;

    const group = [preEl];
    let nextPreEl = preEl.nextElementSibling;
    const lang = getLang(codeEl);
    
    while (
      nextPreEl &&
      nextPreEl.tagName === "PRE"
    ) {
      const nextCodeEl = nextPreEl.querySelector("code");
      if (!nextCodeEl || getLang(nextCodeEl) === lang)
        break;
        
      group.push(nextPreEl);
      visited.add(nextPreEl);
      nextPreEl = nextPreEl.nextElementSibling;
    }

    groupId++;
    createCodeBlockTabs(group, groupId);

    visited.add(preEl);
  });
}

function createCodeBlockTabs(group, groupId) {
  const firstPre = group[0];

  const tabsEl = createElementFromHtml('<ul class="code-tabs nav nav-tabs" role="tablist"></ul>');
  firstPre.before(tabsEl);
  
  const tabPanesEl = createElementFromHtml('<div class="code-tabs-content tab-content border border-top-0"></div>');
  tabsEl.after(tabPanesEl);

  group.forEach((preEl, i) => {
    const codeEl = preEl.querySelector("code");
    
    const lang = getLang(codeEl);
    setLang(codeEl,  lang ?? "txt"); //fix lang class with supported one
    
    const langId = lang ?? ("lang" + i);
    const langTitle = getLangTitle(codeEl) ?? toLangTitle(getFileType(codeEl) ?? lang);
    
    const tabId = "code-tab-" + groupId + "-" + langId;
    const tabPaneId = "code-tabpanel-" + groupId + "-" + langId;
    const active = (i == 0) ? "active" : "";
    const selected = (i == 0) ? "true" : "false";

    const tabEl = createElementFromHtml(`
      <li class="nav-item" role="presentation">
        <button class="nav-link ${active}" id="${tabId}" data-bs-toggle="tab" data-bs-target="#${tabPaneId}"
        type="button" role="tab" aria-controls="${tabPaneId}" aria-selected="${selected}">${langTitle}</button>
      </li>`);
    
    
    const tabPaneEl = createElementFromHtml(`
      <div class="tab-pane ${active}" id="${tabPaneId}" aria-labelledby="${tabId}" 
       role="tabpanel" tabindex="0"></div>`);
    
    tabsEl.appendChild(tabEl);
    tabPanesEl.appendChild(tabPaneEl);
    tabPaneEl.appendChild(preEl);
    
    //update:  no more required as we don't need delayed highlighting 
    //because  the problem was caused by el.textContent = el.innerText
    /*
    const tabButtonEl = tabEl.querySelector("button");
    tabButtonEl.addEventListener('shown.bs.tab', e => {
      const activeTabButtonEl = e.target // newly activated tab
      const activeTabPaneEl = document.querySelector(activeTabButtonEl.dataset.bsTarget);
      const activeCodeEl = activeTabPaneEl.querySelector("pre code");
      
      if (!activeCodeEl.dataset.originalClass)
        return;
      
      activeCodeEl.className = activeCodeEl.dataset.originalClass;
      delete activeCodeEl.dataset.originalClass;
      
      globalHljs.highlightElement(activeCodeEl);
    });
    */
  });
  
  const codeCopyEl = createElementFromHtml(`
    <button type="button" class="code-copy btn btn-outline-subtle">
      <i class="bi bi-copy"></i>
      ${loc('copy')}
    </button>`);

  codeCopyEl.addEventListener("click", async  e => {
    e.preventDefault();
    
    const activeTabPaneEl = tabPanesEl.querySelector("div.tab-pane.active");
    const activeCodeEl = activeTabPaneEl.querySelector("pre code");

    const text = activeCodeEl.textContent?.trim() || "";
    await navigator.clipboard.writeText(text);
    
    const copyCls = "bi-copy";
    const copiedCls = "bi-check-lg";
    const iconEl = codeCopyEl.querySelector("i");
    iconEl.classList.replace(copyCls, copiedCls);
    
    setTimeout(() => {
      const transitionend = () => {
        iconEl.classList.replace(copiedCls, copyCls);
        iconEl.removeEventListener("transitionend", transitionend);
        iconEl.style.opacity = 1;
      }
      
      iconEl.style.opacity = 1;
      iconEl.addEventListener("transitionend", transitionend);
      iconEl.style.opacity = 0;
    }, 1000);
  });

  tabsEl.appendChild(codeCopyEl);
}

function getLang(codeEl) {
  const langPrefix = "lang-";
  const langCls = Array.from(codeEl.classList).find(cls => cls.startsWith(langPrefix));
  const lang = (langCls?.slice(langPrefix.length) ?? getFileType(codeEl))?.toLowerCase();

  //Handle some language fallbacks (to make compatible with highlight.js) 
  switch (lang)
  {
    case "c#":
    case "csharp":
    case "aspx":
      return "cs";
    case "vbhtml":
      return "vb";
    case "xaml":
      return "xml";
    case "none":
      return null;
    default:
      return lang;
  }
}

function setLang(codeEl, lang) {
  const langPrefix = "lang-";
  const langCls = Array.from(codeEl.classList).find(cls => cls.startsWith(langPrefix));
  const newLangCls = langPrefix + lang;
  if (langCls)
    codeEl.classList.replace(langCls, newLangCls);
  else
    codeEl.classList.add(newLangCls);
}

function getFileType(codeEl) {
  return codeEl.dataset.fileType?.toLowerCase();
}

function getLangTitle(codeEl) {
  return codeEl.dataset.title;
}

function toLangTitle(lang) {
  switch (lang)
  {
    case "csharp":
    case "cs":
        return "C#";
    default:
        return lang?.toUpperCase() ?? "CODE";
  }
}

function createElementFromHtml(html) {
  var template = document.createElement("template");
  template.innerHTML = html.trim();
  return template.content.firstChild;
}

function meta(name) {
  return (document.querySelector(`meta[name="${name}"]`))?.content
}

function loc(id, args) {
  let result = meta(`loc:${id}`) || id
  if (args) {
    for (const key in args) {
      result = result.replace(`{${key}}`, args[key])
    }
  }
  return result
}
