// ==UserScript==
// @id          links
// @name        Links to QL related Websites
// @version     1.0
// @author      PredatH0r
// @description	Adds a "Links" menu with links to important QL related websites
// @unwrap
// ==/UserScript==

/*

Version 1.0
- first public release

*/

(function () {
  var menuCaption = "Links";

  function init() {
    nav.navbar[menuCaption] = {
      id: "eql_links",
      callback: "",
      submenu: {
        "Quakehub Videos": { newwindow: "http://quakehub.com/" },
        "ESreality Forum": { newwindow: "http://www.esreality.com/?a=post&forum=17" },
        "QLRanks": { newwindow: "http://www.qlranks.com/" },
        "QLStats": { newwindow: "http://ql.leeto.fi/#/" },
        "QL Console Guide": { newwindow: "http://www.regurge.at/ql/" },
        "Strafing Theory": { newwindow: "http://www.funender.com/quake/articles/strafing_theory.html" },
        "Spawn Logic": { newwindow: "http://graphics.ethz.ch/~gnoris/ql-spawns/aerowalkFakeDist.html" }
      }
    };
  }

  init();

})(window);