// ==UserScript==
// @id             joinGame
// @name           Join Game Through HTTP Link
// @version        1.0
// @author         PredatH0r
// @description    Joins a game through a link of the format http://127.0.0.1:27963/join/91.198.152.211:27003/passwd
// @unwrap
// ==/UserScript==

(function () {
  var hInterval;

  function init() {
    if (!extraQL || !extraQL.isLocalServerRunning())
      return;

    quakelive.AddHook("OnGameModeEnded", startPolling);
    startPolling();
  }

  function startPolling() {
    hInterval = window.setInterval(pollJoinInformation, 1000);
  }

  function pollJoinInformation() {
    $.getJSON(extraQL.BASE_URL + "join", function(info) {
      if (info.server) {
        window.clearTimeout(hInterval);
        if (info.pass)
          quakelive.cvars.Set("password", info.pass);
        window.join_server(info.server, undefined, info.pass);
      }
    });
  }

  init();
})();