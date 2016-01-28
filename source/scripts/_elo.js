// ==UserScript==
// @name           QLStats: Adds an /elo command to show rating information from qlstats.net
// @version        1.1
// @author         PredatH0r
// @description    Use "/elo help" in the console to get a list of available commands.
// @description    /elo score: display rating for all players on the server
// @description    /elo games: display number of games played by each player
// @description    /elo shuffle: suggest best possible balanced teams
// @description    /elo shuffle!: arrange players into teams as suggested
// @enabled        0
// ==/UserScript==

/*

Version 1.1
- fixed help text

Version 1.0
- rewrite to work with Steam exclusive version of Quake Live

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;

  // constants
  var CVAR_elo = "elo";
  var HelpText = "a user script command. Use ^3elo help^7 to get some help.";
  var ConfigstringsMarker = "]\\configstrings";
  var GametypeMap = { 0: "ffa", 1: "duel", 2: "race", 3: "tdm", 4: "ca", 5: "ctf", 6: "1f", 8: "harv", 9: "ft" }

  // state variables
  var pendingEloRequest = null;
  var QLRD = {
    PLAYERS: {},
    OUTPUT: ["say", "echo", "print"],
    FORMAT: ["table", "simple", "list"],
    GAMETYPES: ["ffa", "duel", "tdm", "ctf", "ft"]
  };
  var PREFS = { method: "echo", format: "table", sort: "team", colors: "007" }
  PREFS.set = function (setting, value) { PREFS[setting] = value; }

  function init() {
    // create cvar
    qz_instance.SetCvar(CVAR_elo, HelpText);

    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("cvar.elo", onEloCommand);

    echo("^2elo.js installed");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    msg = msg.replace(/\"/g, "'").replace(/[\r\n]+/g, " ");
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onEloCommand(data) {
    var val = data.value;
    if (val == HelpText)
      return;
    qz_instance.SetCvar(CVAR_elo, HelpText);

    if (val == "help")
      showHelp();
    else if (val == "update")
      clearEloCache();
    else if (val == "score")
      showElo(PREFS.method, PREFS.format);
    else if (val == "say")
      showElo("say", PREFS.format);
    else if (val == "table" || val == "list" || val == "simple")
      showElo("echo", val);
    else if (RegExp("^method(=.*)?$").test(val))
      printOrSetPref("method", val.substr(7), QLRD.OUTPUT);
    else if (RegExp("^format(=.*)?$").test(val))
      printOrSetPref("format", val.substr(7), QLRD.FORMAT, "^5Note:^7 Formatting is only customizable for ^3method=echo^7. All other methods use ^3format=simple^7.");
    else if (RegExp("^sort(=.*)?$").test(val))
      printOrSetPref("sort", val.substr(5), QLRD.SORT);
    else if (RegExp("^colors(=.*)?$").test(val))
      printOrSetColors(val.substr(7));
    //else if (val.indexOf("profile=") == 0)
    //  showQlranksProfile(val.substr(8, val.length - 8));
    else if (val == "games")
      showGameCount();
    else if (val.indexOf("shuffle!") == 0)
      shuffle(true, val.substr(8));
    else if (val.indexOf("shuffle") == 0)
      shuffle(false, val.substr(7));
    else
      qz_instance.SendGameCommand("echo " + HelpText);
  }

 
  function showHelp() {
    qz_instance.SendGameCommand("echo Usage: ^5/" + CVAR_elo + "^7 <^3command^7>");
    qz_instance.SendGameCommand("echo \"^3say^7       shows the QLRanks score to all players\"");
    qz_instance.SendGameCommand("echo \"^3table^7|^3list^7|^3simple^7           ^^^7... formatted in your console\"");
    qz_instance.SendGameCommand("echo \"^3method^7=x  show/set output method\"");
    qz_instance.SendGameCommand("echo \"^3format^7=x  show/set output format used by method=echo\"");
    qz_instance.SendGameCommand("echo \"^3sort^7=x    show/set sorting\"");
    qz_instance.SendGameCommand("echo \"^3colors^7=x  show/set color digits for player,score,badge\"");
    qz_instance.SendGameCommand("echo \"^3score^7     shows the QLRanks score using above settings\"");
    qz_instance.SendGameCommand("echo \"^3games^7     shows the number of completed games for each player\"");
    qz_instance.SendGameCommand("echo \"^3shuffle^7!  suggest/arrange teams based on QLRanks score\"");
    qz_instance.SendGameCommand("echo \"          append ^3,+all,+player1,-player2^7 to add/remove players\"");
    //qz_instance.SendGameCommand("echo \"^3profile=^7x opens QLranks.com player profile in your browser\"");
    qz_instance.SendGameCommand("echo \"          ^3x^7 is a comma separated list of game types and players\"");
    qz_instance.SendGameCommand("echo \"^3update^7    clears cached Elo scores\"");
    //qz_instance.SendGameCommand("echo \"\"");
    //qz_instance.SendGameCommand("echo \"Badge letters after Elo score indicate number of games completed\"");
    //qz_instance.SendGameCommand("echo ^3A-J^7:  <100...<1000, ^3K-Y^7: <2000...<16000, ^3Z^7: >=16000");
  }

  function clearEloCache() {
    QLRD.PLAYERS = {};
    logMsg("Player data cache cleared");
  }

  function printOrSetPref(prefName, newValue, allowedValues, extraHelp) {
    if (newValue == undefined || newValue == "") {
      echo("Current ^3" + prefName + "^7 is: ^5" + PREFS[prefName] + "^7. Allowed: ^5" + allowedValues.join("^7,^5"));
      if (extraHelp)
        echo(extraHelp);
    }
    else if (allowedValues.indexOf(newValue) >= 0) {
      PREFS.set(prefName, newValue);
    }
    else
      echo("^1Invalid " + prefName + ".^7 Allowed: ^5" + allowedValues.join("^7,^5"));
  }

  function printOrSetColors(newValue) {
    function colorInfo(digit) { return (digit >= "1" && digit <= "7" ? "^" + digit : "^5") + digit; }

    if (newValue == undefined || newValue == "") {
      var colors = PREFS.colors;
      qz_instance.SendGameCommand("echo Current value is: " + colorInfo(colors[0]) + colorInfo(colors[1]) + colorInfo(colors[2]) + "^7. Allowed: 3 digits for ^5player name^7, ^5score^7, ^5badge^7");
      qz_instance.SendGameCommand("echo Valid digits are ^50^7=team color (^5player^7, ^1red^7, ^4blue^7, spec), ^11 ^22 ^33 ^44 ^55 ^66 ^77");
      qz_instance.SendGameCommand("echo Use ^5x^7 as 3rd digit to disable badge letters");
    }
    else if (RegExp("^[0-7][0-7][0-7Xx]$").test(newValue))
      PREFS.set("colors", newValue);
    else
      qz_instance.SendGameCommand("echo ^1Invalid value.^7 Use ^5/elo colors^7 for help");
  }

  function showElo() {
    if (pendingEloRequest) {
      echo("Please wait for the current request to complete...");
      return;
    }
    requestEloInformation(showEloInformation);

    function showEloInformation(request) {
      var players = [];
      for (var steamid in request) {
        if (!request.hasOwnProperty(steamid)) continue;
        var p = request[steamid];
        players.push({
          "steamid": p.steamid,
          "name": p.name,
          "team": p.team,
          "elo": p.elo,
          "badge": isNaN(p.games) ? "" : p.games >= 16000 ? "Z" : p.games < 1000 ? String.fromCharCode(65 + Math.floor(p.games / 100)) : String.fromCharCode(74 + Math.floor(p.games / 1000))
        });
      }

      // Sort players by either Elo or team
      var sortStyle = getSortStyle() || "team";
      players.sort(sortPlayerFunc(sortStyle));
      displayPlayers(players, "current", PREFS.method, PREFS.format);
    }
  }

  function showGameCount() {
    var currentOut = PREFS.method;

    // Refresh the current server's details
    requestEloInformation(function(request, matchInfo) {

      var games_played = [];
      for (var steamid in request) {
        if (request.hasOwnProperty(steamid)) {
          games_played.push(request[steamid]);
        }
      }
      games_played.sort(function(a, b) { return b.games - a.games; });

      var step = currentOut == "echo" || currentOut == "print" ? 100 : 1000;

      qz_instance.SendGameCommand(currentOut + " \"^5PLAYER_________   #" + GametypeMap[matchInfo.game_type].toUpperCase() + " games completed^7\"");
      for (var i = 0; i < games_played.length; i++) {
        (function(player, i) {
          window.setTimeout(function() {
            var color = player.games < 400 ? "^3" : player.games >= 10000 ? "^6" : "^2";
            qz_instance.SendGameCommand(currentOut + " \"^7" + pad(player.name, 15) + " " + color + pad(player.games, -6) + "^7\"");
          }, i * step);
        })(games_played[i], i);
      }
    });
  }
  
  function shuffle(doit, args) {
    // Give the request some time to complete
    if (pendingEloRequest) {
      echo("Please wait for the current request to complete...");
      return;
    }
  
    // Refresh the current server's details
    requestEloInformation(function (request) {
 
  
      // build list of players included in the shuffle
      args = "," + args + ",";
      var players = [];
      var addAll = args.indexOf(",+all,") >= 0;
      for (var steamid in request) {
        if (!request.hasOwnProperty(steamid)) continue;
        var p = request[steamid];
        var name = p.name.toLowerCase();
        if (args.indexOf(",-" + name + ",") >= 0)
          return;
        // skip specs by default, but allow overrides via comma separated +playername or +all
        if (addAll || args.indexOf(",+" + name + ",") >= 0 || p.team >= 1 && p.team <= 2) {
          p.oldTeam = p.team;
          p.elo = parseInt(p.elo);
          players.push(p);
        }
      }
  
      if (players.length % 2 != 0 || players.length < 2) {
        echo("Shuffle needs an even number of players, currently " + players.length);
        echo("Use ^3/elo shuffle,+all,+player1,-player2^7 to add/remove players");
        return;
      }
  
      var bestdiff = 1e8;
  
      function k_combinations(set, k) {
        var i, j, combs, head, tailcombs;
  
        if (k > set.length || k <= 0) {
          return [];
        }
  
        if (k == set.length) {
          return [set];
        }
  
        if (k == 1) {
          combs = [];
          for (i = 0; i < set.length; i++) {
            combs.push([set[i]]);
          }
          return combs;
        }
  
        combs = [];
        for (i = 0; i < set.length - k + 1; i++) {
          head = set.slice(i, i + 1);
          tailcombs = k_combinations(set.slice(i + 1), k - 1);
          for (j = 0; j < tailcombs.length; j++) {
            combs.push(head.concat(tailcombs[j]));
          }
        }
        return combs;
      }
  
      function range(start, end) {
        var array = new Array();
        for (var i = start; i < end; i++) {
          array.push(i);
        }
        return array;
      }
  
      var team_size = players.length / 2;
  
      ///ecs@060714 number of team permutations is n choose k where n is server size k teamsize
      var shuffle_perms = k_combinations(range(0, players.length), team_size);
  
      for (var i = 0; i < shuffle_perms.length; i++) {
        var reds = 0;
        var blues = 0;
  
        for (var p = 0; p < players.length; p++) {
          if (shuffle_perms[i].indexOf(p) != -1) {
            reds = reds + players[p].elo;
            players[p].candidateTeam = 1;
          } else {
            blues = blues + players[p].elo;
            players[p].candidateTeam = 2;
          }
        }
  
        ///average it
        var reds_avg = reds / team_size;
        var blues_avg = blues / team_size;
  
        var diff = Math.abs((reds_avg - blues_avg));
  
        if (diff < bestdiff) {
          bestdiff = diff;
          for (var p = 0; p < players.length; p++)
            players[p].team = players[p].candidateTeam;
        }
      }
  
      // swap teams if more people would be moved than stay
      var cntStay = 0, cntMove = 0;
      for (var i = 0; i < players.length; i++) {
        if (players[i].team == players[i].oldTeam)
          ++cntStay;
        else
          ++cntMove;
      }
      if (cntMove > cntStay) {
        for (var i = 0; i < players.length; i++)
          players[i].team = 3 - players[i].team; // 2=>1, 1=>2
      }
  
      // Sort players by team
      players = players.sort(sortPlayerFunc("team"));
  
      if (doit) {
        var commands = [];
  
        // find out where players should be moved to
        var move = [[], []]; // [to-red, to-blue]
        for (var i = 0; i < players.length; i++) {
          if (players[i].team != players[i].oldTeam)
            move[players[i].team - 1].push(i);
        }
  
        // move excessive blue players to red
        while (move[0].length > move[1].length) {
          var i = move[0][0];
          var command = "put " + players[i].clientid + " r";
          commands.push(command);
          move[0].splice(0, 1);
        }
  
        // move excessive red players to blue
        while (move[1].length > move[0].length) {
          var i = move[1][0];
          var command = "put " + players[i].clientid + " b";
          commands.push(command);
          move[1].splice(0, 1);
        }
  
        // red and blue are now equal size and can be exchanged in pairs
        for (var i = 0; i < move[0].length; i++) {
          var p = move[0][i];
          var command = "put " + players[p].clientid + " r";
          commands.push(command);
          extraQL.echo("^5" + command + "  ^2// " + players[p].name);
  
          var p = move[1][i];
          var command = "put " + players[p].clientid + " b";
          commands.push(command);
          extraQL.echo("^5" + command + "  ^2// " + players[p].name);
        }
  
        // send the put commands. needs at least 1sec delay due to server command flood protection
        var delay = 0;
        for (var i = 0; i < commands.length; i++) {
          (function (command, delay) {
            window.setTimeout(function () { qz_instance.SendGameCommand(command); }, delay);
          })(commands[i], delay);
          delay += 1100;
        }
  
        if (commands.length == 0)
          qz_instance.SendGameCommand("print ^2QLRD:^7 No shuffle required");
        else
          window.setTimeout(function () { qz_instance.SendGameCommand("say ^2QLRD:^7 Elo shuffle complete"); }, delay - 1000);
      }
  
      displayPlayers(players, doit ? "Arranging" : "Suggested");
    });
  }

  function requestEloInformation(callback) {
    qz_instance.SendGameCommand("echo " + ConfigstringsMarker); // text marker required by extraQL servlet
    qz_instance.SendGameCommand("configstrings");
    setTimeout(function () {
      qz_instance.SendGameCommand("condump extraql_condump.txt");
      setTimeout(function () {
        var xhttp = new XMLHttpRequest();
        xhttp.timeout = 1000;
        xhttp.onload = function () { onExtraQLServerInfo(xhttp, callback); }
        xhttp.onerror = function () {
          echo("^3extraQL.exe not running:^7");
          pendingEloRequest = null;
        }
        xhttp.open("GET", "http://localhost:27963/serverinfo", true);
        xhttp.send();
      }, 100);
    }, 1000);


    function onExtraQLServerInfo(xhttp, callback) {
      if (xhttp.status != 200) {
        pendingEloRequest = null;
        return;
      }

      pendingEloRequest = {}

      var json = JSON.parse(xhttp.responseText);
      var matchInfo = { game_type: json.gameinfo.g_gametype, players: [] };
      for (var i = 0; i < json.players.length; i++) {
        var obj = json.players[i];
        var player = { "steamid": obj.st, "name": obj.n.toLowerCase(), "team": obj.t, "clientid": obj.clientid };
        matchInfo.players.push(player);
        pendingEloRequest[player.steamid] = player;
      }

      var steamIds = Object.keys(pendingEloRequest);
      if (steamIds.length == 0) {
        pendingEloRequest = null;
        return;
      }
      var url = "http://qlstats.net:8080/elo/" + steamIds.join("+");
      var xhttp = new XMLHttpRequest();
      xhttp.timeout = 5000;
      xhttp.onload = function () { onQlstatsElo(xhttp, matchInfo, callback); }
      xhttp.onerror = function () { echo("^1elo.js:^7 could not get data from qlstats.net"); }
      xhttp.open("GET", url, true);
      xhttp.send();
    }

    function onQlstatsElo(xhttp, matchInfo, callback) {
      var request = pendingEloRequest;
      pendingEloRequest = null;

      if (xhttp.status != 200)
        return;

      var data = JSON.parse(xhttp.responseText);
      var dataBySteamId = {}
      for (var i = 0; i < data.players.length; i++)
        dataBySteamId[data.players[i].steamid] = data.players[i];

      var gametype = GametypeMap[matchInfo.game_type];
      for (var steamid in request) {
        if (!request.hasOwnProperty(steamid)) continue;
        var p = request[steamid];
        var data = dataBySteamId[steamid] || {};
        var gtData = data[gametype] || {};
        p.elo = gtData.elo || 1500;
        p.games = gtData.games || 0;
      }

      callback(request, matchInfo);
    }
  }

  /*
  function showQlranksProfile(playerString) {
    var server = quakelive.serverManager.GetServerInfo(quakelive.currentServerId);
    var gt = server ? mapdb.getGameTypeByID(server.game_type).name.toLowerCase() : "duel";
  
    var players = playerString.split(',');
    $.each(players, function (i, player) {
      if (",duel,ca,tdm,ffa,".indexOf("," + player + ",") >= 0)
        gt = player;
      else
        quakelive.OpenURL("http://www.qlranks.com/" + gt + "/player/" + player);
    });
  }
  */

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  // HELPER FUNCTIONS
  ////////////////////////////////////////////////////////////////////////////////////////////////////

  function getSortStyle() {
    var sortStyle = PREFS.sort;
    if (sortStyle != "elo" && sortStyle != "team")
      sortStyle = "";
    return sortStyle;
  }

  function sortPlayerFunc(sortStyle) {
    return function (player1, player2) {
      var key1 = sortCriteria(player1, sortStyle);
      var key2 = sortCriteria(player2, sortStyle);
      return key1 < key2 ? -1 : key1 > key2 ? +1 : 0;
    };
  }

  function sortCriteria(player, sortPref) {
    var score = player.elo;
    score = isNaN(parseInt(score)) ? "9999" : ("0000" + (10000 - score)).substr(-4);
    var isSpec = player.team == 3 ? "1" : "0";
    var crit = sortPref == "team" ? player.team + score + player.name : isSpec + score + player.name;
    return crit;
  }

  function displayPlayers(players, verb, method, format) {
    // Display the results.
    // NOTE: mul is "1" to separate the header from the results
    if (!method)
      method = PREFS.method;
    if (!format)
      format = PREFS.format;

    // Show the chat pane for 10 seconds if output method is 'print',
    // otherwise it will be difficult to notice.
    if (method == "print") {
      qz_instance.SendGameCommand("+chat;");
      window.setTimeout(function () {
        qz_instance.SendGameCommand("-chat;");
      }, 10000);
    }

    // display team stats
    var stats = calcStats(players);
    var hasTeams = stats.redcount > 0 || stats.blucount > 0;
    var avgInfo = hasTeams ?
      verb + " teams: " + stats.teamSummary + " ^7Avg: " + stats.allavg + "(" + stats.allcount + ")" :
      "^3Avg rating: " + stats.allavg + "(" + stats.allcount + ")";
    qz_instance.SendGameCommand(method + "\"" + avgInfo + "\"");

    // generate output lines
    if (!(method == "echo" || (method == "say" && format != "table")))
      format = "simple";
    var lines = format == "table" ? getTableLines(players, hasTeams, method) : getSequentialLines(players, format);

    // print output lines
    var mul = 1, step = method == "echo" || method == "print" ? 100 : 1000;
    lines.forEach(function (line) {
      window.setTimeout(function (txt) {
        qz_instance.SendGameCommand(method + " \"" + txt + "\";");
      }.bind(null, line), mul++ * step);
    });

    function getSequentialLines(players, format) {
      var colors = PREFS.colors;
      var prefixLine = colors[0] != "0" && colors[1] != "0";
      var prevTeam = players[0].team;
      var teamMemberCount = 0;
      var playersPerLine = format == "list" ? 1 : 4;
      var columnSep = format == "list" ? "" : "^7, ";
      var lines = [];
      for (var i = 0, out = [], len = players.length; i <= len; ++i) {
        var curTeam = i == len ? prevTeam : players[i].team;
        if (curTeam != prevTeam || i == len || (teamMemberCount && teamMemberCount % playersPerLine == 0)) {
          var linePrefix = prefixLine ? getTeamColor(prevTeam) + "PRBS"[prevTeam] + ": ^7" : "";
          var line = linePrefix + out.join(columnSep);
          lines.push(line);
          out = [];
        }
        if (i == len)
          break;
        teamMemberCount = curTeam == prevTeam ? teamMemberCount + 1 : 1;
        prevTeam = curTeam;

        var nameColor = colors[0] == "0" ? getTeamColor(curTeam) : "^" + colors[0];
        var scoreColor = colors[1] == "0" ? getTeamColor(curTeam) : "^" + colors[1];
        var badge = colors[2].toUpperCase() != "X" && players[i].badge ? "^" + colors[2] + players[i].badge : "";
        if (format == "list")
          out.push(nameColor + pad(players[i].name, 10).substr(0, 10) + " " + scoreColor + pad(players[i].elo, -4) + badge);
        else
          out.push(nameColor + players[i].name + " " + scoreColor + players[i].elo + badge);
      }
      return lines;
    }

    function getTableLines(players, hasTeams, method) {
      var playersColumns = [[], [], []];
      if (hasTeams) {
        players.forEach(function (p) {
          playersColumns[p.team == 1 || p.team == 2 ? p.team - 1 : 2].push(p);
        });
      } else {
        var playerCount = players.reduce(function (count, p) { return p.team == 0 ? count + 1 : count; }, 0);
        var nonplayerCount = players.length - playerCount;
        var playerSlots = Math.max(Math.floor((playerCount + 1) / 2), nonplayerCount);
        players.forEach(function (p) {
          playersColumns[p.team == 0 ? (playersColumns[0].length < playerSlots ? 0 : 1) : 2].push(p);
        });
      }

      var colors = PREFS.colors;
      var showBadge = colors[2].toLowerCase() != "x" ? 1 : 0;
      var nameWidth = method == "say" ? 12 : 14;
      var lines = [];
      for (var i = 0; i < 16; i++) {
        var line = "";
        var hasInfo = false;
        for (var c = 0; c < 3; c++) {
          var separator = c < 2 ? (method == "say" ? "^7|" : "^7 | ") : "";
          if (playersColumns[c].length > i) {
            hasInfo = true;
            var p = playersColumns[c][i];
            var teamColor = getTeamColor(p.team);
            var nameColor = colors[0] == "0" ? teamColor : "^" + colors[0];
            var scoreColor = colors[1] == "0" ? teamColor : "^" + colors[1];
            var badgeColor = colors[2] == "0" ? teamColor : "^" + colors[2];

            line += nameColor + pad(p.name, nameWidth).substr(0, nameWidth) + scoreColor + pad(p.elo, -4);
            if (showBadge)
              line += badgeColor + (p.badge || " ");
            line += separator;
          } else
            line += pad("", nameWidth + 4 + showBadge) + separator;
        }

        if (hasInfo)
          lines.push(line);
        else
          break;
      }
      return lines;
    }

  }

  function calcStats(players) {
    var counts = [{ count: 0, sum: 0 }, { count: 0, sum: 0 }, { count: 0, sum: 0 }, { count: 0, sum: 0 }];

    for (var i = 0; i < players.length; i++) {
      var player = players[i];
      if (!parseInt(player.elo))
        continue;
      counts[player.team].count++;
      counts[player.team].sum += player.elo;
      if (player.team == 1 || player.team == 2) {
        counts[0].count++;
        counts[0].sum += player.elo;
      }
    }

    var redavg = counts[1].count == 0 ? 0 : Math.round(counts[1].sum / counts[1].count);
    var bluavg = counts[2].count == 0 ? 0 : Math.round(counts[2].sum / counts[2].count);
    var gap = Math.abs(redavg - bluavg);
    var gapColor = redavg > bluavg ? "^1" : bluavg > redavg ? "^4" : "^2";

    var descr = "^1ragequit";
    if (gap < 300) descr = "^1unplayable";
    if (gap < 200) descr = "^6very unbalanced";
    if (gap < 150) descr = "^3unbalanced";
    if (gap < 100) descr = "^3challenging";
    if (gap < 80) descr = "^2balanced";
    if (gap < 40) descr = "^2very balanced";

    var teamSummary = redavg && bluavg ? " ^1" + redavg + "(" + counts[1].count + ") " + "^4" + bluavg + "(" + counts[2].count + ") ^7Gap: " + gapColor + gap + "  " + descr : "";
    return {
      allavg: counts[0].count == 0 ? 0 : Math.round(counts[0].sum / counts[0].count),
      allcount: counts[0].count,
      redavg: redavg,
      redcount: counts[1].count,
      bluavg: bluavg,
      blucount: counts[2].count,
      gap: gap,
      descr: descr,
      teamSummary: teamSummary
    }
  }

  function getTeamColor(team) {
    return team == 0 ? "^5" : team == 1 ? "^1" : team == 2 ? "^4" : "^7";
  }

  function pad(text, minLength, paddingChar) {
    if (text === undefined || text == null) text = "";
    text = text.toString();
    if (paddingChar === undefined || paddingChar == null || paddingChar == "") paddingChar = " ";
    if (minLength === undefined) minLength = 0;
    var padLeft = minLength < 0;
    minLength = Math.abs(minLength);

    var delta = 0;
    for (var i = 0; i < text.length; i++)
      if (text[i] == "^") delta++;
    while (text.length - 2 * delta < minLength)
      text = padLeft ? paddingChar + text : text + paddingChar;
    return text;
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

