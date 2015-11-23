/*
// @name        extraQL
// @description extraQL userscript tool library
// @author      PredatH0r
// @version     1.4

This script provides common functions to various Quake Live user scripts.

In bundle with extraQL.exe and the modified version of QLHM/hook.js, this
script also acts as the boot strapper to load the locally installed scripts.

Version 1.4
- added error() method

Version 1.3
- fixed echo() when text contains backslashes
- added debug() method

Version 1.2
- un-merged extraQL.js from hook.js

Version 0.102
- added support for distributed client/server setup

*/
(function () {
  var VERSION = "1.3";
  var BASE_URL = "http://127.0.0.1:27963/";
  var tabClickHandlers = {};
  var tabPagePriority = [0];
  var chatBarTabified = false;
  var lastServerCheckTimestamp = 0;
  var lastServerCheckResult = false;

  function init() {
    addStyle("#chatContainer .fullHeight { height:550px; }");
  }

  // public: add CSS rules
  // params: string... or string[]
  function addStyle( /*...*/) {
    var css = "";
    var args = arguments.length == 1 && Array.isArray(arguments[0]) ? arguments[0] : arguments; 
    for (var i = 0; i < args.length; i++)
      css += "\n" + args[i];
    $("head").append("<style>" + css + "\n</style>");
  }

  // public: write a message to the QL console or the in-game chat
  function log(msg) {
    if (msg instanceof Error && msg.fileName)
      msg = msg.fileName + "," + msg.lineNumber + ": " + msg.name + ": " + msg.message;
    console.log(msg);
  }

  function debug(msg) {
    var enabled = quakelive.cvars.Get("eql_debug");
    if (!enabled || enabled.value == "0")
      return;
    if (msg instanceof Error && msg.fileName)
      msg = msg.fileName + "," + msg.lineNumber + ": " + msg.name + ": " + msg.message;
    msg = "^6" + msg;
    if (quakelive.IsGameRunning())
      qz_instance.SendGameCommand("echo \"" + msg.replace('"', "'").replace("\\", "\\\\") + "\"");
    else
      console.log(msg);
  }

  function error(msg) {
    log("^1" + msg);
  }

  function echo(text) {
    qz_instance.SendGameCommand("echo \"" + text.replace('"', "'").replace("\\", "\\\\") + "\"");
  }

  // public: escape special HTML characters in the provided string
  function escapeHtml(text) {
    // originally from mustache.js MIT ( https://raw.github.com/janl/mustache.js/master/LICENSE )
    var entityMap = { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;", "/": "&#x2F;" };
    return String(text).replace(/[&<>"'\/]/g, function(s) { return entityMap[s]; });
    //return $("<div/>").text(text).html();
  }


  var REGEX_FORMAT = new RegExp("(^|[^{])((?:{{)*){([0-9]+)}", "g");

  // public: .NET-like string.Format() which allows positional placeholders like {0} ... to be replaced with parameter values
  function format(template /*, ... */) {
    var args = arguments;
    return template.replace(REGEX_FORMAT, function(item, p1, p2, p3) {
      var intVal = parseInt(p3);
      var replace = intVal >= 0 ? args[1 + intVal] : "";
      return p1 + p2 + replace;
    }).replace("{{", "{").replace("}}", "}");
  };

  // public: add a tab page to the chat bar/area
  function addTabPage(id, caption, content, onClick, priority) {
    tabifyChat();

    if (!onClick)
      onClick = function() { showTabPage(id); };
    if (content)
      $($("#chatContainer").prepend(content).children()[0]).prepend("<div class='chatTitleBar'>" + caption + "<div class='close'>X</div></div>");

    var index;
    if (priority === undefined)
      priority = 999999;
    for (index = 1; index < tabPagePriority.length && tabPagePriority[index] <= priority; index++) {
    }
    tabPagePriority.splice(index, 0, priority);
    $($("#collapsableChat").children()[index - 1]).after("<div class='tab' id='tab_" + id + "'>" + caption + "</div>");
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
    if (!chatBarTabified) {
      if (contentId == "qlv_chatControl")
        $("#chatContainer").addClass("expanded");
      return;
    }

    var $cc = $("#chatContainer");
    var $popup = $("#" + contentId);
    if ($cc.hasClass("expanded") && $popup.hasClass("active"))
      closeTabPage();
    else {
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

    $("#tab_qlv_chatControl").unbind("click").click(function() {
      showTabPage("qlv_chatControl");
      quakelive.mod_friends.UI_ClearMessageAlert();
    });
    for (var id in tabClickHandlers)
      $("#tab_" + id).unbind("click").click(tabClickHandlers[id]);

    $("#chatContainer .chatTitleBar").unbind("click").click(closeTabPage);
  }

  //
  // functions to communicate with extraQL server
  //

  // public: test if the local extraQL HTTP server is running
  function isLocalServerRunning() {
    if (!extraQL.BASE_URL || !(extraQL.BASE_URL.indexOf("://127.0.0.1:")>=0 || extraQL.BASE_URL.indexOf("://localhost:")>=0))
      return false;
    if (new Date().getTime() - lastServerCheckTimestamp < 5000)
      return lastServerCheckResult;
    $.ajax({
      url: extraQL.BASE_URL + "version",
      async: false,
      dataType: "json",
      success: function(version) {
        lastServerCheckResult = true;
        extraQL.serverVersion = version;
      },
      error: function() { lastServerCheckResult = false; }
    });
    lastServerCheckTimestamp = new Date().getTime();
    return lastServerCheckResult;
  }


  // public: write a message the the extraQL.exe HTTP server log window
  function rlog(text) {
    if (isLocalServerRunning())
      $.post(BASE_URL + "log", text);
  }

  // public: store a text file in extraQL.exe's "data" directory
  function store(filename, text) {
    if (isLocalServerRunning())
      $.post(BASE_URL + "data/" + filename, text);
  }

  // public: load a text file from extraQL.exe's "data" directory
  function load(filename, callback) {
    if (isLocalServerRunning())
      $.get(BASE_URL + "data/" + filename, callback, "text");
    else
      callback();
  }


  init();

  // export public functions
  window.extraQL = {
    hookVersion: HOOK_MANAGER.version,
    libVersion: VERSION,
    BASE_URL: BASE_URL,
    isLocalServerRunning: isLocalServerRunning,
    log: log,
    rlog: rlog,
    echo: echo,
    debug: debug,
    error: error,
    addStyle: addStyle,
    escapeHtml: escapeHtml,
    store: store,
    load: load,
    format: format,

    addTabPage: addTabPage,
    showTabPage: showTabPage,
    closeTabPage: closeTabPage
  };
})();