// ==UserScript==
// @id           startpage
// @name         Start Page
// @description  Opens QL with your preferred start page. Set any page as your start page with "Userscripts / Set Start Page"
// @author       PredatH0r
// @version      1.0
// @unwrap
// ==/UserScript==

(function () {
  function init() {
    var home = quakelive.cvars.Get("web_home");
    if (home && home.value)
      window.location.href = "/#!" + home.value;

    HOOK_MANAGER.addMenuItem("Set as Start Page", setStartPage);
  }

  function setStartPage() {
    var page = window.location.hash;
    if (page.length > 2) {
      page = page.substr(2);
      quakelive.cvars.Set("web_home", page);
    }
  }

  init();
})();