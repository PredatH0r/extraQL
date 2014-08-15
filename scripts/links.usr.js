// ==UserScript==
// @id          links
// @name        Links to QL related Websites
// @version     3.0
// @author      PredatH0r
// @description	Adds a "Links" menu with links to important QL related websites
// @unwrap
// ==/UserScript==

/*

Version 3.0
- added some links

Version 2.0
- added some links
- added separator lines to group links

Version 1.0
- first public release

*/

(function () {
  var menuCaption = "Links";

  extraQL.addStyle("ul.sf-menu li ul li.sep { border-top: 1px solid #888; }");

  function init() {
    nav.navbar[menuCaption] = {
      id: "eql_links",
      callback: "",
      submenu: {
        "Quakenet IRChat": { newwindow: "http://webchat.quakenet.org/?nick=" + quakelive.username + "&channels=quakelive&prompt=1" },
        "ESReality Forum": { newwindow: "http://www.esreality.com/?a=post&forum=17" },
        "Reddit Forum": { newwindow: "http://www.reddit.com/r/quakelive"},
        "Quakehub Videos": { newwindow: "http://quakehub.com/" },
        "Quake History": { newwindow: "http://www.quakehistory.com/" },
        "QLRanks": { newwindow: "http://www.qlranks.com/", "class": "sep" },
        "QLStats": { newwindow: "http://ql.leeto.fi/#/" },
        "FaceIt Cups": { newwindow: "http://play.faceit.com/" },
        "125fps League": { newwindow: "https://twitter.com/125fps" },
        "vHUD Editor": { newwindow: "http://visualhud.pk69.com/", "class": "sep" },
        "Wolfcam/Whisperer": { newwindow: "http://www.wolfwhisperer.net/"},
        "Yakumo's QL Guide": { newwindow: "http://www.quakelive.com/forum/showthread.php?831-The-Ultimate-Quake-Live-Guide", "class": "sep" },
        "QL Console Guide": { newwindow: "http://www.regurge.at/ql/" },
        "Strafing Theory": { newwindow: "http://www.funender.com/quake/articles/strafing_theory.html" },
        "Duel Spawn Logic": { newwindow: "http://graphics.ethz.ch/~gnoris/ql-spawns/aerowalkFakeDist.html" }
      }
    };
  }

  init();

})(window);