// ==UserScript==
// @id          188803
// @name        AutoExec for game start/end (e.g. toggle fullscreen)
// @version     2.0
// @author      PredatH0r
// @description	Automates execution of commands when starting/ending game-mode or switching fullscreen
// @include     http://*.quakelive.com/*
// @exclude     http://*.quakelive.com/forum*
// @unwrap
// ==/UserScript==

/*

Version 2.0
- contains the automation features of the previous Alt Menu Script, but without the UI modifications

*/

(function (win) {
  var VERSION = "2.0";

  var window = win;
  var quakelive = win.quakelive;
  var $ = win.jQuery;
  var oldLaunchGame;

  function error(e) {
    window.console.log("ERROR - " + e);
  }

  function debug(msg) {
    window.console.log("DEBUG - " + msg);
  }

  function init() {
    try {
      addStyle(".qkTextOption { position: absolute; left: 110px; width: 410px }");

      HOOK_MANAGER.addMenuItem("Auto Exec...", showConsole);

      // install hooks
      quakelive.AddHook("OnGameModeStarted", OnGameModeStarted);
      quakelive.AddHook("OnGameStarted", OnGameStarted);
      quakelive.AddHook("OnGameModeEnded", OnGameModeEnded);
      oldLaunchGame = window.LaunchGame;
      window.LaunchGame = LaunchGame;
    }
    catch (e) { error(e); }
  }

  function addStyle() {
    var css = "";
    for (var i = 0; i < arguments.length; i++)
      css += "\n" + arguments[i];
    $("head").append("<style>" + css + "\n</style>");
  }

  function showConsole() {
    try {
      var out = [];
      out.push("<div id='qkConsole'>");
      out.push("<fieldset>");

      var mode = parseInt(quakelive.cvars.Get("r_autoFullscreen", "0").value);
      out.push("<b>Fullscreen / Windowed Mode</b>");
      out.push("<ul style='list-style-type: none; margin-top: 15px'>");
      out.push("<li><input type='checkbox' class='qkAutoFullscreen' value='1' " + (mode & 0x01 ? "checked" : "") + ">");
      out.push(" Go fullscreen when joining a game (r_autoFullscreen=+1)</li>");
      out.push("<li><input type='checkbox' class='qkAutoFullscreen' value='2' " + (mode & 0x02 ? "checked" : "") + ">");
      out.push(" Go window mode when leaving a game (r_autoFullscreen=+2)</li>");
      out.push("<li><input type='checkbox' class='qkAutoFullscreen' value='4' " + (mode & 0x04 ? "checked" : "") + ">");
      out.push(" Use fast mode switching (NOT working with some GPUs) (r_autoFullscreen=+4)</li>");
      out.push("</ul>");

      var gameStart = quakelive.cvars.Get("onGameStart", "").value;
      var gameEnd = quakelive.cvars.Get("onGameEnd", "").value;
      out.push("<br><br><b>Commands executed when entering / leaving a game</b>");
      out.push("<br>(e.g. com_maxfps 125;r_gamma 1)");
      out.push("<ul style='list-style-type: none; margin-top: 15px'>");
      out.push("<li style='height:22px'>onGameStart: <input type='text' class='qkTextOption' name='onGameStart' value='" + gameStart + "'>");
      out.push("<li style='height:22px^'>onGameEnd: <input type='text' class='qkTextOption' name='onGameEnd' value='" + gameEnd + "'>");
      out.push("</ul>");
      out.push("</fieldset>");
      out.push("</div>");

      // Inject the console
      qlPrompt({
        id: "qkPrompt",
        title: "Quake Live AutoExec" + " <small>(v" + VERSION + ")</small>",
        customWidth: 550,
        ok: handleConsoleOk,
        okLabel: "Ok",
        cancel: handleConsoleClose,
        cancelLabel: "Cancel",
        body: out.join("")
      });

      // Wait for the prompt to get inserted then do stuff...
      window.setTimeout(function () {
        $("#modal-cancel").focus();
      });
    }
    catch (e) {
      error(e);
      handleConsoleClose();
    }
  }

  function handleConsoleOk() {
    var val = 0;
    $("#qkConsole").find(".qkAutoFullscreen:checked").each(function (idx, item) {
      val += parseInt(item.value);
    });
    quakelive.cvars.Set("r_autoFullscreen", "" + val, true, false);

    $("#qkConsole").find(".qkTextOption").each(function (idx, item) {
      quakelive.cvars.Set(item.name, '"' + item.value + '"', true, false);
    });

    handleConsoleClose();
  }

  function handleConsoleClose() {
    $("#qkPrompt").jqmHide();
  }

  function LaunchGame(launchParams, serverInfo) {
    try {
      debug("LaunchGame() called");
      autoSwitchToFullscreen();
      oldLaunchGame.call(null, launchParams, serverInfo);
    }
    catch (e) { error(e); }
  }

  function OnGameModeStarted() {
    try {
      debug("OnGameModeStarted() called");
      autoSwitchToFullscreen(); // fallback, if starting directly via /connect and bypassing UI
    } catch (e) { error(e); }
  }

  function OnGameStarted() {
    try {
      debug("OnGameStarted() called");
      autoSwitchToFullscreen(); // fallback, if starting directly via /connect and bypassing UI
      qz_instance.SendGameCommand("vstr onGameStart");
    } catch (e) { error(e); }
  }

  function autoSwitchToFullscreen() {
    var auto = quakelive.cvars.Get("r_autoFullscreen").value;
    if (auto != "" && (parseInt(auto) & 0x01) && quakelive.cvars.Get("r_fullscreen").value != "1") {
      debug("auto switching to fullscreen");
      // use both JS and QZ to avoid timing issues and make sure the value sticks
      quakelive.cvars.Set("r_fullscreen", "1", false, false);
      var fast = (parseInt(auto) & 0x04) ? " fast" : "";
      qz_instance.SendGameCommand("seta r_fullscreen 1;vid_restart" + fast);
    }
  }

  function OnGameModeEnded() {
    try {
      debug("OnGameModeEnded() called");
      var auto = quakelive.cvars.Get("r_autoFullscreen").value;
      if (auto != "" && (parseInt(auto) & 0x02) && quakelive.cvars.Get("r_fullscreen").value != "0") {
        debug("auto switching to windowed mode");
        // use both JS and QZ to avoid timing issues and make sure the value sticks
        quakelive.cvars.Set("r_fullscreen", "0", false, false);
        var fast = (parseInt(auto) & 0x04) ? " fast" : "";
        qz_instance.SendGameCommand("seta r_fullscreen 0;vid_restart" + fast);
      }
      qz_instance.SendGameCommand("vstr onGameEnd");
    } catch (e) { error(e); }
  }


  init();

})(window);