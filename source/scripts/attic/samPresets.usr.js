// ==UserScript==
// @name           Quake Live Start-a-Match quick preset access
// @description    Adds links to load your saved presets directly from the start-a-match screen
// @author         PredatH0r
// @version        1.2
// @include        http://*.quakelive.com/*
// @exclude        http://*.quakelive.com/forum*
// @unwrap
// ==/UserScript==

/*

Version 1.2
- updated to work with latest UI changes

Version 1.1
- updated extraQL script url to sourceforge

*/

(function() {
  var amplify = window.amplify;
  var quakelive = window.quakelive;

  function init() {
    extraQL.addStyle(
      "#quickSamPresets { background-color: #AAA; color: black; padding: 3px 15px; }",
      "#quickSamPresets a { color: black; }"
    );
    quakelive.AddHook("OnContentLoaded", onContentLoaded);
    onContentLoaded();
  }

  function onContentLoaded() {
    if (!quakelive.activeModule || quakelive.activeModule.GetTitle() != "Create Match")
      return;
    if ($("#quickSamPresets").length > 0)
      return;

    var html = "<div id='quickSamPresets'>";
    var presets = amplify.store("sam_presets") || {};
    var count = 0;
    $.each(presets, function(name) {
      html += count++ == 0 ? "Load preset: " : " | ";
      html += "<a href='javascript:void(0)'>" + extraQL.escapeHtml(name) + "</a>";
    });
    if (count == 0)
      html += "No saved presets available";
    html += "</div>";
    $("#mod_sam").prepend(html);
    $("#quickSamPresets a").click(function () { quakelive.mod_startamatch.loadPreset($(this).text()); });
  }
  
  init();
})(window);

