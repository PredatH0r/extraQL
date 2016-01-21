// ==UserScript==
// @name           QuakeTV: prototype for automated connect + spec of high rated matches
// @version        0.1
// @author         PredatH0r
// @description    Use "/qtv_gametype ffa" (or ca,duel,tdm,ctf,ft) to switch gametype for the next connect
// @description    Use "/qtv_connect 1" (or bind a key to that command) to connect/spec the best player
// @description    Use "/qtv_follow 1" (or bind a key to that command) to follow the best player on the current server
// @description    Use "/qtv_disconnect 1" (or bind a key to that command) to stop automated following
// @description    Use "/qtv_autoConnect 1" to automatically connect and spec when you start QL
// @enabled        0
// ==/UserScript==

/*

Version 0.1
- proof of concept

*/

var $;

(function() {
  var bestPlayers = [];
  var reconnectTimer = null;
  var followTimer = null;

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    msg = msg.replace(/\"/g, "'").replace(/[\r\n]+/g, " ");
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onQtvConnect(data) {
    if (data.value != "1")
      return;
    qz_instance.SetCvar("qtv_connect", "0");
    reconnect();
  }

  function onQtvFollow(data) {
    if (data.value != "1")
      return;
    qz_instance.SetCvar("qtv_follow", "0");
    followBestPlayer();
  }

  function onQtvDisconnect(data) {
    if (data.value != "1")
      return;
    qz_instance.SetCvar("qtv_disconnect", "0");
    clearTimeout(reconnectTimer);
    clearTimeout(followTimer);
  }

  function reconnect() {
    var region = qz_instance.GetCvar("qtv_region") || 0;
    var gametype = qz_instance.GetCvar("qtv_gametype") || "duel";

    if (reconnectTimer)
      clearTimeout(reconnectTimer);

    $.getJSON("http://api.qlstats.net/api/nowplaying", { region: region }, function(data) {
        if (!data[gametype] || !data[gametype][0]) {
          reconnectTimer = setTimeout(reconnect, 60 * 1000);
          return;
        }

        bestPlayers = data[gametype];
        var curAddr = qz_instance.GetCvar("cl_currentServerAddress") || "";
        var bestAddr = null;
        for (var i = 0, c = data[gametype].length; i < c; i++) {
          if (gametype != "duel" || data[gametype][i].opponent) {
            bestAddr = data[gametype][0].server;
            break;
          }
        }
        if (!bestAddr) {
          bestAddr = curAddr;
          reconnectTimer = setTimeout(reconnect, 10 * 1000);
        }

        if (curAddr == bestAddr) {
          followBestPlayer();
          return;
        }

        qz_instance.SendGameCommand("connect " + bestAddr);
        $.getJSON("http://localhost:27963/restartOBS", function() {
          echo("^2quakeTv:^7 notified extraQL to restart OBS");
        });
      })
      .fail(function(err) {
        echo("^1quakeTv:^7 failed to get list of top matches from qlstats.net: " + err);
        reconnectTimer = setTimeout(reconnect, 5 * 1000);
      });   
  }

  function onGameStart() {
    
  }

  function onGameEnd() {
    reconnectTimer = setTimeout(reconnect, 10 * 1000);
  }

  function onUiMainMenu(data) {
    // When changing maps, ui_mainMenu gets set to 1 and back to 0.
    // To prevent unnecessary config resetting and console flooding, we use a timeout
    if (data.value == "0") {
      qz_instance.SendGameCommand("team s");
      followBestPlayer();
    }
  }

  function followBestPlayer() {
    if (followTimer)
      clearTimeout(followTimer);

    qz_instance.SendGameCommand("clear");
    echo("]\\players");
    qz_instance.SendGameCommand("players");
    setTimeout(function() {
      qz_instance.SendGameCommand("condump extraql_condump.txt");
      setTimeout(function() {
        $.get("http://localhost:27963/condump", function(condump) {
          var players = getPlayersFromCondump(condump);

          log("players on server: " + JSON.stringify(players));
          log("best players: " + JSON.stringify(bestPlayers));

          var playersById = players.reduce(function(agg, p) {
            agg[p.steamid] = p;
            return agg;
          }, {});
          var p = null;
          for (var i = 0, c = bestPlayers.length; i < c; i++) {
            p = playersById[bestPlayers[i].steamid];
            if (p)
              break;
          }
          if (!p)
            p = players[0];
          if (p) {
            echo("following client id " + p.clientid + ": " + p.name);
            qz_instance.SendGameCommand("follow " + p.clientid);
          }
          else {
            echo("nobody found to follow");
          }
        });
      }, 100);
    }, 1000);

    followTimer = setTimeout(followBestPlayer, 10 * 1000);
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

    // install QL event listeners
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("game.start", onGameStart);
    channel.subscribe("game.end", onGameEnd);
    channel.subscribe("cvar.ui_mainmenu", onUiMainMenu);
    channel.subscribe("cvar.qtv_connect", onQtvConnect);
    channel.subscribe("cvar.qtv_disconnect", onQtvDisconnect);
    channel.subscribe("cvar.qtv_follow", onQtvFollow);
    echo("^2quakeTv.js installed");

    if (qz_instance.GetCvar("qtv_autoConnect") == "1")
      setTimeout(reconnect, 3000);
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