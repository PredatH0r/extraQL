/*
// @name        extraQL
// @description Quake Live userscript utilities
// @author      PredatH0r
// @version     0.102

This script provides common functions to various Quake Live user scripts.

In bundle with extraQL.exe and the modified version of QLHM/hook.js, this
script also acts as the boot strapper to load the locally installed scripts.

Version 0.102
- added support for distributed client/server setup

*/

(function () {
  // external global variables
  var quakelive = window.quakelive;
  var console = window.console;

  var BASE_URL = "http://127.0.0.1:27963/";
  var tabClickHandlers = {};
  var chatBarTabified = false;
  var lastServerCheckTimestamp = 0;
  var lastServerCheckResult = false;

  function init() {
    addStyle("#chatContainer .fullHeight { height:550px; }");
  }

  // public: test if the local extraQL HTTP server is running
  function isLocalServerRunning() {
    if (new Date().getTime() - lastServerCheckTimestamp < 5000)
      return lastServerCheckResult;
    $.ajax({
      url: "http://127.0.0.1:27963/version",
      async: false,
      dataType: "json",
      success: function (version) {
        lastServerCheckResult = true;
        extraQL.serverVersion = version;
      },
      error: function() { lastServerCheckResult = false; }
    });
    lastServerCheckTimestamp = new Date().getTime();
    return lastServerCheckResult;
  }

  // public: add CSS rules
  // params: string...
  function addStyle(/*...*/) {
    var css = "";
    for (var i = 0; i < arguments.length; i++)
      css += "\n" + arguments[i];
    $("head").append("<style>" + css + "\n</style>");
  }

  // public: add a tab page to the chat bar/area
  function addTabPage(id, caption, content, onClick) {
    tabifyChat();

    if (!onClick)
      onClick = function() { showTabPage(id); };
    //$("#chatContainer").append(content);
    if (content)
      $($("#chatContainer").prepend(content).children()[0]).prepend("<div class='chatTitleBar'>" + caption + "<div class='close'>X</div></div>");
    $("#collapsableChat").append("<div class='tab' id='tab_" + id + "'>" + caption + "</div>");
    tabClickHandlers[id] = onClick;

    restoreTabPageClickHandlers();
  }

  // private: helper function
  function tabifyChat() {
    if (chatBarTabified)
      return;

    addStyle(
      "#chatContainer.expanded > .active { display: block; }",
      "#chatContainer .chatTitleBar { background-color: #444; color: white; width: 280px; height: 14px; font-size: 11px; padding: 0 10px; cursor: pointer; }",
      "#chatContainer .chatTitleBar .close { display: inline-block; font-size: 11px; float: right; }",
      "#collapsableChat { height: 28px; padding: 0px 6px !important; background-color: #721808; cursor: pointer; }", // + 2*6px border-top/-bottom
      "#collapsableChat div { display: inline-block; padding: 6px 10px; }",
      "#collapsableChat .tab:hover { background-color: #A0220B; }",
      "#collapsableChat .tab.active { background-color: #A0220B; padding: 6px 9px; border: 1px solid white; border-bottom: none; }",
      "#collapsableChat.bottomDockBar .tab.active { border-top: none; border-bottom: 1px solid white; }",
      "#collapsableChat.bottomDockBar { position: fixed; bottom: 0; right: 0px; width: 294px; background-color: rgba(114, 24, 8, 0.80); z-index: 2; }",
      "#collapsableChat.bottomDockBar.active { background-color: #B5260D; }"
    );
    if ($("#chatContainer").length == 0) {
      addStyle(
        "#chatContainer { position: fixed; bottom: 0px; right: 0px; width: 306px; z-index: 1; }",
        "#chatContainer.expanded { border-left: 3px solid #444; border-top: 3px solid #444; border-right: 3px solid #444; margin: 0px -3px; }"
      );
      $("body").append("<div id='chatContainer' class='ingame_only'><div id='collapsableChat'></div></div>");
    }
    else
      $("#collapsableChat").empty().append("<div class='tab' id='tab_qlv_chatControl'>Chat</div>");

    $("#qlv_chatControl").prepend("<div class='chatTitleBar'>Chat<div class='close'>X</div></div>");

    quakelive.mod_friends.roster.UI_OnRosterUpdated = function() {
      var numOnline = this.GetNumOnlineContacts();
      $('#tab_qlv_chatControl').text('Chat (' + numOnline + ')');
      this.UI_Show();
    }.bind(quakelive.mod_friends.roster);
    quakelive.mod_friends.UI_SetChatTitle = function() {
      var numOnline = this.roster.GetNumOnlineContacts();
      $('#tab_qlv_chatControl').text('Chat (' + numOnline + ')');
    }.bind(quakelive.mod_friends);  

    chatBarTabified = true;
  }

  // public: show a tab page
  function showTabPage(contentId) {
    var $cc = $("#chatContainer");
    var $popup = $("#" + contentId);
    if ($cc.hasClass("expanded") && $popup.hasClass("active"))
      closeTabPage();
    else {
      if (!extraQL.isOldUi)
        $("#qlv_chatControl").css("display", contentId == "qlv_chatControl" ? "" : "none"); // hack for default chat
      $popup.addClass("active");
      $cc.children().not("#collapsableChat").not($popup).removeClass("active");
      $cc.addClass("expanded");

      var $tab = $("#tab_" + contentId);
      $("#collapsableChat .tab").not($tab).removeClass("active");
      $tab.addClass("active");
    }
    event.stopPropagation();
  }

  // public: closes any open tab page
  function closeTabPage() {
    $("#chatContainer").removeClass("expanded");
    $("#collapsableChat .tab").removeClass("active");
  }

  // private: helper function
  function restoreTabPageClickHandlers() {
    $("#collapsableChat").unbind("click").click(closeTabPage);
    $("#collapsableChat .tab").unbind("click");

    $("#tab_qlv_chatControl").unbind("click").click(function () {
      showTabPage("qlv_chatControl");
      quakelive.mod_friends.UI_ClearMessageAlert();
    });
    for(var id in tabClickHandlers)
      $("#tab_" + id).unbind("click").click(tabClickHandlers[id]);

    $("#chatContainer .chatTitleBar").unbind("click").click(closeTabPage);
  }

  // public: write a message to the QL console or the in-game chat
  function log(msg) {
    if (msg instanceof Error && msg.fileName)
      msg = msg.fileName + "," + msg.lineNumber + ": " + msg.name + ": " + msg.message;
    if (quakelive.IsGameRunning())
      qz_instance.SendGameCommand("echo \"" + msg.replace('"', "'") + "\"");
    else
      console.log(msg);
  }

  // public: write a message the the extraQL.exe HTTP server log window
  function rlog(text) {
    $.post(BASE_URL + "log", text);
  }

  function echo(text) {
    qz_instance.SendGameCommand("echo \"" + text.replace("\"", "'") + "\"");
  }

  // public: escape special HTML characters in the provided string
  function escapeHtml(text) {
    // originally from mustache.js MIT ( https://raw.github.com/janl/mustache.js/master/LICENSE )
    var entityMap = { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;", "/": "&#x2F;" };
    return String(text).replace(/[&<>"'\/]/g, function (s) { return entityMap[s]; });
    //return $("<div/>").text(text).html();
  }

  // public: store a text file in extraQL.exe's "data" directory
  function store(filename, text) {
    $.post(BASE_URL + "data/" + filename, text);
  }

  // public: load a text file from extraQL.exe's "data" directory
  function load(filename, callback) {
    $.get(BASE_URL + "data/" + filename, callback, "html");
  }

  var REGEX_FORMAT = new RegExp("(^|[^{])((?:{{)*){([0-9]+)}", "g");

  // public: .NET-like string.Format() which allows positional placeholders like {0} ... to be replaced with parameter values
  function format(template /*, ... */) {
    var args = arguments;
    return template.replace(REGEX_FORMAT, function (item,p1,p2,p3) {
      var intVal = parseInt(p3);
      var replace = intVal >= 0 ? args[1+intVal] : "";
      return p1+p2+replace;
    }).replace("{{", "{").replace("}}", "}");
  };

  init();

  // export public functions
  window.extraQL = {
    BASE_URL: BASE_URL,
    isLocalServerRunning: isLocalServerRunning,
    log: log,
    rlog: rlog,
    echo: echo,
    addStyle: addStyle,
    escapeHtml: escapeHtml,
    store: store,
    load: load,
    format: format,

    isOldUi: $("#chatContainer").length == 0,
    addTabPage: addTabPage,
    showTabPage: showTabPage,
    closeTabPage: closeTabPage
  };
})();