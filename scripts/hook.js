/*
// @name        extraQL Script Manager
// @version     0.104
// @author      PredatH0r
// @credits     wn
// @description	Manages the installation and execution of QuakeLive userscripts

This script is a stripped down version of wn's QuakeLive Hook Manager (QLHM),
which is designed to work with a local extraQL.exe script server.

Version 0.104
- fixed: on 2nd run all scripts were deactivated again

Version 0.103
- fixed async loading and execution of scripts without @unwrap

Version 0.102
- fallback to remote script server when there is no local server running

*/

// called in ql.Init
function main_hook() {
  if (quakelive.mod_legals !== quakelive.activeModule)
    HOOK_MANAGER.init();
}

// referenced globals
var $ = window.jQuery;
var qz_instance = window.qz_instance;
var quakelive = window.quakelive;
var qlPrompt = window.qlPrompt;
var nav = window.nav;


(function(aWin, undefined) {
// !!!
// IMPORTANT:  It is unlikely that you'll need to change anything in this file.  If you actually do,
// it is probably only in the config object below.
// !!!
  var config = {
    consoleCaption: "extraQL v0.104",
    menuCaption: "Userscripts",
    BASE_URL: "http://127.0.0.1:27963/",
    REMOTE_URL: "http://ql.beham.biz:27963/",
    reset: false,
    autoEnableAllScripts: true,
    async: true
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

  var storage = { };
  storage.init = function() {
    var STORAGE_TEMPLATE = { scripts: { enabled: { /* [id] */  } } };

    if (localStorage && localStorage.extraQL) {
      config.autoEnableAllScripts = false;
      if (!config.reset) {
        try {
          var tmp = JSON.parse(localStorage.extraQL);
          storage.scripts = tmp.scripts;
        } catch (ex) {
        }
      }
    }

    if (!$.isPlainObject(storage.scripts))
      storage.scripts = STORAGE_TEMPLATE.scripts;

    storage.save();
  };

  storage.save = function() {
    localStorage.extraQL = JSON.stringify(storage);
  };


/*
* Hook Manager
*/
  function HookManager() {
    this.repo = [];
  }

  HookManager.prototype.init = function() {
    log("^2Initializing " + config.consoleCaption);
    storage.init();
    this.hud = new HudManager(this);
    this.initExtraQL();
  };

  HookManager.prototype.initExtraQL = function () {
    // first try with a local extraQL HTTP server
    $.ajax({ url: config.BASE_URL + "scripts/extraQL.js", dataType: "script", timeout: 1000 })
      .done(this.initScripts.bind(this))
      .fail(this.tryRemoteRepository.bind(this));
  };

  HookManager.prototype.tryRemoteRepository = function () {
    // try remote extraQL HTTP server
    config.BASE_URL = config.REMOTE_URL;
    $.ajax({ url: config.BASE_URL + "scripts/extraQL.js", dataType: "script", timeout: 1000 })
      .done(this.initScripts.bind(this))
      .fail(function () { log("^1Failed^7 to connect to extraQL script server. Scripts are disabled."); });
  }

  HookManager.prototype.initScripts = function() {
    var self = this;
    log("Loading userscripts from ^2" + config.BASE_URL + "^7");
    window.extraQL.BASE_URL = config.BASE_URL;
    $.ajax({ url: config.BASE_URL + "repository.json", dataType: "json", timeout: 1000 })
      .done(function(scriptList) {
        self.repo = scriptList;
        if (config.autoEnableAllScripts) {
          // activate all scripts after first run
          $.each(scriptList, function(index, script) {
            storage.scripts.enabled[script.id] = true;
          });
          storage.save();
        }
        self.loadScripts();
      });
  };

  HookManager.prototype.loadScripts = function() {
    var self = this;

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

  HookManager.prototype.loadScript = function (id) {
    var repoScript = $.grep(this.repo, function(item) { return item.id == id; })[0];

    if (!repoScript) {
      log("Deactivating userscript with unknown ID ^1" + id + "^7");
      storage.scripts.enabled[id] = false;
      storage.save();
      return;
    }

    log("^7Starting userscript ^5" + id + "^7: ^3" + repoScript.name + "^7");
    var url = config.BASE_URL + "scripts/" + repoScript.filename;

    if (repoScript.hasOwnProperty("unwrap")) {
      // scripts marked with @unwrap can be executed directly to preserve file name info for error messages. They must not contain global "return" statements.
      $.ajax({ url: url, dataType: "script", timeout: config.async ? 5000 : 1000, async: config.async })
        .fail(function(d1, d2, d3, err) {
          log("^1Failed to retrieve script with ID ^5" + id + "^1 : ^7" + err);
      });
    }
    else {
      // scripts not marked with @unwrap are put into a closure
      $.ajax({ url: url, dataType: "html", timeout: config.async ? 5000 : 1000, async: config.async })
        .done(function(code) {
          var closure = ";(function() { try { " + code + "} catch(ex) { console.log(\"^1" + id + "^7: \" + ex); }})();";
          $.globalEval(closure);
        })
        .fail(function (d1, d2, d3, err) {
          log("^1Failed to retrieve script with ID ^5" + id + "^1 : ^7" + err);
        });
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
      log("^7'^5" + id + "^7' has been disabled. You must /web_restart for the change to take effect.");
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
    var sortedMenus = keys(scriptMenuItems).sort();
    for (var caption in sortedMenus)
      nav.navbar[config.menuCaption].submenu[caption] = { "class": "qlhm_nav_scriptMenuItem", callback: "" };

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
    var scripts = this.hm.repo.slice(0);
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
      title: config.consoleCaption,
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
    var repoScript = $.grep(this.hm.repo, function(item) { return item.id == id; })[0];

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
    init: hm.init.bind(hm),
    addMenuItem: hm.addMenuItem.bind(hm)
  };

})(window);