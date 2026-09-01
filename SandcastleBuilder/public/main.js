import { default as hljsRazor } from "./cshtml-razor.es.min.js";

export default {
  start: () => {
  },

  configureHljs: (hljs) => {
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

    docfx.hljs = hljs;
    //This should not be called in start() above because we need docfx.hljs in getLangFallback which is used by this:
    docfx.groupCodeBlocks();
  },

  iconLinks: docfx.iconLinks
}
