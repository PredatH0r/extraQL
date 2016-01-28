// ==UserScript==
// @name           QuakeTV: automated connect + spec of high rated matches (based on qlstats.net)
// @version        1.0.1
// @author         PredatH0r
// @description    Use "/qtv help" to get a list of supported commands
// @description    Use "/qtv 0" to stop connecting + following
// @description    Use "/qtv 1..10" to connect + follow the n-th best match
// @description    Use "/qtv follow" to follow the best player on the current server only
// @description    Commands can be combined like: /qtv duel,all,1
// @enabled        0
// ==/UserScript==

/*

Version 1.0.1
- fixed: could not connect to same server again after disconnecting
- fixed: ignore "dead player" information for non-round-based games and always follow the top rated player
- fixed: no longer connecting to the best server after the match on a 2nd-best server started

Version 1.0
- using ZMQ relay stream from qlstats.net to get notified about player deaths/joins so POV can be updated instantly
- reduced console spam to a minimum


Version 0.3
- combined various commands and cvars into "/qtv".
- added support to follow the n-th best match and connect to the next best match if a connection can't be established within 10sec

Version 0.1
- proof of concept

*/

var $;

(function() {
  var gametype = "ca";
  var serverBrowserData = [];
  var connectTimer = null;
  var followTimer = null;
  var bestMatchRank = 0;
  var active = false;
  var following = null;
  var connectTime = 0;
  var mapStartTime = 0;
  var autoReconnect;
  var eventSource = null;
  var deadPlayers = [];
  var clientIdCache = null;

  var HELP_MSG = "use ^2\\qtv help^7 to get a list of qtv commands";

  function log(msg) {
    //console.log(msg);
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

    // multiple instructions can be specified, separated with a comma
    val.split(",").forEach(function(val) {
      var idx;
      val = val.trim();
      if (val == "help")
        showHelp();
      else if ((idx = ["all","eu","af","as","au","na","sa"].indexOf(val)) >= 0)
        qz_instance.SetCvar("qtv_region", idx);
      else if ("duel,ffa,ca,tdm,ctf,ft".indexOf(val) >= 0)
        qz_instance.SetCvar("qtv_gameType", val);
      else if (val == "0") {
        active = false;
        autoReconnect = false;
        unregisterZmqEventSource();
        clearTimeout(connectTimer);
        clearTimeout(followTimer);
        qz_instance.SendGameCommand("team s");
      }
      else if (parseInt(val) > 0) {
        active = true;
        autoReconnect = true;
        connect(val - 1);
      }
      else if (val == "follow") {
        active = true;
        autoReconnect = false;
        registerZmqEventSource();
        following = null;
        followBestPlayerOnCurrentServer();
      }
    });
  }

  function showHelp() {
    echo("Use ^5qtv^7 ^3command^7(s) with these commands:");
    echo("^3follow^7         switches POV to the highest rated player on the current server");
    echo("^3duel^7,^3ffa^7,...   set gametype for auto-connect");
    echo("^3all^7,^3eu^7,^3na^7...   set region to all,eu,na,sa,au,as,af");
    echo("^31^7..^310^7          connect to + follow the n-th best match");
    echo("^30^7              stop automatic connecting and following");
    echo("^5qtv_announce ^30^7|^31^7  to turn announcements off/on when switching POV");
    echo("Multiple commands can be separated with comma, spaces are NOT allowed");
    echo("e.g.: ^2\\qtv duel,all,1^7");
  }

  function connect(matchRank) {
    log("connect(" + matchRank + ")");
    var region = qz_instance.GetCvar("qtv_region") || 0;
    gametype = qz_instance.GetCvar("qtv_gameType") || "duel";
    bestMatchRank = matchRank || 0;

    clearTimeout(connectTimer);
    clearTimeout(followTimer);

    $.getJSON("http://api.qlstats.net/api/nowplaying", { region: region }, function(data) {
        if (!data[gametype] || !data[gametype][0]) {
          echo("^1quakeTV:^7 couldn't get match data from qlstats.net. retrying in 30sec...");
          connectTimer = setTimeout(connect, 30 * 1000);
          return;
        }

        var bestMatches = data[gametype];
        var curAddr = qz_instance.GetCvar("cl_currentServerAddress") || "";
        if (qz_instance.GetCvar("ui_mainmenu") == "1")
          curAddr = "";

        // find a match with at least 2 players
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
          // can't find a good match, so stay on the current server and check again in 30sec
          bestAddr = curAddr;
          connectTimer = setTimeout(connect, 30 * 1000);
        }

        if (curAddr == bestAddr) {
          // if already connected to the best match, just ensure we follow the best player
          if (qz_instance.GetCvar("ui_mainmenu") == "0")
            followBestPlayerOnCurrentServer();
          return;
        }

        // connect to the new server
        following = null;
        connectTime = 0;
        mapStartTime = 0;
        clientIdCache = null;
        unregisterZmqEventSource();
        qz_instance.SendGameCommand("connect " + bestAddr);
        $.getJSON("http://localhost:27963/restartOBS", function() {
          log("^2quakeTv:^7 notified extraQL to restart OBS");
        });

        // give QL 15sec to connect to the server and raise the cvar.ui_mainmenu notification when the map was loaded
        clearTimeout(connectTimer);
        connectTimer = setTimeout(connectionTimedOut, 15 * 1000);
      })
      .fail(function(err) {
        echo("^1quakeTv:^7 failed to get list of top matches from qlstats.net: " + err);
        connectTimer = setTimeout(connect, 30 * 1000);
      });  
    
    function connectionTimedOut() {
      // happens when server is down or passworded
      if (autoReconnect)
        connect(bestMatchRank + 1);
    }
  }

  function unregisterZmqEventSource() {
    if (!eventSource)
      return;
    eventSource.removeEventListener('message', onZmqMessage);
    eventSource.close();
    eventSource = null;
  }

  function registerZmqEventSource() {
    if (eventSource)
      unregisterZmqEventSource();

    $.getJSON("http://api.qlstats.net/api/qtv/" + qz_instance.GetCvar("cl_currentServerAddress") + "/url", function(data) {
      if (data.ok && data.streamUrl) {
        eventSource = new window.EventSource(data.streamUrl);
        eventSource.addEventListener('message', onZmqMessage);
      }
    });
  }

  function onZmqMessage(e) {
    log(e.data);
    var event = JSON.parse(e.data);
    if (event.TYPE == "INIT") {
      gametype = event.GAME_TYPE || gametype || "ca";
      if ("ca,ft,ad".indexOf(gametype) >= 0) {
        event.PLAYERS.forEach(function(p) {
          if (p.DEAD)
            deadPlayers.push(p.STEAM_ID);
        });
      }
    }
    else if (event.TYPE == "MATCH_STARTED") {
      // QLDS resets the g_levelStartTime time when the match starts. We need to reset mapStartTime so there won't be a reconnect attempt
      mapStartTime = 0; 
    }
    else if (event.TYPE == "PLAYER_DEATH" && !event.WARMUP && "ca,ft,ad".indexOf(gametype) >= 0 || event.TYPE == "PLAYER_SWITCHTEAM" && event.TEAM == "SPECTATOR") {
      deadPlayers.push(event.STEAM_ID);
      if (event.STEAM_ID == following) {
        following = null;
        clearTimeout(followTimer);
        followTimer = setTimeout(followBestPlayerOnCurrentServer, 1000);
      }
    }
    else if (event.TYPE == "PLAYER_SWITCHTEAM")
      clientIdCache = null;
    else if (event.TYPE == "ROUND_OVER") {
      deadPlayers = [];
      clearTimeout(followTimer);
      followTimer = setTimeout(followBestPlayerOnCurrentServer, 5000);
    }
    else if (event.TYPE == "MATCH_REPORT") {
      deadPlayers = [];
      if (autoReconnect) {
        clearTimeout(connectTimer);
        connectTimer = setTimeout(connect, 10 * 1000);
      }
    }
  }

  function onUiMainMenu(data) {
    // When changing/reloading maps, ui_mainMenu gets set to 1 and back to 0.
    // This is our confirmation that a /connect attempt succeeded
    if (data.value == "0") {
      log("onUiMainMenu(0)");
      // stop connection timeout
      clearTimeout(connectTimer);
      connectTimer = null;
      connectTime = Date.now();
      mapStartTime = 0;

      if (active) {
        qz_instance.SendGameCommand("team s");
        following = null;
        followBestPlayerOnCurrentServer();

        if (!eventSource)
          registerZmqEventSource();
      }
    }
  }


  function followBestPlayerOnCurrentServer() {
    log("followBestPlayerOnCurrentServer(), ct=" + connectTime);
    if (!connectTime) // not connected -> can't follow
      return;

    if (followTimer)
      clearTimeout(followTimer);   

    $.getJSON("http://api.qlstats.net/api/server/" + qz_instance.GetCvar("cl_currentServerAddress") + "/players", function(data) {
      if (data.ok) {
        log("/players");
        //gametype = data.serverinfo.gt || gametype;
        serverBrowserData = data.players;
        serverBrowserData.sort(function(a, b) { return (b.rating || 0) - (a.rating || 0); });
        serverBrowserData = serverBrowserData.reduce(function(agg, p) {
          if (p.team >= 0 && p.team <= 2 && !p.quit && ("ca,ft,ad".indexOf(gametype) < 0 || deadPlayers.indexOf(p.steamid) < 0))
            agg.push(p);
          return agg;
        }, []);

        if (!mapStartTime)
          mapStartTime = data.serverinfo.mapstart;

        if (data.serverinfo.mapstart != mapStartTime) {
          // map_restart: speccing POV is lost
          following = null;
          mapStartTime = data.serverinfo.mapstart;
          if (autoReconnect) {
            log("autoReconnect");
            connect();
          }
          else {
            log("mapRestart");
            switchPov();
          }
        }
        else if (serverBrowserData.length < 2 && autoReconnect && new Date().getTime() > connectTime + 30 * 1000) {
          // connect to a new server when there are fewer than 2 players on the current server
          log("emptyServer");
          connect();
        }
        else { //if (serverBrowserData.length >= 1 && serverBrowserData[0].steamid != following) {
          // either a higher rated player joined, or the highest rated player left or moved to spec
          log("restoreBestPov");
          switchPov();
        }
      }
      else {
        log("quakeTV: can't get ratings for current server: " + data.msg);
      }
    });

    // safeguard timer to make sure we don't accidentally end up in free-float cam
    followTimer = setTimeout(followBestPlayerOnCurrentServer, 10 * 1000);

    function switchPov() {
      log("switchPov()");
      if (!clientIdCache)
        refreshClientIdCache(setNewPov);
      else
        setNewPov();

      function refreshClientIdCache(cb) {
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
              clientIdCache = playersById;
              cb();
            });
          }, 100);
        }, 1000);
      }

      function setNewPov() {
        var p = null;
        var name = null;
        for (var i = 0, c = serverBrowserData.length; i < c; i++) {
          p = clientIdCache[serverBrowserData[i].steamid];
          name = serverBrowserData[i].name;
          if (p && ("ca,ft,ad".indexOf(gametype) < 0 || deadPlayers.indexOf(p.steamid) < 0))
            break;
          p = null;
        }

        if (!p) {
          log("nobody to follow");
          // connect to a different server when nobody was found to spec
          if (autoReconnect && new Date().getTime() > connectTime + 30*1000)
            connect();
          return;
        }

        log("following clientid " + p.clientid);
        qz_instance.SendGameCommand("follow " + p.clientid);
        if (p.steamid != following) {         
          var cmd = qz_instance.GetCvar("qtv_announce") == "1" ? "say" : "echo";
          qz_instance.SendGameCommand(cmd + ' "^6QTV^7 is now following ' + (name || p.name).replace(/"/g, "'") + '"');
          following = p.steamid;
        }
      };
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

    if (qz_instance.GetCvar("qtv_region") == "")
      qz_instance.SetCvar("qtv_region", "0");
    if (qz_instance.GetCvar("qtv_gameType") == "")
      qz_instance.SetCvar("qtv_gameType", "duel");
    if (qz_instance.GetCvar("qtv_announce") == "")
      qz_instance.SetCvar("qtv_announce", "0");

    var qtv = qz_instance.GetCvar("qtv");
    if (qtv == "")
      qz_instance.SetCvar("qtv", HELP_MSG);
    else if (qtv != HELP_MSG)
      onQtvCommand({ value: qtv });

    // install QL event listeners
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("cvar.ui_mainmenu", onUiMainMenu);
    channel.subscribe("cvar.qtv", onQtvCommand);
    echo("^2quakeTv.js installed");
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