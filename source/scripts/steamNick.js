// ==UserScript==
// @id             steamNick
// @name           steamNick
// @version        1.0
// @author         PredatH0r
// @description    add a /steamnick command to change steam nick name
// @unwrap
// ==/UserScript==

/*

Version 1.0
- first user script designed to work with Steam exclusive version of Quake Live

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;

  // constants
  var STEAMNICK_CVAR = "steamnick";

  function init() {    
    qz_instance.SetCvar(STEAMNICK_CVAR, qz_instance.GetCvar("name"));
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("cvar." + STEAMNICK_CVAR, onSteamNickCvarChanged);
    log("/" + STEAMNICK_CVAR + " installed");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onSteamNickCvarChanged(data) {
    var name = data.value;
    if (!name)
      return;

    var xhttp = new XMLHttpRequest();
    xhttp.timeout = 10000;
    xhttp.onload = function () {
      if (xhttp.status == 200)
        echo("^3/" + STEAMNICK_CVAR + " changed successfully.^7");
      else
        echo("^1/" + STEAMNICK_CVAR + " failed: ^7" + xhttp.responseText);
    }
    xhttp.onerror = function () { echo("^1/" + STEAMNICK_CVAR + " timed out. ^7Please make sure extraQL.exe 1.21 (or newer) is running on your PC."); }
    xhttp.open("GET", "http://localhost:27963/steamnick?name=" + encodeURIComponent(name), true);
    xhttp.send();
  }


  // there is a race condition between QL's bundle.js and the userscripts.
  // if window.req was published by bundle.js, we're good to go.
  // otherwise add a callback to main_hook_v2, which will be called by bundle.js later
  if (window.req)
    init();
  else {
    var oldHook = window.main_hook_v2;
    window.main_hook_v2 = function() {
      if (typeof oldHook == "function")
        oldHook();
      init();
    }
  }
})();

