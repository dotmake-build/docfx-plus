//This file will also be used by offline/main.js so it should not be ES module, it should be a regular script file

window.docfx = window.docfx || {};

(function () {

  this.groupCodeBlocks = function() {
    const allPreCodeEls = Array.from(document.querySelectorAll("pre code"));
    const visited = new Set();
    let groupId = 0;

    allPreCodeEls.forEach(codeEl => {
      const preEl = codeEl.parentElement;
      if (visited.has(preEl)) return;

      const group = [preEl];
      let nextPreEl = preEl.nextElementSibling;
      const lang = this.getLang(codeEl);
      const groupedLangs = new Set();
      groupedLangs.add(lang);
      
      while (
        nextPreEl &&
        nextPreEl.tagName === "PRE"
      ) {
        const nextCodeEl = nextPreEl.querySelector("code");
        if (!nextCodeEl)
          break;
        const nextLang = this.getLang(nextCodeEl);
        if (groupedLangs.has(nextLang))
          break;
          
        group.push(nextPreEl);
        visited.add(nextPreEl);
        groupedLangs.add(nextLang);
        nextPreEl = nextPreEl.nextElementSibling;
      }

      groupId++;
      this.createCodeBlockTabs(group, groupId);

      visited.add(preEl);
    });
  }

  this.createCodeBlockTabs = function(group, groupId) {
    const firstPre = group[0];

    const tabsEl = this.createElementFromHtml('<ul class="code-tabs nav nav-tabs" role="tablist"></ul>');
    firstPre.before(tabsEl);
    
    const tabPanesEl = this.createElementFromHtml('<div class="code-tabs-content tab-content border border-top-0"></div>');
    tabsEl.after(tabPanesEl);

    group.forEach((preEl, i) => {
      const codeEl = preEl.querySelector("code");
      
      const lang = this.getLang(codeEl);
      const langFallback = this.getLangFallback(lang);
      this.setLang(codeEl, langFallback); //fix lang class with supported one
      
      const langId = langFallback ?? ("lang" + i);
      const langTitle = this.getLangTitle(codeEl) ?? this.toLangTitle(this.getFileType(codeEl) ?? lang);
      
      const tabId = "code-tab-" + groupId + "-" + langId;
      const tabPaneId = "code-tabpanel-" + groupId + "-" + langId;
      const active = (i == 0) ? "active" : "";
      const selected = (i == 0) ? "true" : "false";

      const tabEl = this.createElementFromHtml(`
        <li class="nav-item" role="presentation">
          <button class="nav-link ${active}" id="${tabId}" data-bs-toggle="tab" data-bs-target="#${tabPaneId}"
          type="button" role="tab" aria-controls="${tabPaneId}" aria-selected="${selected}">${langTitle}</button>
        </li>`);
      
      
      const tabPaneEl = this.createElementFromHtml(`
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
        
        this.hljs.highlightElement(activeCodeEl);
      });
      */
    });
    
    const codeCopyEl = this.createElementFromHtml(`
      <button type="button" class="code-copy btn btn-outline-subtle">
        <i class="bi bi-copy"></i>
        ${this.loc('copy')}
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

  this.getLang = function(codeEl) {
    const langPrefix = "lang-";
    const langCls = Array.from(codeEl.classList).find(cls => cls.startsWith(langPrefix));
    const lang = (langCls?.slice(langPrefix.length) ?? this.getFileType(codeEl))
      ?.toLowerCase()
      ?.trim();

    return (lang === "" || lang === "none" || lang === undefined)
      ? null
      : lang;
  }

  this.getLangFallback = function(lang) {
    if (lang && !this.isString(lang))
      lang = this.getLang(lang);

    //Handle some language fallbacks (to make compatible with highlight.js) 
    switch (lang)
    {
      case "c#":
      case "csharp":
      case "aspx":
      case "aspx-cs":
        return "cs";
      case "vbhtml":
      case "aspx-vb":
        return "vb";
      case "xaml":
      case "config":
      case "csproj":
      case "slnx":
        return "xml";
      case "none":
      case "":
      case null:
        return "txt";
      default:
        return this.hljs.getLanguage(lang) ? lang : "txt";
    }
  }

  this.setLang = function(codeEl, lang) {
    const langPrefix = "lang-";
    const langCls = Array.from(codeEl.classList).find(cls => cls.startsWith(langPrefix));
    const newLangCls = langPrefix + lang;
    if (langCls)
      codeEl.classList.replace(langCls, newLangCls);
    else
      codeEl.classList.add(newLangCls);
  }

  this.getFileType = function(codeEl) {
    return codeEl.dataset.fileType?.toLowerCase();
  }

  this.getLangTitle = function(codeEl) {
    return codeEl.dataset.title ?? codeEl.getAttribute("name");
  }

  this.toLangTitle = function(lang) {
    switch (lang)
    {
      case "csharp":
      case "cs":
          return "C#";
      default:
          return lang?.toUpperCase() ?? "CODE";
    }
  }

  this.createElementFromHtml = function(html) {
    var template = document.createElement("template");
    template.innerHTML = html.trim();
    return template.content.firstChild;
  }

  this.meta = function(name) {
    return (document.querySelector(`meta[name="${name}"]`))?.content
  }

  this.loc = function(id, args) {
    let result = this.meta(`loc:${id}`) || id
    if (args) {
      for (const key in args) {
        result = result.replace(`{${key}}`, args[key])
      }
    }
    return result
  }

  this.isString = function(param) {
    return typeof param === "string" || param instanceof String;
  }

}).call(window.docfx);
