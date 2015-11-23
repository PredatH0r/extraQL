// ==UserScript==
// @name           InstaBounce: Automate special config with key binds for InstaBounce gametype
// @version        1.1
// @author         PredatH0r
// @description    InstaBounce is InstaGib + 0-Damage-Bounce-Rockets + Grappling hook.
// @description    This script automatically detects this game type and executes the config files
// @description    ibounce_on.cfg and ibounce_off.cfg to setup aliases and key binds.
// @description    The aliases +hook and +rock allow you to instantly switch and fire weapons.
// @description    Your config is restored when you leave an InstaBounce server.
// @enabled        1
// ==/UserScript==

/*

Version 1.1
- fixed issues with restoring original config

Version 1.0
- initial release

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;

  // constants
  var CVAR_DisableMsg = "cg_disableInstaBounceBindMsg";
  var CVAR_InstaBounce = "cg_instaBounce";
  var IB_DISABLED = "-1";
  var IB_AUTODETECT = "0";
  var IB_ACTIVE = "1";

  // internal variables
  var ignoreEvents = false;
  var mainMenuTimer;


  function init() {
    restoreNormalConfig(); // in case someone used /quit and the normal config wasn't restored
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("cvar.ui_mainmenu", onUiMainMenu); // used to detect in-game vs. menu-mode
    channel.subscribe("cvar.cg_spectating", onSpectating); // wait for the player to join so he can see the "print" message
    var ib = qz_instance.GetCvar(CVAR_InstaBounce);
    var status = ib == IB_AUTODETECT || ib == IB_ACTIVE ? "auto-detect" : "disabled";
    echo("^2instaBounce.js installed^7 (" + CVAR_InstaBounce + "=" + ib + ": " + status + ")");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    msg = msg.replace(/\"/g, "'").replace(/[\r\n]+/g, " ");
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onUiMainMenu(data) {
    if (ignoreEvents) return;

    // When changing maps, ui_mainMenu gets set to 1 and back to 0.
    // To prevent unnecessary config resetting and console flooding, we use a timeout
    if (data.value == "0") {
      if (mainMenuTimer)
        clearTimeout(mainMenuTimer);
      checkFactoryForInstaBounce();
    }
    else {
      mainMenuTimer = setTimeout(restoreNormalConfig, 1000);
    }
  }

  function onSpectating(data) {
    if (ignoreEvents) return;
    if (parseInt(data.value) == 0 && qz_instance.GetCvar(CVAR_InstaBounce) == IB_ACTIVE)
      showKeyBindMessage();
  }

  function restoreNormalConfig() {
    mainMenuTimer = undefined;
    if (qz_instance.GetCvar(CVAR_InstaBounce) != IB_ACTIVE)
      return;
    ignoreEvents = true;
    qz_instance.SendGameCommand("exec ibounce_off.cfg");
    ignoreEvents = false;
    qz_instance.SetCvar(CVAR_InstaBounce, "1");
    echo("^3instaBounce.js:^7 restored normal config (ibounce_off.cfg)");
  }

  function checkFactoryForInstaBounce() {
    if (qz_instance.GetCvar(CVAR_InstaBounce) == IB_DISABLED)
      return;
    qz_instance.SendGameCommand("serverinfo");
    qz_instance.SendGameCommand("condump extraql_condump.txt");
    setTimeout(function() {
      var xhttp = new XMLHttpRequest();
      xhttp.timeout = 1000;
      xhttp.onload = function() { extraQLCondumpOnLoad(xhttp); }
      xhttp.onerror = function() { echo("^3extraQL.exe not running:^7 run extraQL.exe to auto-exec ibounce_on/off.cfg when connecting to (non-)InstaBounce servers."); }
      xhttp.open("GET", "http://localhost:27963/condump", true);
      xhttp.send();
    }, 100);
  }

  function extraQLCondumpOnLoad(xhttp) {
    var isInstaBounce = false;
    if (xhttp.status == 200) {
      var condump = xhttp.responseText;
      var idx, idx2;
      if ((idx = condump.lastIndexOf("g_factoryTitle")) >= 0 && (idx2 = condump.indexOf("\n", idx)) >= 0) {
        var factory = condump.substr(idx + 14, idx2 - idx - 14).trim();
        if (factory.indexOf("InstaBounce") >= 0)
          isInstaBounce = true;
      }
      else {
        log("cound not detect g_factory in condump");
      }
    }
    else
      log("^1instaBounce.js:^7 failed to load serverinfo/condump through extraQL.exe: " + xhttp.responseText);

    if (isInstaBounce) {
      if (qz_instance.GetCvar(CVAR_InstaBounce) == IB_AUTODETECT) {
        qz_instance.SendGameCommand("writeconfig ibounce_off.cfg"); // backup current config
        // writeconfig doesn't write immediately, so we have to defer the config changes
        setTimeout(function() {
          qz_instance.SendGameCommand("exec ibounce_on.cfg");
          qz_instance.SetCvar(CVAR_InstaBounce, IB_ACTIVE);
          echo("^3instaBounce.js:^7 activated InstaBounce config (ibounce_on.cfg)");
        }, 1000);
      }
    }
    else
      restoreNormalConfig();
  }

  function showKeyBindMessage() {
    if (qz_instance.GetCvar(CVAR_DisableMsg) == "1")
      return;
    var msg = "^3Edit^7 Quake Live/<steam-id>/baseq3/^3ibounce_on.cfg^7 to ^5bind +hook^7 and ^5+rock^7 to your preferred keys/buttons and disable this message.";
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
    qz_instance.SendGameCommand("print \"" + msg + "\"");
    msg = "^7Your original config will be restored automatically. You can also force it with ^5exec ibounce_off.cfg^7.";
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
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

