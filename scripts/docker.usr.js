// ==UserScript==
// @id          docker
// @name        Window Docker
// @version     1.0
// @author      PredatH0r
// @description	Adds a menu that allows you to dock the QL window to a side of your screen using your full screen height or width
// @note        This script requires extraQL.exe to be running!
// @unwrap
// ==/UserScript==

/*
 Adds a menu that allows you to
 - toggle full screen
 - dock the QL window to a side of your screen (with full desktop width or height)
 - move the QL window into a screen corner
 - set the window to the minimal size (1024x768)

 This script only works when extraQL.exe is running on your PC.

 CVARS:
 - web_dockWidth: width of QL window when docked to left/right screen border
 - web_dockHeight: height of QL window when docked to top/bottom screen border
*/

(function () {
  // external symbols
  var quakelive = window.quakelive;
  var extraQL = window.extraQL;

  var SET_FULLSCREEN = -1;
  var SET_MINSIZE = 0;
  var SET_LEFT = 0x01;
  var SET_RIGHT = 0x02;
  var SET_WIDTH = SET_LEFT | SET_RIGHT;
  var SET_TOP = 0x04;
  var SET_BOTTOM = 0x08;
  var SET_HEIGHT = SET_TOP | SET_BOTTOM;

  var URL_BASE = extraQL.BASE_URL;

  var CVAR_DOCKWIDTH = "web_dockWidth";
  var CVAR_DOCKHEIGHT = "web_dockHeight";

  function init() {
    if (extraQL.isOldUi)
      return;

    extraQL.addStyle(
      "#winposControl { float: right; padding: 0px 30px; }",
      "#winposControl:hover { background-color: #323232; }",
      "#winposControl img { width: 18px; height: 18px; margin-top: 5px; margin-bottom: 5px; cursor: pointer; background-color: white; }",
      "#winposControl img:hover { background-color: #F6D000; }",
      "#winposPopup { position: absolute; top: 110px; left: 774px; display: none; background-color: #222; padding: 6px; z-index: 1000; }",
      "#winposPopup.hoverBar { display: block; }",
      "#winposPopup.hoverPopup { display: block; }",
      "#winposPopup img { width: 18px; height: 18px; margin: 3px; cursor: pointer; background-color: white; }",
      "#winposPopup img:hover { background-color: #F6D000; }"
      );

    quakelive.AddHook("OnContentLoaded", onContentLoaded);
  }

  function onContentLoaded() {
    if ($("#winposControl").length)
      return;

    if (extraQL.isOldUi)
      return;

    $("#tn_settings").after(
        "<li id='winposControl'>"
      + "  <img data-mode='" + SET_FULLSCREEN + "' src='" + URL_BASE + "images/maxsize.png' title='Toggle fullscreen'/>"
      + "</li>");

    $("#qlv_content").append(
          "<div id='winposPopup'>"
          + "<div>"
          + "  <img data-mode='" + (SET_TOP|SET_LEFT) + "' src='" + URL_BASE + "images/topleft.png' title='Move to top left'>"
          + "  <img data-mode='" + (SET_TOP|SET_WIDTH) + "' src='" + URL_BASE + "images/top.png' title='Dock top'>"
          + "  <img data-mode='" + (SET_TOP|SET_RIGHT) + "' src='" + URL_BASE + "images/topright.png' title='Move to top right'>"
          + "</div>"
          + "<div>"
          + "  <img data-mode='" + (SET_LEFT|SET_HEIGHT) + "' src='" + URL_BASE + "images/left.png' title='Dock left, toggle extra width for chat'>"
          + "  <img data-mode='" + SET_MINSIZE + "' src='" + URL_BASE + "images/minsize.png' title='Toggle 1328x768 / 1024x768'>"
          + "  <img data-mode='" + (SET_RIGHT|SET_HEIGHT) + "' src='" + URL_BASE + "images/right.png' title='Dock right, toggle extra width for chat'>"
          + "</div>"
          + "<div>"
          + "  <img data-mode='" + (SET_BOTTOM|SET_LEFT) + "' src='" + URL_BASE + "images/bottomleft.png' title='Move to bottom left'>"
          + "  <img data-mode='" + (SET_BOTTOM|SET_WIDTH) + "' src='" + URL_BASE + "images/bottom.png' title='Dock bottom'>"
          + "  <img data-mode='" + (SET_BOTTOM|SET_RIGHT) + "' src='" + URL_BASE + "images/bottomright.png' title='Move to bottom right'>"
          + "</div>" +
          "</div>"
          );
    var $popup = $("#winposPopup");
    $("#winposControl").hover(
      function() { if (quakelive.cvars.Get("r_fullscreen").value == "0") $popup.addClass("hoverBar"); },
      function () { window.setTimeout(function() { $popup.removeClass("hoverBar"); }, 100); });
    $popup.hover(
      function () { $popup.addClass("hoverPopup"); },
      function () { window.setTimeout(function () { $popup.removeClass("hoverPopup"); }, 500); });
    $("#winposControl img").unbind("click").click(setMode);
    $("#winposPopup img").unbind("click").click(setMode);
  }

  function setMode() {
    move(parseInt($(this).data("mode")));
  }

  function move(action) {
    if (action == SET_FULLSCREEN) {
      $.ajax({ url: URL_BASE + "toggleFullscreen" })
        .fail(extraQLNotResponding);
    } else {
      if (quakelive.cvars.Get("r_fullscreen").value == "1")
        return;
      var width = quakelive.cvars.Get(CVAR_DOCKWIDTH).value;
      var height = quakelive.cvars.Get(CVAR_DOCKHEIGHT).value;
      $.ajax({ url: URL_BASE + "dockWindow?sides=" + action +"&w=" + width + "&h=" + height})
        .fail(extraQLNotResponding);
    }
  }

  function extraQLNotResponding() {
    window.qlPrompt({ title: 'docker.js', body: "extraQL HTTP server is not responding", fatal: false, alert: true });
  }

  if (extraQL)
    init();
  else
    $.getScript("https://raw.githubusercontent.com/PredatH0r/extraQL/master/scripts/extraQL.js", init);
})();
