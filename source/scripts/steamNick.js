// ==UserScript==
// @name           SteamNick: Adds cvars to change your Steam nickname
// @version        1.3
// @author         PredatH0r
// @description    /steamnick <name> sets your Steam nickname to <name>
// @description    /sn_clan + /sn_name + /sn_suffix are combined into /steamnick
// @description    Only the sn_* variables will change your Steam Nick when QL is loaded.
// @description    You can use /steamnick in combination with AutoExec's gamestart.cfg/gameend.cfg.
// @enabled        1
// ==/UserScript==

/*

Version 1.3
- added /sn_suffix

Version 1.2
- improved error handling when name change fails

version 1.1
- added /sn_name and /sn_clan

Version 1.0
- first user script designed to work with Steam exclusive version of Quake Live

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;

  // constants
  var STEAMNICK_CVAR = "steamnick";
  var NAME_CVAR = "sn_name";
  var CLAN_PREFIX_CVAR = "sn_clan";
  var CLAN_SUFFIX_CVAR = "sn_suffix";

  function init() {
    var steamName = qz_instance.GetCvar("name");

    var clanPrefix = qz_instance.GetCvar(CLAN_PREFIX_CVAR);
    if (!clanPrefix) {
      // make sure the CVAR exists
      qz_instance.SetCvar(CLAN_PREFIX_CVAR, "");
      clanPrefix = "";
    }

    var clanSuffix = qz_instance.GetCvar(CLAN_SUFFIX_CVAR);
    if (!clanSuffix) {
      // make sure the CVAR exists
      qz_instance.SetCvar(CLAN_SUFFIX_CVAR, "");
      clanSuffix = "";
    }

    var nickname = qz_instance.GetCvar(NAME_CVAR);
    if (!nickname) {
      // make sure the CVAR exists
      qz_instance.SetCvar(NAME_CVAR, steamName);
      nickname = "";
    }


    // if any of the name/tag cvars are set, then modify the steam name accordingly
    if (clanPrefix || clanSuffix || nickname) {
      var steamnick = clanPrefix + nickname + clanSuffix;
      qz_instance.SetCvar(STEAMNICK_CVAR, steamnick);
      onSteamNickCvarChanged({ name: STEAMNICK_CVAR, value: steamnick });
    }
    else
      qz_instance.SetCvar(STEAMNICK_CVAR, steamName);


    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("cvar." + STEAMNICK_CVAR, onSteamNickCvarChanged);
    channel.subscribe("cvar." + CLAN_PREFIX_CVAR, onSteamNickCvarChanged);
    channel.subscribe("cvar." + CLAN_SUFFIX_CVAR, onSteamNickCvarChanged);
    channel.subscribe("cvar." + NAME_CVAR, onSteamNickCvarChanged);
    echo("^2steamNick.js installed");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    msg = msg.replace(/\"/g, "'").replace(/[\r\n]+/g, " ");
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onSteamNickCvarChanged(data) {
    // German keyboard cannot enter ^ (it's the console key), so allow \ for colors too. Updating the cvar will call this function again.
    var value = data.value.replace(/\\/g, "^"); 
    if (value != data.value) {
      qz_instance.SetCvar(data.name, value);
      return;
    }

    // if any of the sn_* cvars was changed, combine them into "steamnick", which will cause another call to this function
    if (data.name != STEAMNICK_CVAR) {
      var newNick = qz_instance.GetCvar(CLAN_PREFIX_CVAR) + qz_instance.GetCvar(NAME_CVAR) + qz_instance.GetCvar(CLAN_SUFFIX_CVAR);
      if (newNick)
        qz_instance.SetCvar(STEAMNICK_CVAR, newNick);
      return;
    }
   
    // don't allow empty steam nicknames
    if (!value || value == qz_instance.GetCvar("name"))
      return;

    // call the extraQL.exe servlet which changes the Steam nickname through a steam_api.dll call
    var xhttp = new XMLHttpRequest();
    xhttp.timeout = 10000;
    xhttp.onload = function () {
      if (xhttp.status == 200)
        echo("^3/" + data.name + " changed successfully.^7");
      else {
        echo("^1/" + data.name + " failed: ^7" + xhttp.responseText);
        qz_instance.SetCvar(STEAMNICK_CVAR, qz_instance.GetCvar("name"));
      }
    }
    xhttp.onerror = function() {
       echo("^1/" + STEAMNICK_CVAR + " timed out. ^7Please make sure extraQL.exe 2.0 (or newer) is running on your PC.");
       qz_instance.SetCvar(STEAMNICK_CVAR, qz_instance.GetCvar("name"));
    }
    xhttp.open("GET", "http://localhost:27963/steamnick?name=" + encodeURIComponent(value), true);
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

