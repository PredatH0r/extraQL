/*
This script scans the RACE leader board of all maps and downloads the match JSONs of each listed match.
To run it, set ENABLED=true and execute a /web_reload.
*/

(function () {
  var ENABLED = false;
  var CONCURRENT_MAPS = 1; // must stay 1
  var CONCURRENT_REQUESTS = 10;
  var extraQL = window.extraQL;

  var maps = [
    "basesiege", "beyondreality", "blackcathedral"
//    /*
    ,"bloodlust", "brimstoneabbey", "campercrossings", "campgrounds", "citycrossings", "courtyard", "deepinside", "distantscreams"
    ,"divineintermission", "doubleimpact", "duelingkeeps", "eviscerated", "falloutbunker", "finnegans", "fluorescent", "foolishlegacy", "futurecrossings", "gospelcrossings", "industrialaccident"
    ,"infinity", "innersanctums", "ironworks", "japanesecastles", "jumpwerkz", "overlord", "pillbox", "railyard", "rebound", "reflux", "repent", "qzpractice1", "scornforge", "shakennotstirred"
    ,"shiningforces", "siberia", "skyward", "spacectf", "spacechamber", "spidercrossings", "stonekeep", "qzpractice2", "stronghold", "theedge", "theatreofpain", "trinity", "troubledwaters", "warehouse"
//    */
  ];

  var gameIds = {}; // hashset with unique game IDs for current map
  var games = [];
  var gameIdList = "";

  var mapIndex;
  var gameIndex;

  function run() {
    mapIndex = 0;
    processNextMap();
  }

  function done() {
    extraQL.store("!ndex.json", "[" + gameIdList + "]");
    extraQL.log("^2done!^7");
  }

  function processNextMap() {
    ++mapIndex;
    if (mapIndex == maps.length + CONCURRENT_MAPS) {
      done();
      return;
    }
    if (mapIndex > maps.length) return;

    var map = maps[mapIndex - 1];
    requestMapData(map);
  }

  function requestMapData(map) {
    $.getJSON("/race/map/" + map,
        function(data) { processMapData(data, map); })
      .fail(function() { extraQL.log("^1can't get data for " + map + "^7"); });
  }

  function processMapData(data, map) {
    gameIds = {};
    games = [];
    gameIndex = 0;
    $.each(data.scores, function(i, item) {
      gameIds[item.guid] = map;
      if (gameIdList)
        gameIdList += ",";
      gameIdList += "{\"" + item.guid + "\":\"" + map + "\"}\n";
    });
    processGames(map);
  }

  function processGames(map) {
    $.each(gameIds, function(id) {
      games.push(id);
    });

    extraQL.log("^2" + map + "^7 (" + mapIndex + "/" + maps.length + "): " + games.length + " matches");

    for (var i=0; i<CONCURRENT_REQUESTS; i++)
      processNextGame(map);
  }

  function processNextGame(map) {
    ++gameIndex;
    if (gameIndex == games.length + CONCURRENT_REQUESTS) {
      processNextMap();
      return;
    }
    if (gameIndex > games.length) return;

    if (gameIndex % 10 == 0) {
      extraQL.log("  #" + gameIndex);
    }

    var gameId = games[gameIndex - 1];
    $.ajax({
      url: "/stats/matchdetails/" + gameId,
      dataType: "html",
      success: function(data) { extraQL.store(gameId + ".json", data); },
      error: function() { extraQL.log("^1can't get data for " + gameId + "^7"); },
      complete: function() { processNextGame(map); }
    });
  }

  if (ENABLED) {
    if (window.extraQL)
      run();
    else {
      $.getScript("http://beham.biz/ql/extraQL.js", run);
    }
  }
})();