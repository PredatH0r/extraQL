// ==UserScript==
// @name           Whois: Adds a /whois command to show alias nicknames stored on qlstats.net
// @version        1.0
// @author         PredatH0r
// @description    Use "/whois nickname -or- client-id (from /players)"
// @enabled        1
// ==/UserScript==

/*

Version 1.0
- first release

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;

  // constants
  var CVAR_whois = "whois";
  var HelpText = "a user script command. Use ^3" + CVAR_whois +" help^7 to get some help.";
  var ConfigstringsMarker = "]\\configstrings";

  // state variables
  var pendingAjaxRequest = null;
  var playerCache = { timestamp: 0, players: {} }
  var PREFS = { method: "echo" }
  PREFS.set = function (setting, value) { PREFS[setting] = value; }

  function init() {
    // create cvar
    qz_instance.SetCvar(CVAR_whois, HelpText);

    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("cvar." + CVAR_whois, onCommand);

    echo("^2whois.js installed");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    msg = msg.replace(/\"/g, "'").replace(/[\r\n]+/g, " ");
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onCommand(data) {
    var val = data.value;
    if (val == HelpText)
      return;
    qz_instance.SetCvar(CVAR_whois, HelpText);

    if (val == "help")
      showHelp();
    else if (val == "update")
      playerCache.timestamp = 0;
    else
      showAliases(val);
  }

 
  function showHelp() {
    qz_instance.SendGameCommand("echo Usage: ^5/" + CVAR_whois + "^7 <^3nickname^7 or ^3player-id^7> (use /players to list player-ids)");
    qz_instance.SendGameCommand("echo ^5/" + CVAR_whois + "^3 update^7: force update of /player information");
  }


  function showAliases(arg) {
    if (pendingAjaxRequest) {
      echo("Please wait for the current request to complete...");
      return;
    }
    requestPlayerInformation(arg, showInformation);

    function showInformation(aliases) {
      var method = PREFS.method;
      var lines = [];
      for (var steamid in pendingAjaxRequest) {
        if (!pendingAjaxRequest.hasOwnProperty(steamid)) continue;
        var req = pendingAjaxRequest[steamid];
        var player = aliases[steamid];

        var nicks = [ /*req.name*/ ];
        if (player) {
          for (var strippedNick in player) {
            if (!player.hasOwnProperty(strippedNick)) continue;
            var alias = player[strippedNick];
            //if (alias.nick != req.name)
              nicks.push(alias.nick);
          }
        }
        lines.push(("0" + req.clientid).substr(-2) + " " + nicks.join("^3 aka ^7"));
      }

      if (lines.length == 0)
        lines.push("No aliases known for " + arg);

      var totalDelay = 0;
      var msgDelay = method == "say" || method == "say_team" ? 1000 : method == "print" ? 100 : 0;
      lines.forEach(function (line) {
        setTimeout(function () { qz_instance.SendGameCommand(method + "\"^3whois: ^7" + line + "\"") }, totalDelay);
        totalDelay += msgDelay;
      });
    }
  }

 
  function requestPlayerInformation(arg, callback) {
    if (Date.now() - playerCache.timestamp < 60000) {
      requestAliasInformation(arg, playerCache.players, callback);
      return;
    }

    qz_instance.SendGameCommand("echo " + ConfigstringsMarker); // text marker required by extraQL servlet
    qz_instance.SendGameCommand("configstrings");
    setTimeout(function () {
      qz_instance.SendGameCommand("condump extraql_condump.txt");
      setTimeout(function () {
        var xhttp = new XMLHttpRequest();
        xhttp.timeout = 1000;
        xhttp.onload = function () { onExtraQLServerInfo(arg, xhttp, callback); }
        xhttp.onerror = function () {
          echo("^3extraQL.exe not running:^7");
          pendingAjaxRequest = null;
        }
        xhttp.open("GET", "http://localhost:27963/serverinfo", true);
        xhttp.send();
      }, 100);
    }, 1000);


    function onExtraQLServerInfo(arg, xhttp, callback) {
      if (xhttp.status != 200) {
        pendingAjaxRequest = null;
        return;
      }

      var json = null;
      try {
        json = JSON.parse(xhttp.responseText);
      } catch (err) {
      }
      if (!json || !json.players) {
        pendingAjaxRequest = null;
        return;
      }

      playerCache.players = json.players;
      playerCache.timestamp = Date.now();
      requestAliasInformation(arg, json.players, callback);
    }

    function requestAliasInformation(arg, players, callback) {
      var steamIds = [];
      pendingAjaxRequest = {};
      for (var i = 0; i < players.length; i++) {
        var obj = players[i];
        var player = { "steamid": obj.st, "name": obj.n.toLowerCase(), "clientid": obj.clientid };
        if (arg == "*" || player.name.indexOf(arg) >= 0 || player.clientid.toString() == arg) {
          steamIds.push(player.steamid);
          pendingAjaxRequest[player.steamid] = player;
        }
      }

      if (steamIds.length == 0) {
        pendingAjaxRequest = null;
        return;
      }
      var url = "http://qlstats.net:8080/aliases/" + steamIds.join("+") + ".json";
      var xhttp = new XMLHttpRequest();
      xhttp.timeout = 5000;
      xhttp.onload = function () { onQlstatsAliases(xhttp, callback); }
      xhttp.onerror = function () { echo("^1elo.js:^7 could not get data from qlstats.net"); }
      xhttp.open("GET", url, true);
      xhttp.send();
    }

    function onQlstatsAliases(xhttp, callback) {
      if (xhttp.status == 200)
        callback(JSON.parse(xhttp.responseText));
      pendingAjaxRequest = null;
    }
  }

  


  // there is a race condition between QL's bundle.js and the userscripts.
  // if window.req was published by bundle.js, we're good to go.
  // otherwise add a callback to main_hook_v2, which will be called by bundle.js later
  try {
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
  } catch (err) {
    console.log(err);
  }
})();

