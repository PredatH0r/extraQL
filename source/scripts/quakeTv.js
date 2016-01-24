// ==UserScript==
// @name           QuakeTV: prototype for automated connect + spec of high rated matches
// @version        0.2
// @author         PredatH0r
// @description    Use "/qtv help" to get a list of supported commands
// @description    Use "/qtv 0" to stop auto connecting + following
// @description    Use "/set qtv_autoConnect 1" to automatically connect and spec when you start QL
// @enabled        0
// ==/UserScript==

/*

Version 0.2
- combined various commands and cvars into "/qtv".
- added support to follow the n-th best match and connect to the next best match if a connection can't be established within 10sec

Version 0.1
- proof of concept

*/

var $;

(function() {
  var serverBrowserData = [];
  var connectTimer = null;
  var followTimer = null;
  var bestMatchRank = 0;
  var active = false;
  var following = null;

  var HELP_MSG = "use ^2\\qtv help^7 to get a list of qtv commands";

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    msg = msg.replace(/\"/g, "'").replace(/[\r\n]+/g, " ");
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onQtvCommand(data) {
    var val = data.value.toLowerCase();
    if (val == HELP_MSG)
      return;
    qz_instance.SetCvar("qtv", HELP_MSG);

    val.split(",").forEach(function(val) {
      val = val.trim();
      if (val == "help")
        showHelp();
      else if ("all,eu,na,sa,eu,as,af".indexOf(val) >= 0)
        qz_instance.SetCvar("qtv_region", val);
      else if ("duel,ffa,ca,tdm,ctf,ft".indexOf(val) >= 0) {
        qz_instance.SetCvar("qtv_gameType", val);
        connect();
      }
      else if (val == "0") {
        active = false;
        clearTimeout(connectTimer);
        clearTimeout(followTimer);
        qz_instance.SendGameCommand("team s");
      }
      else if (parseInt(val) > 0)
        connect(val - 1);
      else if (val == "follow") {
        following = null;
        followBestPlayerOnCurrentServer();
      }
    });
  }

  function showHelp() {
    echo("Use ^5qtv^7 ^3command^7(s) with these commands:");
    echo("^3duel^7,^3ffa^7,...   set gametype and connect to the best match");
    echo("^3all^7,^3eu^7,...     set region to all,eu,na,sa,au,as,af");
    echo("^3connect^7        connect to the best match with the current gametype");
    echo("^3follow^7         switches POV to the highest rated player on the current server");
    echo("^31^7..^310^7          connect+follow the n-th best match");
    echo("^30^7              stop automatic connecting and following");
    echo("Multiple commands can be separated with comma, spaces are NOT allowed");
  }

  function connect(matchRank) {
    var region = qz_instance.GetCvar("qtv_region") || 0;
    var gametype = qz_instance.GetCvar("qtv_gametype") || "duel";
    bestMatchRank = matchRank || 0;
    active = true;
    following = null;

    if (connectTimer)
      clearTimeout(connectTimer);
    if (followTimer)
      clearTimeout(followTimer);

    $.getJSON("http://api.qlstats.net/api/nowplaying", { region: region }, function(data) {
        if (!data[gametype] || !data[gametype][0]) {
          connectTimer = setTimeout(connect, 60 * 1000);
          return;
        }

        var bestMatches = data[gametype];
        var curAddr = qz_instance.GetCvar("cl_currentServerAddress") || "";
        var bestAddr = null;
        if (bestMatchRank >= bestMatches.length)
          bestMatchRank = 0;
        for (var i = bestMatchRank, c = bestMatches.length; i < c; i++) {
          if (bestMatches[i].opponent) {
            bestAddr = bestMatches[i].server;
            serverBrowserData = [{ steamid: bestMatches[i].steamid, team: 1 }]; // fake some initial server browser data, real stuff will be received later
            break;
          }
        }

        if (!bestAddr) {
          // can't find a good match, so stay on the current server and check again in 10sec
          bestAddr = curAddr;
          connectTimer = setTimeout(connect, 10 * 1000);
        }

        if (curAddr == bestAddr && qz_instance.GetCvar("ui_mainmenu") == "0") {
          // if already connected to the best match, just ensure we follow the best player
          followBestPlayerOnCurrentServer();
          return;
        }

        qz_instance.SendGameCommand("connect " + bestAddr);
        $.getJSON("http://localhost:27963/restartOBS", function() {
          echo("^2quakeTv:^7 notified extraQL to restart OBS");
        });
        connectTimer = setTimeout(connectionTimedOut, 15 * 1000);
      })
      .fail(function(err) {
        echo("^1quakeTv:^7 failed to get list of top matches from qlstats.net: " + err);
        connectTimer = setTimeout(connect, 5 * 1000);
      });   
  }

  function connectionTimedOut() {
    // happens when server is down or passworded
    connect(bestMatchRank + 1);
  }

  function onGameStart() {
    
  }

  function onGameEnd() {
    clearTimeout(connectTimer);
    connectTimer = setTimeout(connect, 7 * 1000);
  }

  function onUiMainMenu(data) {
    // when changing maps, ui_mainMenu gets set to 1 and back to 0.
    if (data.value == "0") {
      qz_instance.SendGameCommand("team s");
      if (active) {
        following = null;
        followBestPlayerOnCurrentServer();
      }

      // stop connection timeout
      clearTimeout(connectTimer);
      connectTimer = null;
    }
  }

  function followBestPlayerOnCurrentServer() {
    if (followTimer)
      clearTimeout(followTimer);

    $.getJSON("http://api.qlstats.net/api/server/" + qz_instance.GetCvar("cl_currentServerAddress") + "/players", function(data) {
      if (data.ok) {
        serverBrowserData = data.players;
        serverBrowserData.sort(function(a, b) { return (b.rating || 0) - (a.rating || 0); });
        serverBrowserData = serverBrowserData.reduce(function(agg, p) {
          if (p.team >= 0 && p.team <= 2 && !p.quit)
            agg.push(p);
          return agg;
        }, []);
        if (serverBrowserData.length < 2) {
          // connect to a new server when there are fewer than 2 players on the current server
          connect();
        }
        else if (serverBrowserData[0].steamid != following) {
          // either a higher rated player joined, or the highest rated player left or moved to spec
          switchPov();
        }
      }
    });


    function switchPov() {
      // execute "/players" to print a mapping from steam-id to client-id in the console, 
      // dump the console log to a file, call extraQL to read the log file content, ...
      echo("]\\players");
      qz_instance.SendGameCommand("players");
      setTimeout(function() {
        qz_instance.SendGameCommand("condump extraql_condump.txt");
        setTimeout(function() {
          $.get("http://localhost:27963/condump", function(condump) {
            var players = getPlayersFromCondump(condump);
            var playersById = players.reduce(function(agg, p) {
              agg[p.steamid] = p;
              return agg;
            }, {});

            var p = null;
            for (var i = 0, c = serverBrowserData.length; i < c; i++) {
              p = playersById[serverBrowserData[i].steamid];
              if (p)
                break;
              p = null;
            }

            if (!p) {
              // connect to a different server when nobody was found to spec
              connect(bestMatchRank == 0 ? 1 : 0);
              return;
            }

            echo("following client id " + p.clientid + ": " + p.name);
            qz_instance.SendGameCommand("follow " + p.clientid);
            following = p.steamid;
          });
        }, 100);
      }, 1000);

      followTimer = setTimeout(followBestPlayerOnCurrentServer, 10 * 1000);
    }
  }

  function getPlayersFromCondump(condump) {
    var idx = condump.lastIndexOf("]\\players");
    if (idx < 0) {
      return null;
    }
    var players = [];
    var lines = condump.substring(idx).split('\n');
    lines.forEach(function(line) {
      var match = /^(?:\[\d+:\d\d\.\d+\] )?([ \d]\d) (\d+) (.) (.+)$/.exec(line);
      if (match)
        players.push({ clientid: parseInt(match[1].trim()), opflag: match[3], name: match[4], steamid: match[2] });
    });
    return players;
  }

  function main() {
    // load jQuery if not already loaded and then try again
    if (!$) {
      var script = document.createElement("script");
      script.src = "https://code.jquery.com/jquery-2.2.0.min.js";
      script.onload = function() {
        $ = window.jQuery;
        main();
      }
      document.documentElement.firstChild.appendChild(script);
      return;
    }

    qz_instance.SetCvar("qtv", HELP_MSG);

    // install QL event listeners
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("game.start", onGameStart);
    channel.subscribe("game.end", onGameEnd);
    channel.subscribe("cvar.ui_mainmenu", onUiMainMenu);
    channel.subscribe("cvar.qtv", onQtvCommand);
    echo("^2quakeTv.js installed");

    if (qz_instance.GetCvar("qtv_autoConnect") == "1")
      setTimeout(connect, 3000);
  }

  // there is a race condition between QL's bundle.js and the userscripts.
  // if window.req was published by bundle.js, we're good to go.
  // otherwise add a callback to main_hook_v2, which will be called by bundle.js later
  if (window.req)
    main();
  else {
    var oldHook = window.main_hook_v2;
    window.main_hook_v2 = function () {
      if (typeof oldHook == "function")
        oldHook();
      main();
    }
  }
})();