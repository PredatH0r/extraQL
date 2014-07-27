// ==UserScript==
// @id             110327
// @name           Escaper
// @version        1.6.1
// @namespace      phob.net
// @author         wn
// @contributor    PredatH0r
// @description    Press escape to close Quake Live's Game Summary, Live Game Info and notification popups
// @unwrap
// ==/UserScript==

(function() {
  function QLE() {
    document.addEventListener("keyup", function(e) {
      if (e.keyCode != 27) return;
      window.quakelive.matchtip.HideMatchTooltip(-1);
      window.jQuery("#stats_details, #ql_notifier .ql_notice").remove();
    }, false);
  }

  var scriptNode = document.createElement("script");
  scriptNode.setAttribute("type", "text/javascript");
  scriptNode.text = "(" + QLE.toString() + ")();";
  document.body.appendChild(scriptNode);
})();