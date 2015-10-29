// ==UserScript==
// @id             autoExec
// @name           autoExec
// @version        2.0
// @author         PredatH0r
// @description    executes commands when you start/leave a game
// @unwrap
// ==/UserScript==

/*

Version 2.0
- rewrite to work with Steam exclusive version of Quake Live

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;
  var lastGameEndTimestamp;

  function init() {    
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("game.start", onGameStart);
    channel.subscribe("game.end", onGameEnd);
    echo("^2autoExec.js installed");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onGameStart() {
    echo("^3autoExec.js: executing gamestart.cfg");
    qz_instance.SendGameCommand("exec gamestart.cfg");
    autoSwitchFullscreen(1);
  }

  function onGameEnd() {
    var now = Date.now();

    // QL sends 2x "game.end" notifications and r_fullscreen is latched between the two, so we don't know the real state
    // this hack ignores the 2nd if it's within 2 seconds of the first
    if (lastGameEndTimestamp + 2000 > now) 
      return;
    lastGameEndTimestamp = now;

    echo("^3autoExec.js: executing gameend.cfg");
    qz_instance.SendGameCommand("exec gameend.cfg");
    autoSwitchFullscreen(0);
  }

  function autoSwitchFullscreen(enter) {
    var auto = qz_instance.GetCvar("r_autoFullscreen");
    var mask = enter ? 0x01 : 0x02;
    if (auto != "" && (parseInt(auto) & mask) && qz_instance.GetCvar("r_fullscreen") != enter) {
      //log("autoExec.js: switching to " + (enter ? "fullscreen" : "windowed mode"));
      extraQl_SetFullscreen(enter);

      // extraQL may not be running or QL might have ignored the Alt+Enter keypress (when disconnecting), so check again after a timeout
      setTimeout(1000, function () {
        if (qz_instance.GetCvar("r_fullscreen") != enter)
          qz_instance.SendGameCommand("set r_fullscreen " + mode + "; vid_restart");
      });
    }
  }

  function extraQl_SetFullscreen(mode) {
    var xhttp = new XMLHttpRequest();
    xhttp.timeout = 1000;
    xhttp.onload = function () {
      if (xhttp.status == 200) {
        log("autoExec.js: switched r_fullscreen=" + mode + " through extraQL.exe");
      } else
        log("autoExec.js: failed to switch r_fullscreen=" + mode + " through extraQL.exe: " + xhttp.responseText);
    }
    xhttp.onerror = function () { echo("^3extraQL.exe not running:^7 run extraQL.exe to prevent a map-reload when switching fullscreen"); }
    xhttp.open("GET", "http://localhost:27963/toggleFullscreen?mode=" + mode, true);
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

