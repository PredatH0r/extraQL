// ==UserScript==
// @name           instaBounce
// @version        1.0
// @author         PredatH0r
// @description    sets up the aliases needed for playing the InstaBounce game type
// ==/UserScript==

/*

Version 1.0
- initial release

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;
  var ibounceActive = false;
  var checkFactory = false;

  function init() {
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("game.start", onGameStart);
    channel.subscribe("cvar.cg_spectating", onSpectating); // fires after connecting and loading the map
    channel.subscribe("game.end", onGameEnd);
    echo("^2instaBounce.js installed");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    msg = msg.replace(/\"/g, "'").replace(/[\r\n]+/g, " ");
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onGameStart() {
    checkFactory = true;
  }

  function onSpectating() {
    if (checkFactory) {
      checkFactoryForInstaBounce();
      checkFactory = false;
    }
    else if (qz_instance.GetCvar("cg_disableInstaBounceBindMsg") != "1") {
      var msg = "^7Edit ^3ibounce_on.cfg^7 to ^5bind^7 your preferred keys/buttons to ^5+hook^7 and ^5+rock^7 and disable this message.";
      qz_instance.SendGameCommand("echo \"" + msg + "\"");
      qz_instance.SendGameCommand("print \"" + msg + "\"");
      msg = "^7When you disconnect, your original config will be restored automatically. You can also force it manually with ^5exec ibounce_off^7.";
      qz_instance.SendGameCommand("echo \"" + msg + "\"");
    }
    return;
  }

  function checkFactoryForInstaBounce() {
    qz_instance.SendGameCommand("serverinfo");
    qz_instance.SendGameCommand("condump extraql_condump.txt");
    var xhttp = new XMLHttpRequest();
    xhttp.timeout = 1000;
    xhttp.onload = function () {
      if (xhttp.status == 200) {
        var condump = xhttp.responseText;
        var idx, idx2;
        if ((idx = condump.lastIndexOf("g_factoryTitle")) >= 0 && (idx2 = condump.indexOf("\n", idx)) >= 0) {
          var factory = condump.substr(idx + 14, idx2 - idx - 14).trim();
          qz_instance.SendGameCommand("writeconfig ibounce_off.cfg");
          if (factory.indexOf("InstaBounce") >= 0) {
            echo("^3instBounce.js:^7 executing ibounce_on.cfg");
            qz_instance.SendGameCommand("exec ibounce_on.cfg");
            ibounceActive = true;
          }
        } else {
          log("cound not detect g_factory");
        }
      } else
        log("instaBounce.js: failed to load serverinfo/condump through extraQL.exe: " + xhttp.responseText);
    }
    xhttp.onerror = function () { echo("^3extraQL.exe not running:^7 run extraQL.exe to allow auto-execing ibounce.cfg when connecting to InstaBounce servers."); }
    xhttp.open("GET", "http://localhost:27963/condump", true);
    xhttp.send();
  }

  function onGameEnd() {
    if (ibounceActive) {
      log("^3instaBounce.js:^7 restoring previous config");
      qz_instance.SendGameCommand("exec ibounce_off.cfg");
      ibounceActive = false;
    }
  }


  // there is a race condition between QL's bundle.js and the userscripts.
  // if window.req was published by bundle.js, we're good to go.
  // otherwise add a callback to main_hook_v2, which will be called by bundle.js later
  if (window.req)
    init();
  else {
    var oldHook = window.main_hook_v2;
    window.main_hook_v2 = function () {
      if (typeof oldHook == "function")
        oldHook();
      init();
    }
  }
})();

