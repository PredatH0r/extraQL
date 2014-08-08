/*
// @name        extraQL Script Manager
// @version     0.106
// @author      PredatH0r
// @credits     wn
// @description	Manages the installation and execution of QuakeLive userscripts

This script is a stripped down version of wn's QuakeLive Hook Manager (QLHM),
which is designed to work with a local or remote extraQL.exe script server.

Version 0.106
- added offline support with local script cache (HTML5 local storage)
- added hook.js version check against sourceforge
- merged extraQL.js into hook.js

Version 0.105
- sorts menu items under "Userscripts" alphabetically

Version 0.104
- fixed: on 2nd run all scripts were deactivated again

Version 0.103
- fixed async loading and execution of scripts without @unwrap

Version 0.102
- fallback to remote script server when there is no local server running

*/

// called in ql.Init
function main_hook() {
  if (quakelive.mod_legals === quakelive.activeModule)
    return;

  ExtraQL();
  HOOK_MANAGER.init();
}

// referenced globals
var $ = window.jQuery;
var qz_instance = window.qz_instance;
var quakelive = window.quakelive;
var qlPrompt = window.qlPrompt;
var nav = window.nav;
var console = window.console;

(function (aWin, undefined) {
  // !!!
  // IMPORTANT:  It is unlikely that you'll need to change anything in this file.  If you actually do,
  // it is probably only in the config object below.
  // !!!
  var config = {
    version: "0.106",
    consoleCaption: "extraQL v",
    menuCaption: "Userscripts",
    BASE_URL: "http://127.0.0.1:27963/",
    REMOTE_URL: "http://ql.beham.biz:27963/",
    SOURCE_URL: "http://sourceforge.net/p/extraql/source/ci/master/tree/scripts/hook.js?format=raw",
    reset: false,
    autoEnableAllScripts: true,
    async: true,
    enableUnwrap: false // can be overruled by cvar eql_unwrap
  };


  // This is used to indicate if /web_reload is required (e.g. scripts were enabled or disabled)
  var webReloadRequired = false;

  // Holds the caption->handler pairs for script menu items added through HOOK_MANAGER.addMenuItem(...) 
  var scriptMenuItems = {};


  /*
  * Helpers
  */

  function log() {
    var args = Array.prototype.slice.call(arguments);
    if (!args.length) return;
    qz_instance.SendGameCommand("echo \"" + (1 == args.length ? args[0] : JSON.stringify(args)) + "\"");
  }

  function e(text) {
    return extraQL.escapeHtml(text);
  }


  /*
  * localStorage Manager
  */

  var storage = {};
  storage.init = function () {
    var STORAGE_TEMPLATE = { repository: undefined, scripts: { enabled: { /* [id] */ }, code: { /* id */ } } };
    storage.repository = STORAGE_TEMPLATE.repository;
    storage.scripts = STORAGE_TEMPLATE.scripts;

    if (localStorage && localStorage.extraQL) {
      config.autoEnableAllScripts = false;
      if (!config.reset) {
        try {
          // copy values from storage into new template to allow migration between versions
          var tmp = JSON.parse(localStorage.extraQL);
          if (tmp.repository)
            storage.repository = tmp.repository;
          if (tmp.scripts && tmp.scripts.enabled)
            storage.scripts.enabled = tmp.scripts.enabled;
          if (tmp.scripts && tmp.scripts.code)
            storage.scripts.code = tmp.scripts.code;
        } catch (ex) {
        }
      }
    }

    storage.save();
  };

  storage.save = function () {
    localStorage.extraQL = JSON.stringify(storage);
  };


  /*
  * Hook Manager
  */
  function HookManager() {
  }

  HookManager.prototype.init = function () {
    log("^2Initializing " + config.consoleCaption + config.version);
    storage.init();
    this.hud = new HudManager(this);
    this.loadRepository();
  };

  HookManager.prototype.loadRepository = function () {
    // try a local extraQL HTTP server
    var self = this;
    $.ajax({ url: config.BASE_URL + "repository.json", dataType: "json", timeout: 1000 })
      .done(function(repo) { self.initRepository(repo); })
      .fail(this.tryRemoteRepository.bind(this));
  };

  HookManager.prototype.tryRemoteRepository = function () {
    // try remote extraQL HTTP server
    var self = this;
    config.BASE_URL = config.REMOTE_URL;
    extraQL.BASE_URL = config.REMOTE_URL;
    $.ajax({ url: config.BASE_URL + "repository.json", dataType: "json", timeout: 1000 })
      .done(function(repo) { self.initRepository(repo); })
      .fail(this.tryLocalScriptCache.bind(this));
  }

  HookManager.prototype.tryLocalScriptCache = function () {
    // try local script cache
    if (!storage.repository) {
      log("^1Failed to load scripts from extraQL server or local cache.^7 1Scripts are unavailable!");
      return;     
    }
    config.BASE_URL = null;
    extraQL.BASE_URL = null;
    log("^1Could not connected to an extraQL server^7, some cached scripts will not work.");
    this.loadScripts();
  }

  HookManager.prototype.initRepository = function (repo) {
    log("Loading userscripts from ^2" + config.BASE_URL + "^7");

    this.checkForUpdatedHookJs();

    storage.repository = repo;
    if (config.autoEnableAllScripts) {
      // activate all scripts after first run
      $.each(repo, function (index, script) {
        storage.scripts.enabled[script.id] = true;
      });
    }
    storage.save();
    this.loadScripts();
  };

  HookManager.prototype.checkForUpdatedHookJs = function () {
    $.ajax({ url: config.BASE_URL + "proxy?url=" + encodeURI(config.SOURCE_URL), dataType: "html", timeout: 10000 })
      .done(function (code) {
        var match = /@version\s*(\S+)/.exec(code);
        if (match.length < 2) return;
        var localVersion = config.version.split(".");
        var remoteVersion = match[1].split(".");
        var needUpdate = false;
        for (var i = 0; i < Math.min(localVersion.length, remoteVersion.length) ; i++) {
          if (localVersion[i] > remoteVersion[i])
            break;
          needUpdate |= localVersion[i] < remoteVersion[i];
        }
        if (needUpdate) {
          qlPrompt({
            id: "qlhmUpdate",
            title: "hook.js v" + config.version,
            customWidth: 500,
            okLabel: "Close",
            cancelLabel: "Reload",
            cancel: function () { qz_instance.SendGameCommand("web_reload"); },
            body: "<b>A newer version of hook.js is available.</b>" +
              "<br>" +
              "<br>To install the update, either restart extraQL.exe" +
              "<br>or <a href='" + config.SOURCE_URL + "' target='_blank'>download hook.js " + match[1] + "</a> manually." +
              "<br>After the update you need to reload the UI."
          });
        }
      })
    .fail(function (x, y, err) { console.log("update check failed: " + err); });
  }

  HookManager.prototype.loadScripts = function () {
    var self = this;

    // get sorted list of enabled script IDs (to make execution order a bit less random)
    var scriptIds = [];
    $.each(storage.scripts.enabled, function (id, enabled) {
      if (enabled)
        scriptIds.push(id);
    });
    scriptIds.sort();

    // Fire off requests for each script
    $.each(scriptIds, function (index, id) {
      self.loadScript(id);
    });
  };

  HookManager.prototype.loadScript = function(id) {
    var self = this;
    var repoScript = $.grep(storage.repository, function(item) { return item.id == id; })[0];

    if (!repoScript) {
      log("Deactivating userscript with unknown ID ^1" + id + "^7");
      storage.scripts.enabled[id] = false;
      storage.save();
      return;
    }

    log("^7Starting userscript ^5" + id + "^7: ^3" + repoScript.name + "^7");

    if (config.BASE_URL) {
      var url = config.BASE_URL + "scripts/" + repoScript.filename;
      $.ajax({ url: url, dataType: "text", timeout: config.async ? 5000 : 1000, async: config.async })
        .done(function(remotecode) {
          storage.scripts.code[id] = remotecode;
          storage.save();
          if (repoScript.hasOwnProperty("unwrap") && config.enableUnwrap) {
            // scripts marked with @unwrap can be injected without a closure. This preserves file name info for error messages. Such scripts must not contain global "return" statements.
            $.ajax({ url: url, dataType: "script", timeout: config.async ? 5000 : 1000, async: config.async })
              .fail(function(a, b, err) { self.runScriptFromCache(id, err); });            
          }
          else
            self.runScriptFromCache(id, false);
        });
    } else {
      this.runScriptFromCache(id, false);
    }
  }

  HookManager.prototype.runScriptFromCache = function (id, err) {
    var code = storage.scripts.code[id];
    if (err !== false || !code)
      log("^1Failed to retrieve script with ID ^5" + id + "^1 : ^7" + err);
    else {
      var closure = ";(function() { try { " + code + "} catch(ex) { console.log(\"^1" + id + "^7: \" + ex); }})();";
      $.globalEval(closure);
    }
  };

  HookManager.prototype.toggleUserScript = function (id, enable) {
    // return true if web_reload is required to make the change take effect

    if (enable && !storage.scripts.enabled[id]) {
      storage.scripts.enabled[id] = true;
      storage.save();
      this.loadScript(id);
      return false;
    } else if (!enable && storage.scripts.enabled[id]) {
      storage.scripts.enabled[id] = false;
      storage.save();
      log("^7'^5" + id + "^7' has been disabled. You must /web_restart for the change to take effect.");
      return true;
    }
    return false;
  };

  HookManager.prototype.addMenuItem = function (aCaption, aHandler) {
    this.hud.addMenuItem(aCaption, aHandler);
  };


  /**
  * HUD Manager
  */
  function HudManager(aHookManager) {
    this.hm = aHookManager;
    this.width = 900;
    this.selectedScriptElement = null;
    this.rebuildNavBarTimer = undefined;

    quakelive.AddHook("OnLayoutLoaded", this.OnLayoutLoaded.bind(this));

    // window.alert is currently unhandled... remove this if a better option is enabled.
    if ("function alert() { [native code] }" === (aWin.alert + "")) {
      var self = this;
      aWin.alert = function (aMsg) { self.alert.call(self, { title: "ALERT!", body: aMsg }); };
    }
  }

  HudManager.prototype.alert = function (aOptions) {
    var self = this;
    var opts = $.extend({ title: self.hm.name }, aOptions, { alert: true });
    qlPrompt(opts);
  };

  HudManager.prototype.OnLayoutLoaded = function () {
    var layout = quakelive.activeModule ? quakelive.activeModule.GetLayout() : "";
    // Proper layout and no existing menu?
    if ("bare" !== layout && "postlogin_bare" !== layout && !$("#qlhm_nav, #hooka").length) {
      this.injectMenuEntry();
    }
  };

  HudManager.prototype.addMenuItem = function (aCaption, aHandler) {
    scriptMenuItems[aCaption] = aHandler;
    if (this.rebuildNavBarTimer)
      window.clearTimeout(this.rebuildNavBarTimer);
    this.rebuildNavBarTimer = window.setTimeout(function () { this.rebuildNav(); }.bind(this), 200);
  };

  HudManager.prototype.onMenuItemClicked = function (aItem) {
    var caption = $(aItem).text();
    var handler = scriptMenuItems[caption];
    if (handler)
      handler();
  };

  HudManager.prototype.rebuildNav = function () {
    // method could have been called by the timer before the menu was created
    this.rebuildNavBarTimer = undefined;
    if (!nav.navbar[config.menuCaption])
      return;

    // Generate script command submenu
    var sortedMenuItems = Object.keys(scriptMenuItems).sort();
    for (var idx in sortedMenuItems) {
      var caption = sortedMenuItems[idx];
      nav.navbar[config.menuCaption].submenu[caption] = { "class": "qlhm_nav_scriptMenuItem", callback: "" };
    }

    // Rebuild the navbar
    nav.initNav({
      location: "#newnav_top",
      supernav_id: "topNav",
      object: nav.navbar
    });
  };

  HudManager.prototype.injectMenuEntry = function () {
    var self = this;
    if (!window.extraQL)
      return;

    extraQL.addStyle(
      "#qlhm_console { text-align: left !important; width: 100%; }", "#qlhm_console #detailspane { float: right; position: relative; top: 0px; width: 270px; }", "#qlhm_console strong, #qlhm_console legend { font-weight: bold; }", "#qlhm_console fieldset { margin: 10px 0 20px 0; padding: 5px; }", "#qlhm_console ul { list-style: none; }", "#qlhm_console ul li { margin-top: 5px; whitespace: nowrap; overflow:hidden; }", "#qlhm_console ul li.selected { background-color: #ffc; }", "#qlhm_console input.userscript-new { width: 400px; margin-bottom: 5px; }", "#qlhm_console a.del, #qlhm_console a.viewSource { text-decoration: none; }", "#qlhm_console .italic { font-style: italic; }", "#qlhm_console .strike { text-decoration: line-through; }", "#qlhm_console .underline { text-decoration: underline; }", "#qlhm_console .details { margin-left: 15px; font-size: smaller; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: " + (self.width - 50) + "px; }", "#qlhm_console .table { display: table; }", "#qlhm_console .row { display: table-row; }", "#qlhm_console .cell { display: table-cell; padding-right: 10px; }", "#qlhm_console .notInstalled { color: #aaa; }", "#userscripts { overflow: auto; width: 550px; max-height: 400px; }", "#qlhmSource textarea.userscript-source { width: " + (self.width - 140) + "px; }", "#qlhm_nav_scriptMgmt { border-bottom: 1px solid #888; }"
    );

    // New...
    if ($("#tn_exit").length) {
      nav.navbar[config.menuCaption] = {
        id: "qlhm_nav",
        callback: "",
        submenu: {
          "Script Management": { id: "qlhm_nav_scriptMgmt", callback: "" }
        }
      }; // Override nav.initNav to do post-init stuff for QLHM
      var oldInitNav = nav.initNav;
      nav.initNav = function () {
        oldInitNav.apply(nav, arguments);

        // QLHM-specific stuff
        $("#qlhm_nav_scriptMgmt > a").click(function () {
          self.showConsole.call(self);
          return false;
        });
        $(".qlhm_nav_scriptMenuItem").click(function () { self.onMenuItemClicked(this); });
      };
      self.rebuildNav();
    }
      // ... or old? (TODO: remove eventually)
    else {
      extraQL.addStyle(
        "#hooka { position: relative; bottom: 20px; left: 10px; z-index: 99999; font-weight: bold; padding: 2px; text-shadow: 0 0 10px #000; }", "#hooka:hover { cursor: pointer; text-shadow: 0 0 10px yellow; }"
      );
      $("#qlv_mainLogo").append($("<a id='hooka'>HOOK</a>").click(function () {
        self.loadRepository.call(self);
        return false;
      }));
    }
  };

  HudManager.prototype.showConsole = function () {
    var self = this;

    webReloadRequired = false;

    // sort all scripts
    var scripts = storage.repository.slice(0);
    scripts.sort(function (a, b) {
      var x = a.name;
      var y = b.name;
      x = x.toLowerCase(), y = y.toLowerCase();
      return (x < y ? -1 : x > y ? 1 : 0);
    });

    // Generate the console
    var out = [];
    out.push("<div id='qlhm_console'>");
    //out.push("<fieldset>");
    //out.push("<b>Add Scripts:</b>");
    //out.push(" &nbsp; <input type='text' class='userscript-new' placeholder='Enter script URL -or- select from below'>");
    //out.push("<div>");
    //out.push("</fieldset>");

    out.push("<div id='detailspane'>");
    out.push("<b class='underline'>Script Details</b>");
    out.push("<div id='scriptDetails'>(click on a script to see the details)</div>");
    out.push("</div>");

    out.push("<div>");
    out.push("<fieldset>");
    out.push("<b>User Scripts</b>");
    out.push(" &nbsp; ");
    out.push("(<a class='selectall' href='javascript:void(0)'>select all</a>");
    out.push(" - <a class='unselectall' href='javascript:void(0)'>unselect all</a>");
    out.push(")");
    out.push("<ul id='userscripts'>");
    $.each(scripts, function (i, script) {
      out.push(self.scriptRowFromScriptRepository(script));
    });
    out.push("</ul>");
    out.push("</fieldset>");
    out.push("</div>");

    out.push("</div>");

    // Inject the console
    qlPrompt({
      id: "qlhmPrompt",
      title: config.consoleCaption + config.version,
      customWidth: self.width,
      ok: self.handleConsoleOk.bind(self),
      okLabel: "Apply",
      cancel: self.handleConsoleClose.bind(self),
      cancelLabel: "Close",
      body: out.join("")
    });

    // Wait for the prompt to get inserted then do stuff...
    setTimeout(function () {
      $("#modal-cancel").focus();

      var $ui = $("#qlhm_console");
      $ui.on("keydown", function (ev) {
        // Suppress backtick (99.999% intended for the QL console)
        if (192 == ev.keyCode) ev.preventDefault();
      })
        .on("click", "#userscripts li", function () { self.showDetails(this); })
        .on("click", ".selectall", function () { $ui.find(":checkbox").prop("checked", true); })
        .on("click", ".unselectall", function () { $ui.find(":checkbox").prop("checked", false); })
        .on("click", ".deleteunsel", function () {
          $ui.find(":checkbox").each(function (index, item) {
            var $item = $(item);
            if (!$item.prop("checked") && !$item.parent().find("a").hasClass("notInstalled")) {
              $item.closest("li").data("toDelete", true).find("label").addClass("strike");
            }
            self.showDetails();
          });
        });
    }, 0);
  };

  HudManager.prototype.scriptRowFromScriptRepository = function (script) {
    var id = script.id;
    return "<li id='userscript" + id + "' data-id='" + id + "'>"
      + "<input type='checkbox' class='userscript-state'" + (storage.scripts.enabled[id] ? "checked" : "") + ">"
      + " <label for='userscript" + id + "'>" + "<a href='javascript:void(0)' target='_empty'>" + e(script.name) + "</a></label>"
      + "</li>";
  };

  HudManager.prototype.showDetails = function (elem) {
    var self = this, $details = $("#scriptDetails");

    $("#userscripts li").removeClass("selected");
    $details.empty();

    if (!elem) {
      if (self.selectedScriptElement && $(self.selectedScriptElement).length) {
        elem = self.selectedScriptElement;
      } else {
        $details.append("(click on a script to see its details)");
        self.selectedScriptElement = null;
        return;
      }
    }

    var $elem = $(elem);
    var id = $elem.closest("li").data("id");
    var repoScript = $.grep(storage.repository, function (item) { return item.id == id; })[0];

    self.selectedScriptElement = elem;
    $elem.addClass("selected");

    var author = e(repoScript.author);
    var version = e(repoScript.version || "<i>not installed</i>");
    var descr = e(repoScript.description || "");

    if (repoScript && repoScript.note)
      descr = descr + (!descr ? "" : "<br><br>") + "<b>NOTE:</b><br>" + repoScript.note;

    $details.append("<div class='table'>"
      + "<div class='row'>"
      + "<div class='cell'><b>Script ID:</b></div>"
      + "<div class='cell'><a href='" + config.BASE_URL + "scripts/" + repoScript.filename + "' target='_empty'>" + id + "</a></div>"
      + "</div>"
      + "<div class='row'><div class='cell'><b>Author:</b></div><div class='cell'>" + author + "</div></div>"
      + "<div class='row'><div class='cell'><b>Version:</b></div><div class='cell'>" + version + "</div></div>"
      + "</div>"
      + "<br>" + (descr ? ("<p>" + descr + "</p><br>") : "")
    );
  };

  HudManager.prototype.handleConsoleOk = function () {
    var self = this;
    var $con = $("#qlhm_console");
    var $uNew = $con.find("input.userscript-new");

    // enable/disable scripts
    $con.find("input.userscript-state").each(function () {
      var $input = $(this);
      var $item = $input.closest("li");
      var id = $item.data("id");
      webReloadRequired |= self.hm.toggleUserScript(id, $input.prop("checked"));
    });


    $uNew.val("");

    if (webReloadRequired) {
      //$("#modal-buttons").append("<span style='color:#c00000; font-size: 12pt'> ... and reload website</span>");
      $("#modal-cancel").prop("value", "Restart");
    }

    self.showDetails();
  };

  HudManager.prototype.handleConsoleClose = function () {
    if (webReloadRequired)
      qz_instance.SendGameCommand("web_reload");
    else
      $("#qlhmPrompt").jqmHide();
  };


  // Make init and addMenuItem available
  var hm = new HookManager();
  aWin.HOOK_MANAGER = {
    init: hm.init.bind(hm),
    addMenuItem: hm.addMenuItem.bind(hm)
  };

})(window);



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
function ExtraQL() {
  var BASE_URL = "http://127.0.0.1:27963/";
  var tabClickHandlers = {};
  var chatBarTabified = false;
  var lastServerCheckTimestamp = 0;
  var lastServerCheckResult = false;

  function init() {
    addStyle("#chatContainer .fullHeight { height:550px; }");
  }

  // public: add CSS rules
  // params: string...
  function addStyle(/*...*/) {
    var css = "";
    for (var i = 0; i < arguments.length; i++)
      css += "\n" + arguments[i];
    $("head").append("<style>" + css + "\n</style>");
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


  var REGEX_FORMAT = new RegExp("(^|[^{])((?:{{)*){([0-9]+)}", "g");

  // public: .NET-like string.Format() which allows positional placeholders like {0} ... to be replaced with parameter values
  function format(template /*, ... */) {
    var args = arguments;
    return template.replace(REGEX_FORMAT, function (item, p1, p2, p3) {
      var intVal = parseInt(p3);
      var replace = intVal >= 0 ? args[1 + intVal] : "";
      return p1 + p2 + replace;
    }).replace("{{", "{").replace("}}", "}");
  };

  // public: add a tab page to the chat bar/area
  function addTabPage(id, caption, content, onClick) {
    tabifyChat();

    if (!onClick)
      onClick = function () { showTabPage(id); };
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
    $("#collapsableChat").empty().append("<div class='tab' id='tab_qlv_chatControl'>Chat</div>");

    $("#qlv_chatControl").prepend("<div class='chatTitleBar'>Chat<div class='close'>X</div></div>");

    quakelive.mod_friends.roster.UI_OnRosterUpdated = function () {
      var numOnline = this.GetNumOnlineContacts();
      $('#tab_qlv_chatControl').text('Chat (' + numOnline + ')');
      this.UI_Show();
    }.bind(quakelive.mod_friends.roster);
    quakelive.mod_friends.UI_SetChatTitle = function () {
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
    for (var id in tabClickHandlers)
      $("#tab_" + id).unbind("click").click(tabClickHandlers[id]);

    $("#chatContainer .chatTitleBar").unbind("click").click(closeTabPage);
  }

  //
  // functions to communicate with extraQL server
  //

  // public: test if the local extraQL HTTP server is running
  function isLocalServerRunning() {
    if (!this.BASE_URL)
      return false;
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
      error: function () { lastServerCheckResult = false; }
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

    addTabPage: addTabPage,
    showTabPage: showTabPage,
    closeTabPage: closeTabPage
  };
}