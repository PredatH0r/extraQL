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
  var NAME_CVAR = "sn_name";
  var CLAN_CVAR = "sn_clan";

  // state variables
  var clantag;
  var nickname;

  function init() {
    var steamName = qz_instance.GetCvar("name");

    clantag = qz_instance.GetCvar(CLAN_CVAR);
    if (!clantag) {
      // make sure the CVAR exists
      qz_instance.SetCvar(CLAN_CVAR, "");
      clantag = "";
    }

    nickname = qz_instance.GetCvar(NAME_CVAR);
    if (!nickname) {
      // make sure the CVAR exists
      qz_instance.SetCvar(NAME_CVAR, steamName);
      nickname = "";
    }


    // if sn_clan or sn_name are set, then modify the steam name accordingly
    if (clantag || nickname) {
      var steamnick = clantag + nickname;
      qz_instance.SetCvar(STEAMNICK_CVAR, steamnick);
      onSteamNickCvarChanged({ name: STEAMNICK_CVAR, value: steamnick });
    }
    else
      qz_instance.SetCvar(STEAMNICK_CVAR, steamName);


    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("cvar." + STEAMNICK_CVAR, onSteamNickCvarChanged);
    channel.subscribe("cvar." + CLAN_CVAR, onSteamNickCvarChanged);
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
      var newNick = qz_instance.GetCvar(CLAN_CVAR) + qz_instance.GetCvar(NAME_CVAR);
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

