/*
// @name        extraQL Script Manager
// @version     1.6
// @author      wn, PredatH0r
// @description	Manages the installation and execution of QuakeLive userscripts

This script is a stripped down version of wn's QuakeLive Hook Manager (QLHM),
which is designed to work with a local or remote extraQL.exe script server.

Version 1.6
- added link to Wiki page in Script Management / Script details
- showing screenshot in Script Management / Script details

Version 1.4
- HTTPS compatibility
- code cleanup, now allows an array of BASE_URLS to probe through

Version 1.3
- Account Settings page failed to open when hook.js was installed

Version 1.2
- unmerged extraQL.js library to separate file (so updates can be loaded through hook.js)

Version 0.109
- added "priority" parameter to extraQL.addTabPage() so tabs dont show up in random order

Version 0.108
- added border in Script Management console list box to make scroll bar more visible

Version 0.107
- changed display string from "extraQL" to "hook.js" to avoid confusion of version numbers

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
  if (location.protocol == "https:") // Account Settings uses https which doesn't allow running scripts from a http server
    return;

  HOOK_MANAGER.init();
}

// referenced globals
var $ = window.jQuery;
var qz_instance = window.qz_instance;
var quakelive = window.quakelive;
var qlPrompt = window.qlPrompt;
var nav = window.nav;
var console = window.console;

(function(aWin, undefined) {
  // !!!
  // IMPORTANT:  It is unlikely that you'll need to change anything in this file.  If you actually do,
  // it is probably only in the config object below.
  // !!!
  var config = {
    version: "1.6",
    consoleCaption: "hook.js v",
    menuCaption: "Userscripts",
    BASE_URLS: [ "http://localhost:27963/", "https://localhost:27963/", "http://ql.beham.biz:27963/" ],
    BASE_URL: null,
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
  storage.init = function() {
    var STORAGE_TEMPLATE = { repository: undefined, scripts: { enabled: { /* [id] */  }, code: { /* id */  } } };
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

  storage.save = function() {
    localStorage.extraQL = JSON.stringify(storage);
  };


  /*
  * Hook Manager
  */
  function HookManager() {
  }

  HookManager.prototype.init = function() {
    log("^2Initializing " + config.consoleCaption + config.version);
    storage.init();
    this.hud = new HudManager(this);
    this.loadRepository(0);
  };

  HookManager.prototype.loadRepository = function (baseUrlIndex) {
    if (baseUrlIndex >= config.BASE_URLS.length) {
      this.tryLocalScriptCache();
      return;
    }

    config.BASE_URL = config.BASE_URLS[baseUrlIndex];
    var self = this;
    $.ajax({ url: config.BASE_URL + "repository.json", dataType: "json", timeout: 3000 })
      .done(function(repo) { self.initRepository(repo); })
      .fail(function () { self.loadRepository(baseUrlIndex + 1); });
  };

  HookManager.prototype.tryLocalScriptCache = function() {
    if (!storage.repository) {
      log("^1Failed to load scripts from extraQL server or local cache.^7 Scripts are unavailable!");
      return;
    }
    config.BASE_URL = null;
    log("^1Could not connected to an extraQL server^7, some cached scripts will not work.");
    $.globalEval(storage.scripts.code["extraQL"]);
    this.loadScripts();
  };

  HookManager.prototype.initRepository = function(repo) {
    log("Loading userscripts from ^2" + config.BASE_URL + "^7");

    var self = this;
    $.ajax({ url: config.BASE_URL + "scripts/extraQL.js", dataType: "text" })
      .done(function (code) {
        storage.scripts.code["extraQL"] = code;
        storage.save();
        $.globalEval(code);
        self.runScripts(repo);
      })
      .fail(function() {
        config.BASE_URL = null;
        log("^1Could not load extraQL.js library.^7 Scripts are unavailable!");
      });
  }

  HookManager.prototype.runScripts = function (repo) {
    storage.repository = repo;
    if (config.autoEnableAllScripts) {
      // activate all scripts after first run
      $.each(repo, function(index, script) {
        storage.scripts.enabled[script.id] = true;
      });
    }
    storage.save();
    this.loadScripts();

    this.checkForUpdatedHookJs();
  };

  HookManager.prototype.checkForUpdatedHookJs = function() {
    $.ajax({
      url: config.BASE_URL + "proxy",
      data: { url: config.SOURCE_URL },
      dataType: "text",
      timeout: 10000
      })
      .done(function(code) {
        var match = /@version\s*(\S+)/.exec(code);
        if (match.length < 2) return;
        var localVersion = config.version.split(".");
        var remoteVersion = match[1].split(".");
        var needUpdate = false;
        for (var i = 0; i < Math.min(localVersion.length, remoteVersion.length); i++) {
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
            cancel: function() { qz_instance.SendGameCommand("web_reload"); },
            body: "<b>A newer version of hook.js is available.</b>" +
              "<br>" +
              "<br>To install the update, either restart extraQL.exe" +
              "<br>or <a href='" + config.SOURCE_URL + "' target='_blank'>download hook.js " + match[1] + "</a> manually." +
              "<br>After the update you need to reload the UI."
          });
        }
      })
      .fail(function(x, y, err) { console.log("update check failed: " + err); });
  };

  HookManager.prototype.loadScripts = function() {
    var self = this;
    extraQL.BASE_URL = config.BASE_URL;

    // get sorted list of enabled script IDs (to make execution order a bit less random)
    var scriptIds = [];
    $.each(storage.scripts.enabled, function(id, enabled) {
      if (enabled)
        scriptIds.push(id);
    });
    scriptIds.sort();

    // Fire off requests for each script
    $.each(scriptIds, function(index, id) {
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
      $.ajax({ url: url, dataType: "text", timeout: config.async ? 10000 : 10000, async: config.async })
        .done(function(remotecode) {
          storage.scripts.code[id] = remotecode;
          storage.save();
          if (repoScript.hasOwnProperty("unwrap") && config.enableUnwrap) {
            // scripts marked with @unwrap can be injected without a closure. This preserves file name info for error messages. Such scripts must not contain global "return" statements.
            $.ajax({ url: url, dataType: "script", timeout: config.async ? 5000 : 1000, async: config.async })
              .fail(function(a, b, err) { self.runScriptFromCache(id, err); });
          } else
            self.runScriptFromCache(id, false);
        });
    } else {
      this.runScriptFromCache(id, false);
    }
  };
  HookManager.prototype.runScriptFromCache = function(id, err) {
    var code = storage.scripts.code[id];
    if (err !== false || !code)
      log("^1Failed to retrieve script with ID ^5" + id + "^1 : ^7" + err);
    else {
      var closure = ";(function() { try { " + code + "} catch(ex) { console.log(\"^1" + id + "^7: \" + ex); }})();";
      $.globalEval(closure);
    }
  };

  HookManager.prototype.toggleUserScript = function(id, enable) {
    // return true if web_reload is required to make the change take effect

    if (enable && !storage.scripts.enabled[id]) {
      storage.scripts.enabled[id] = true;
      storage.save();
      this.loadScript(id);
      return false;
    } else if (!enable && storage.scripts.enabled[id]) {
      storage.scripts.enabled[id] = false;
      storage.save();
      log("^7'^5" + id + "^7' has been disabled. You must /web_reload for the change to take effect.");
      return true;
    }
    return false;
  };

  HookManager.prototype.addMenuItem = function(aCaption, aHandler) {
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
      aWin.alert = function(aMsg) { self.alert.call(self, { title: "ALERT!", body: aMsg }); };
    }
  }

  HudManager.prototype.alert = function(aOptions) {
    var self = this;
    var opts = $.extend({ title: self.hm.name }, aOptions, { alert: true });
    qlPrompt(opts);
  };

  HudManager.prototype.OnLayoutLoaded = function() {
    var layout = quakelive.activeModule ? quakelive.activeModule.GetLayout() : "";
    // Proper layout and no existing menu?
    if ("bare" !== layout && "postlogin_bare" !== layout && !$("#qlhm_nav, #hooka").length) {
      this.injectMenuEntry();
    }
  };

  HudManager.prototype.addMenuItem = function(aCaption, aHandler) {
    scriptMenuItems[aCaption] = aHandler;
    if (this.rebuildNavBarTimer)
      window.clearTimeout(this.rebuildNavBarTimer);
    this.rebuildNavBarTimer = window.setTimeout(function() { this.rebuildNav(); }.bind(this), 200);
  };

  HudManager.prototype.onMenuItemClicked = function(aItem) {
    var caption = $(aItem).text();
    var handler = scriptMenuItems[caption];
    if (handler)
      handler();
  };

  HudManager.prototype.rebuildNav = function() {
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

  HudManager.prototype.injectMenuEntry = function() {
    var self = this;
    if (!window.extraQL)
      return;

    extraQL.addStyle(
      "#qlhm_console { text-align: left !important; width: 100%; }",
      "#qlhm_console #detailspane { float: right; position: relative; top: 0px; width: 270px; }",
      "#qlhm_console strong, #qlhm_console legend { font-weight: bold; }",
      "#qlhm_console fieldset { margin: 10px 0 20px 0; padding: 5px; }",
      "#qlhm_console ul { list-style: none; }",
      "#qlhm_console ul li { margin-top: 5px; whitespace: nowrap; overflow:hidden; }",
      "#qlhm_console ul li.selected { background-color: #ffc; }",
      "#qlhm_console input.userscript-new { width: 400px; margin-bottom: 5px; }",
      "#qlhm_console a.del, #qlhm_console a.viewSource { text-decoration: none; }",
      "#qlhm_console .italic { font-style: italic; }",
      "#qlhm_console .strike { text-decoration: line-through; }",
      "#qlhm_console .underline { text-decoration: underline; }",
      "#qlhm_console .details { margin-left: 15px; font-size: smaller; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: " + (self.width - 50) + "px; }",
      "#qlhm_console .table { display: table; }",
      "#qlhm_console .row { display: table-row; }",
      "#qlhm_console .cell { display: table-cell; padding-right: 10px; }",
      "#qlhm_console .notInstalled { color: #aaa; }",
      "#userscripts { overflow: auto; width: 550px; max-height: 400px; border: 1px solid #666; padding: 5px; margin-top: 5px; }",
      "#qlhmSource textarea.userscript-source { width: " + (self.width - 140) + "px; }",
      "#qlhm_nav_scriptMgmt { border-bottom: 1px solid #888; }"
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
      nav.initNav = function() {
        oldInitNav.apply(nav, arguments);

        // QLHM-specific stuff
        $("#qlhm_nav_scriptMgmt > a").click(function() {
          self.showConsole.call(self);
          return false;
        });
        $(".qlhm_nav_scriptMenuItem").click(function() { self.onMenuItemClicked(this); });
      };
      self.rebuildNav();
    }
    // ... or old? (TODO: remove eventually)
    else {
      extraQL.addStyle(
        "#hooka { position: relative; bottom: 20px; left: 10px; z-index: 99999; font-weight: bold; padding: 2px; text-shadow: 0 0 10px #000; }", "#hooka:hover { cursor: pointer; text-shadow: 0 0 10px yellow; }"
      );
      $("#qlv_mainLogo").append($("<a id='hooka'>HOOK</a>").click(function() {
        self.loadRepository.call(self);
        return false;
      }));
    }
  };

  HudManager.prototype.showConsole = function() {
    var self = this;

    webReloadRequired = false;

    // sort all scripts
    var scripts = storage.repository.slice(0);
    scripts.sort(function(a, b) {
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
    $.each(scripts, function(i, script) {
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
    setTimeout(function() {
      $("#modal-cancel").focus();

      var $ui = $("#qlhm_console");
      $ui.on("keydown", function(ev) {
          // Suppress backtick (99.999% intended for the QL console)
          if (192 == ev.keyCode) ev.preventDefault();
        })
        .on("click", "#userscripts li", function() { self.showDetails(this); })
        .on("click", ".selectall", function() { $ui.find(":checkbox").prop("checked", true); })
        .on("click", ".unselectall", function() { $ui.find(":checkbox").prop("checked", false); })
        .on("click", ".deleteunsel", function() {
          $ui.find(":checkbox").each(function(index, item) {
            var $item = $(item);
            if (!$item.prop("checked") && !$item.parent().find("a").hasClass("notInstalled")) {
              $item.closest("li").data("toDelete", true).find("label").addClass("strike");
            }
            self.showDetails();
          });
        });
    }, 0);
  };

  HudManager.prototype.scriptRowFromScriptRepository = function(script) {
    var id = script.id;
    return "<li id='userscript" + id + "' data-id='" + id + "'>"
      + "<input type='checkbox' class='userscript-state'" + (storage.scripts.enabled[id] ? "checked" : "") + ">"
      + " <label for='userscript" + id + "'>" + "<a href='javascript:void(0)' target='_empty'>" + e(script.name) + "</a></label>"
      + "</li>";
  };

  HudManager.prototype.showDetails = function(elem) {
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
    var repoScript = $.grep(storage.repository, function(item) { return item.id == id; })[0];

    self.selectedScriptElement = elem;
    $elem.addClass("selected");

    var author = e(repoScript.author);
    var version = e(repoScript.version || "<i>not installed</i>");
    var descr = e(repoScript.description || "");
    var scriptName = repoScript.filename.replace(".usr.js", "");
    var wikiUrl = "http://sourceforge.net/p/extraql/wiki/" + scriptName + "/";
    var screenshot = repoScript.screenshot || "http://beham.biz/extraql/" + scriptName.toLowerCase() + "1.png";

    if (repoScript && repoScript.note)
      descr = descr + (!descr ? "" : "<br><br>") + "<b>NOTE:</b><br>" + repoScript.note;

    $details.append("<div class='table'>"
      + "<div class='row'>"
      + "<div class='cell'><b>Script ID:</b></div>"
      + "<div class='cell'><a href='" + config.BASE_URL + "scripts/" + repoScript.filename + "' target='_empty'>" + id + "</a></div>"
      + "</div>"
      + "<div class='row'><div class='cell'><b>Author:</b></div><div class='cell'>" + author + "</div></div>"
      + "<div class='row'><div class='cell'><b>Version:</b></div><div class='cell'>" + version + "</div></div>"
      + "<div class='row'><div class='cell'><b>Wiki:</b></div><div class='cell'><a href='" + wikiUrl + "' target='_blank'>" + e(scriptName) + "</a></div></div>"
      + "</div>"
      + "<br>" + (descr ? ("<p>" + descr + "</p><br>") : "")
      + "<a href='" + wikiUrl + "' target='_blank'><img id='scriptDetailScreenshot' src='" + screenshot + "' style='margin-top: 30px; max-width: 260px; max-height: 280px' alt='" + e(scriptName) + "'></a>"
    );
    $("#scriptDetailScreenshot").error(function() { $(this).hide(); });
  };

  HudManager.prototype.handleConsoleOk = function() {
    var self = this;
    var $con = $("#qlhm_console");
    var $uNew = $con.find("input.userscript-new");

    // enable/disable scripts
    $con.find("input.userscript-state").each(function() {
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

  HudManager.prototype.handleConsoleClose = function() {
    if (webReloadRequired)
      qz_instance.SendGameCommand("web_reload");
    else
      $("#qlhmPrompt").jqmHide();
  };


  // Make init and addMenuItem available
  var hm = new HookManager();
  aWin.HOOK_MANAGER = {
    version: config.version,
    init: hm.init.bind(hm),
    addMenuItem: hm.addMenuItem.bind(hm)
  };

})(window);
