// ==UserScript==
// @name        Quake Live Twich.tv Streams and VODs
// @version     1.0
// @author      PredatH0r
// @downloadUrl https://raw.githubusercontent.com/PredatH0r/extraQL/master/scripts/11_twitch.usr.js
// @description	Shows a list of twitch.tv QL live streams and videos
// @include     http://*.quakelive.com/*
// @exclude     http://*.quakelive.com/forum*
// @unwrap
// ==/UserScript==

/*

Version 1.0
- first public release

*/

(function () {
  // external variables
  var quakelive = window.quakelive;
  var extraQL = window.extraQL;

  // config
  var games = ["Quake Live", "Quake II", "Quake"];
  var videoChannels = ["faceittv", "zlive", "leveluptv", "tastyspleentv"];

  // constants
  var URL_STREAMS = "https://api.twitch.tv/kraken/streams?limit=50&game={0}";
  var URL_VIDEOS = "https://api.twitch.tv/kraken/channels/{0}/videos?limit=50&broadcasts={1}";
  var UPDATE_INTERVAL = 60000;

  var VIEW_STREAMS = "streams";
  var VIEW_CASTS = "casts";
  var VIEW_VIDEOS = "videos";
  var currentView = VIEW_STREAMS;

  function init() {
    // delay init so that twitch, twitter, ESR and IRC scripts add items to chat menu bar in a defined order
    setTimeout(delayedInit, 0);
  }

  function delayedInit() {
    onContentLoaded();
    quakelive.AddHook("OnContentLoaded", onContentLoaded);
    quakelive.AddHook("OnGameModeEnded", updateStreams);
  }

  function onContentLoaded() {
    if ($("#twitch").length)
      return;

    var fixedElementsHeight = 277;

    extraQL.addStyle(
      "#twitch { width: 300px; color: black; background-color: white; display: none; }",
      "#twitchHeader { border-bottom: 1px solid #e8e8e8; padding: 9px 9px 8px 9px; }",
      "#twitchHeader .headerText { font-size: 14px; line-height: 18px; font-weight: bold; }",
      "#twitchHeader a { color: black; font-size: 14px; }",
      "#twitchHeader a.active { color: #A0220B; }",
      "#twitchDetails { padding: 6px 6px; border-bottom: 1px solid #e8e8e8; }",
      "#twitchStatus { height: 45px; overflow: hidden; margin-top: 3px; }",
      "#twitchContent { height: " + (550 - fixedElementsHeight) + "px; overflow: auto; }",
      "#twitchContent p { font-weight: bold; font-size: 12pt; background-color: #444; color: white; padding: 2px 6px; margin-top: 3px; overflow: hidden; }",
      "#twitchContent div { padding: 3px 6px; max-height: 14px; overflow: hidden; }",
      "#twitchContent .active { background-color: #ccc; }",
      "#twitchContent a { color: black; text-decoration: none; }",
      "#twitchContent a:hover { text-decoration: underline; }"
      );

    var content =
      "<div id='twitch' class='chatBox tabPage'><div>" +
      "  <div id='twitchHeader'><span class='headerText'>Twitch.tv " +
      "<a href='javascript:void(0)' id='twitchShowStreams' class='active'>live streams</a> | " +
      "<a href='javascript:void(0)' id='twitchShowCasts'>archive</a> | " +
      "<a href='javascript:void(0)' id='twitchShowVideos'>vods</a>" +
      "</span></div>" +
      "  <div id='twitchDetails'><img src='' width='288' height='180'><div id='twitchStatus'></div></div>" +
      "  <div id='twitchContent' data-fill='" + fixedElementsHeight + "'></div>" +
      "</div></div>";
    extraQL.addTabPage("twitch", "Twitch", content);

    $("#twitchShowStreams").click(function () {
      currentView = VIEW_STREAMS;
      $("#twitchHeader a").removeClass("active");
      $(this).addClass("active");
      updateStreams();
    });

    $("#twitchShowCasts").click(function () {
      currentView = VIEW_CASTS;
      $("#twitchHeader a").removeClass("active");
      $(this).addClass("active");
      updateVideos(true);
    });

    $("#twitchShowVideos").click(function() {
      currentView = VIEW_VIDEOS;
      $("#twitchHeader a").removeClass("active");
      $(this).addClass("active");
      updateVideos(false);
    });

    updateStreams();
  }

  /*********************************************************************/
  // streams
  /*********************************************************************/

  var GAME_THREADS = 4;
  var gameIndex;
  var gameStreams;
  var streamCount;
  
  function updateStreams() {
    if (quakelive.IsGameRunning())
      return;

    gameIndex = 0;
    gameStreams = {};
    streamCount = 0;
    if (currentView == VIEW_STREAMS)
      showLoadingScreen();
    for (var i=0; i<GAME_THREADS; i++)
      loadStreamsForNextGame();
  }

  function loadStreamsForNextGame() {
    ++gameIndex;
    if (gameIndex == games.length + GAME_THREADS) {
      // last thread completed
      displayStreams();
      return;
    }
    if (gameIndex > games.length) return;

    var game = games[gameIndex - 1];
    //extraQL.log("Loading streams for ^3" + game + "^7");
    $.ajax({
      url: extraQL.format(URL_STREAMS, encodeURIComponent(game)),
      dataType: "jsonp",
      jsonp: "callback",
      success: parseStreamsForGame,
      error: function() { extraQL.log("^Failed^7 to load twitch streams for " + game); },
      complete: loadStreamsForNextGame
    });
  }

  function parseStreamsForGame(data) {
    $.each(data.streams, function (i, stream) {
      if (!gameStreams[stream.game])
        gameStreams[stream.game] = [];
      gameStreams[stream.game].push(stream);
    });
    streamCount += data.streams.length;
  }

  function displayStreams() {
    try {
      // update tab caption
      if (streamCount == 0)
        $("#tab_twitch").html("Twitch");
      else
        $("#tab_twitch").html("Twitch (" + streamCount + ")");

      if (currentView != VIEW_STREAMS)
        return;

      var $streams = $("#twitchContent");
      $streams.empty();

      // iterate streams in defined order
      $.each(games, function (i, game) {
        var streams = gameStreams[game];
        if (!streams || streams.length == 0) return;
        $streams.append("<p>" + game + "</p>");

        // update stream list
        $.each(streams, function(j, item) {
          $streams.append("<div" +
            " data-preview='" + item.preview.medium + "'" +
            " data-status=\"" + extraQL.escapeHtml(item.channel.status) + "\"" +
            ">" +
            "<a href='" + item.channel.url + "' target='_blank'>" +
            extraQL.escapeHtml(item.channel.display_name) + " (" + item.viewers + ")</a></div>");
        });
      });

      $("#twitchContent div").hover(showStreamDetails);
      showDetailsForFirstEntry();

      window.setTimeout(updateStreams, UPDATE_INTERVAL);
    } catch (e) {
      extraQL.log(e);
    }
  }

  function showLoadingScreen() {
    var $streams = $("#twitchContent");
    $("#twitchDetails>img").attr("src", "");
    $streams.empty().append("<div>Loading...</div>");
  }

  function showDetailsForFirstEntry() {
    var divs = $("#twitchContent>div");
    if (divs.length == 0) {
      $("#twitchDetails img").attr("src", extraQL.BASE_URL+"images/offline.jpg");
      $("#twitchStatus").text("Offline");
      $("#twitchContent").html("<div>No live streams found.</div>");
    } else {
      showStreamDetails.apply(divs[0]);
      $(divs[0]).addClass("active");
    }
  }

  function showStreamDetails() {
    var $this = $(this);
    $("#twitchDetails img").attr("src", $this.data("preview"));
    $("#twitchStatus").html($this.data("status"));
    $("#twitchContent div").removeClass("active");
    $this.addClass("active");
  }

  /*********************************************************************/
  // videos
  /*********************************************************************/

  var VIDEO_THREADS = 3;
  var videoChannelIndex;
  var videos;
  var loadCasts;

  function updateVideos(casts) {
    videoChannelIndex = 0;
    videos = [];
    loadCasts = casts;
    showLoadingScreen();
    for (var threads = 0; threads < VIDEO_THREADS; threads++)
      loadVideosForNextChannel();
  }

  function loadVideosForNextChannel() {
    ++videoChannelIndex;
    if (videoChannelIndex == videoChannels.length + VIDEO_THREADS) {
      // last thread is done
      sortAndDisplayVideos();
      return;
    }
    if (videoChannelIndex > videoChannels.length) {
      // some thread is done
      return;
    }

    var channel = videoChannels[videoChannelIndex - 1];
    //extraQL.log("Loading videos for channel ^3" + channel + "^7");
    $.ajax({
        url: extraQL.format(URL_VIDEOS, channel, loadCasts),
        dataType: "jsonp",
        jsonp: "callback",
        success: parseVideosFromChannel,
        error: function () { extraQL.log("^1Failed^7 to load twitch video list for channel " + channel); },
        complete: loadVideosForNextChannel
      });
  }

  function parseVideosFromChannel(data) {
    $.each(data.videos, function (i, video) {
      if (games.indexOf(video.game) < 0) return; // ignore videos of unsubscribed games
      if (loadCasts && videoChannels.indexOf(video.channel.name) < 0) return;  // ignore past casts from non-featured channels
      videos.push(video);
    });
  }

  function sortAndDisplayVideos() {
    try {
      var $streams = $("#twitchContent");
      $streams.empty();
      if (videos.length == 0) {
        $streams.append("<div>No videos found</div>");
        return;
      }

      videos.sort(function(a, b) { return -(a.recorded_at < b.recorded_at ? -1 : a.recorded_at > b.recorded_at ? +1 : 0); });

      // update video list
      $.each(videos, function (i, item) {
        if (i > 100) return;
        var date = new Date(item.recorded_at);
        var vidDate = (1900 + date.getYear()) + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
        var hours = item.length / 3600;
        hours = hours < 1 ? "" : Math.floor(hours) + "h";
        var vidLength = " " + hours + ("0" + Math.round(item.length / 60 % 60)).slice(-2) + "m" + " - ";
        var channel = item.channel ? (item.channel.display_name ? item.channel.display_name : item.channel.name) : "";
        //var descr = item.description && item.description != item.title ? extraQL.escapeHtml(item.description) + "&lt;br&gt;" : "";
        $streams.append("<div" +
          " data-preview='" + item.preview + "'" +
          " data-status=\"[" + vidDate + "] " + vidLength + " &lt;b&gt;" + extraQL.escapeHtml(channel) + "&lt;/b&gt;&lt;br&gt;"+ extraQL.escapeHtml(item.title) + "\"" +
          ">" +
          "<a href='" + item.url + "' target='_blank'>" + extraQL.escapeHtml(item.title) + "</a></div>");
      });
      $("#twitchContent div").hover(showStreamDetails);

      showDetailsForFirstEntry();
    } catch (e) {
      extraQL.log(e);
    }
  }


  if (extraQL)
    init();
  else
    $.getScript("http://beham.biz/ql/extraQL.js", init);
})();