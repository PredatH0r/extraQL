/*
// @name        QUAKE LIVE HOOK MANAGER
// @version     0.5.1
// @author      wn
// @contributor PredatH0r
// @description	Manages the installation and execution of QuakeLive userscripts
*/

// called in ql.Init
function main_hook() {
  qz_instance.SendGameCommand("echo main_hook called");
  if (quakelive.mod_legals !== quakelive.activeModule) HOOK_MANAGER.init();
}


(function(aWin, undefined) {
// !!!
// IMPORTANT:  It is unlikely that you'll need to change anything in this file.  If you actually do,
// it is probably only in the config object below.
// !!!
var config = {
    BASE_URL: "http://qlhm.phob.net/"
  , EXTRAQL_URL: "http://127.0.0.1:27963/"
  , manual: []
  , debug: false
  , menuCaption: "Userscripts"
};

// !!!
// IMPORTANT:  Changing anything below this point might break things!
// !!!


// This is the service that acts as a proxy to retrieve userscripts.  It also does some extra work,
// such as pre-parsing of the userscript metadata block.
var JSONP_PROXY_TEMPLATE = config.BASE_URL + "uso/{{id}}";

// This is used to determine whether `hook.js` and the proxy service are on the same version.
var VERSION_CHECK_URL = config.BASE_URL + "versioncheck";

// List of userscripts shown in the HOOK console
var USERSCRIPT_REPOSITORY_URL = config.BASE_URL + "qlhmUserscriptRepository.js";

// This is used to indicate if /web_reload is required (e.g. scripts were enabled or disabled)
var webReloadRequired = false;

// Holds the caption->handler pairs for script menu items added through HOOK_MANAGER.addMenuItem(...) 
var scriptMenuItems = {};

// Local reference to jQuery (set during initialization)
var $;


/**
 * Helpers
 */
function log() {
  var args = Array.prototype.slice.call(arguments);
  if (!args.length) return;
  if (console.firebuglite) console.log.apply(console, args);
  qz_instance.SendGameCommand("echo " + (1 == args.length ? args[0] : JSON.stringify(args)));
}

function debug() {
  if (!config.debug) return;
  var args = Array.prototype.slice.call(arguments);
  if (!args.length) return;
  if (console.firebuglite) console.debug.apply(console, args);
  qz_instance.SendGameCommand("echo DEBUG: " + (1 == args.length ? args[0] : JSON.stringify(args)));
}

// Defines a read-only property on an object (enumerable by default)
function readOnly(aObj, aProp, aVal, aEnum) {
  aEnum = undefined === aEnum ? true : !!aEnum;
  Object.defineProperty(aObj, aProp, {get: function() { return aVal }, enumerable: aEnum});
}

// Simple extend with exceptions
function extend(aTarget, aSource, aProtect) {
  aProtect = Array.isArray(aProtect) ? aProtect : [];
  for (var p in aSource) {
    if (-1 === aProtect.indexOf(p)) {
      aTarget[p] = aSource[p];
    }
  }
}

// Escape HTML
// originally from mustache.js MIT ( https://raw.github.com/janl/mustache.js/master/LICENSE )
var entityMap = { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;", "/": "&#x2F;" };
function e(aStr) {
  return String(aStr).replace(/[&<>"'\/]/g, function(s) {
    return entityMap[s];
  });
}

function injectStyle(aStyle) {
  var s = document.createElement("style");
  s.type = "text/css";
  s.textContent = Array.isArray(aStyle) ? aStyle.join("") : aStyle;
  document.body.appendChild(s);
}

function injectScript(aScript) {
  var code = Array.isArray(aScript) ? aScript.join("") : aScript;
  $.globalEval(code);
}


/**
 * localStorage Manager
 */
var storage = Object.create(null);
readOnly(storage, "root", "qlhm");
readOnly(storage, "init", function storageInit(aCallback, aForceReset) {
  var STORAGE_TEMPLATE = {settings: {}, scripts: {available: {}, enabled: {}, cache: {}}};

  if (aForceReset) log("^1WARNING: ^7resetting QLHM localStorage");

  if (!aForceReset && storage.root in localStorage) {
    try {
      var tmp = JSON.parse(localStorage[storage.root]);
      extend(storage, {settings: tmp.settings, scripts: tmp.scripts});
    }
    catch(e) {}
  }

  if (aForceReset || !storage.settings || !jQuery.isPlainObject(storage.settings)) {
    storage.scripts = STORAGE_TEMPLATE.scripts;
    storage.settings = STORAGE_TEMPLATE.settings;
    storage.save();
  }

  // convert old integer based script-ids to strings. Required for extraQL and maybe other non-USO script sources
  if (Array.isArray(storage.scripts.available)) {
    var available = {};
    for (var i = 0; i < storage.scripts.available.length; i++)
      available[storage.scripts.available[i].toString()] = true;
    storage.scripts.available = available;

    var enabled = {};
    for (i = 0; i < storage.scripts.enabled.length; i++)
      enabled[storage.scripts.enabled[i].toString()] = true;
    storage.scripts.enabled = enabled;

    var cache = {};
    for (var key in storage.scripts.cache)
      cache[key.toString()] = storage.scripts.cache[key];
    storage.scripts.cache = cache;
  }
  aCallback();
});
readOnly(storage, "save", function storageSave() {
  setTimeout(function() {
    localStorage[storage.root] = JSON.stringify({settings: storage.settings, scripts: storage.scripts});
  }, 0);
});


/**
 * HUD Manager
 */
function HudManager(aHookManager) {
  readOnly(this, "hm", aHookManager);
  readOnly(this, "width", 900);
  this.selectedScriptElement = null;
  this.rebuildNavBarTimer = undefined;

  quakelive.AddHook("OnLayoutLoaded", this.OnLayoutLoaded.bind(this));

  // window.alert is currently unhandled... remove this if a better option is enabled.
  if ("function alert() { [native code] }" === (aWin.alert+"")) {
    var self = this;
    aWin.alert = function(aMsg) { self.alert.call(self, {title: "ALERT!", body: aMsg}) };
  }
}

HudManager.prototype.alert = function(aOptions) {
  var self = this;
  var opts = $.extend({title: self.hm.name}, aOptions, {alert: true});
  qlPrompt(opts);
}

HudManager.prototype.OnLayoutLoaded = function() {
  var layout = quakelive.activeModule ? quakelive.activeModule.GetLayout() : "";
  // Proper layout and no existing menu?
  if ("bare" !== layout && "postlogin_bare" !== layout && !$("#qlhm_nav,#hooka").length) {
    this.injectMenuEntry();
  }
}

HudManager.prototype.addMenuItem = function(aCaption, aHandler) {
  scriptMenuItems[aCaption] = aHandler;
  if (this.rebuildNavBarTimer)
    window.clearTimeout(this.rebuildNavBarTimer);
  this.rebuildNavBarTimer = window.setTimeout(function() { this.rebuildNav(); }.bind(this), 200);
}

HudManager.prototype.onMenuItemClicked = function(aItem) {
  var caption = $(aItem).text();
  var handler = scriptMenuItems[caption];
  if (handler)
    handler();
}

HudManager.prototype.rebuildNav = function() {
  // method could have been called by the timer before the menu was created
  this.rebuildNavBarTimer = undefined;
  if (!nav.navbar[config.menuCaption])
    return;

  // Generate script command submenu
  for (var caption in scriptMenuItems) {
    nav.navbar[config.menuCaption].submenu[caption] = { class: "qlhm_nav_scriptMenuItem", callback: "" };
  }

  // Rebuild the navbar
  nav.initNav({
      location: "#newnav_top"
    , supernav_id: "topNav"
    , object: nav.navbar
  });
}

HudManager.prototype.injectMenuEntry = function() {
  var self = this;

  injectStyle([
      "#qlhm_console { text-align: left !important; width: 100%; }"
    , "#qlhm_console #detailspane { float: right; position: relative; top: 0px; width: 270px; }"
    , "#qlhm_console strong, #qlhm_console legend { font-weight: bold; }"
    , "#qlhm_console fieldset { margin: 10px 0 20px 0; padding: 5px; }"
    , "#qlhm_console ul { list-style: none; }"
    , "#qlhm_console ul li { margin-top: 5px; whitespace: nowrap; overflow:hidden; }"
    , "#qlhm_console ul li.selected { background-color: #ffc; }"
    , "#qlhm_console input.userscript-new { width: 400px; margin-bottom: 5px; }"
    , "#qlhm_console a.del, #qlhm_console a.viewSource { text-decoration: none; }"
    , "#qlhm_console .italic { font-style: italic; }"
    , "#qlhm_console .strike { text-decoration: line-through; }"
    , "#qlhm_console .underline { text-decoration: underline; }"
    , "#qlhm_console .details { margin-left: 15px; font-size: smaller; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: " + (self.width - 50) + "px; }"
    , "#qlhm_console .table { display: table; }"
    , "#qlhm_console .row { display: table-row; }"
    , "#qlhm_console .cell { display: table-cell; padding-right: 10px; }"
    , "#qlhm_console .notInstalled { color: #aaa; }"
    , "#userscripts { overflow: auto; width: 550px; max-height: 400px; }"
    , "#qlhmSource textarea.userscript-source { width: " + (self.width - 140) + "px; }"
    , "#qlhm_nav_scriptMgmt { border-bottom: 1px solid #888; }"
  ]);

  // New...
  if ($("#tn_exit").length) {
    nav.navbar[config.menuCaption] = {
      id: "qlhm_nav"
      , callback: ""
      , submenu: {
        "Script Management": { id: "qlhm_nav_scriptMgmt", callback: "" }
      }
    }

    // Override nav.initNav to do post-init stuff for QLHM
    var oldInitNav = nav.initNav;
    nav.initNav = function() {
      oldInitNav.apply(nav, arguments);

      // QLHM-specific stuff
      $("#qlhm_nav_scriptMgmt > a").click(function() { self.loadRepository.call(self); return false; });
      $(".qlhm_nav_scriptMenuItem").click(function() { self.onMenuItemClicked(this); });
    }

    self.rebuildNav();
  }
  // ... or old? (TODO: remove eventually)
  else {
    injectStyle([
        "#hooka { position: relative; bottom: 20px; left: 10px; z-index: 99999; font-weight: bold; padding: 2px; text-shadow: 0 0 10px #000; }"
      , "#hooka:hover { cursor: pointer; text-shadow: 0 0 10px yellow; }"
    ]);
    $("#qlv_mainLogo").append($("<a id='hooka'>HOOK</a>").click(function() { self.loadRepository.call(self); return false; }));
  }
}

function cleanupName(name) {
  if (name.indexOf("Quake Live ") == 0)
    return name.substr(11);
  if (name.indexOf("QL ") == 0)
    return name.substr(3);
  return name;
}


HudManager.prototype.scriptRowFromScript = function(aScript) {
  var id = aScript._meta.id.toString()
    , enabled = storage.scripts.enabled[id]
    ;

  return "<li id='userscript" + id + "' data-id='" + id + "'>"
       + "<input type='checkbox' class='userscript-state' " + (enabled ? "checked" : "") + ">"
       + " <label for='userscript" + id + "'><a href='javascript:void(0)'>" + e(cleanupName(aScript.headers.name[0])) + "</a></label>"
       + "</li>";
}

HudManager.prototype.scriptRowFromScriptRepository = function(aScriptInfo) {
  var id = aScriptInfo.id;
  return "<li id='userscript" + id + "' data-id='" + id + "'>"
       + "<input type='checkbox' class='userscript-state'>"
       + " <label for='userscript" + id + "'>"
       + "<a class='notInstalled italic' href='javascript:void(0)' target='_empty'>" + e(cleanupName(aScriptInfo.name)) + "</a></label>"
       + "</li>";
}

HudManager.prototype.loadRepository = function() {
  var self = this;
  $.getScript(USERSCRIPT_REPOSITORY_URL).always(function() { self.showConsole.call(self) });
}

HudManager.prototype.showConsole = function () {
  var self = this;

  webReloadRequired = false;

  // Get and sort all scripts
  var scripts = [];
  $.each(storage.scripts.available, function(id) {
    scripts.push(storage.scripts.cache[id]);
  });
  $.each(HOOK_MANAGER.userscriptRepository, function(index, script) {
    if (!storage.scripts.available[script.id.toString()])
      scripts.push(script);
  });

  scripts.sort(function(a, b) {
    var x = cleanupName(a.headers ? a.headers.name[0] : a.name);
    var y = cleanupName(b.headers ? b.headers.name[0] : b.name);
    x = x.toLowerCase(), y = y.toLowerCase();
    return (x < y ? -1 : x > y ? 1 : 0);
  });

  // Generate the console
  var out = [];
  out.push("<div id='qlhm_console'>");
  out.push("<fieldset>");
  out.push("<b>Add Scripts:</b>");
  out.push(" &nbsp; <input type='text' class='userscript-new' placeholder='Enter userscripts.org (USO) script IDs directly -or- select from below'>");
  out.push("<div>");
  out.push("</fieldset>");

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
  out.push(" - <a class='deleteunsel' href='javascript:void(0)'>delete unselected</a>");
  out.push(")");
  out.push("<ul id='userscripts'>");
  $.each(scripts, function(i, script) {
    if (script._meta)
      out.push(self.scriptRowFromScript(script));
    else
      out.push(self.scriptRowFromScriptRepository(script));
  });
  out.push("</ul>");
  out.push("</fieldset>");
  out.push("</div>");

  out.push("</div>");

  // Inject the console
  qlPrompt({
      id: "qlhmPrompt"
    , title: self.hm.name + " <small>(v" + self.hm.version + ")</small>"
    , customWidth: self.width
    , ok: self.handleConsoleOk.bind(self)
    , okLabel: "Apply"
    , cancel: self.handleConsoleClose.bind(self)
    , cancelLabel: "Close"
    , body: out.join("")
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

HudManager.prototype.showDetails = function(elem) {
  var self = this
    , $details = $("#scriptDetails")
    ;

  $("#userscripts li").removeClass("selected");
  $details.empty();

  if (!elem) {
    if (self.selectedScriptElement && $(self.selectedScriptElement).length) {
      elem = self.selectedScriptElement;
    }
    else {
      $details.append("(click on a script to see its details)");
      self.selectedScriptElement = null;
      return;
    }
  }

  var $elem = $(elem)
    , id = $elem.closest("li").data("id")
    , cacheScript = storage.scripts.cache[id]
    , repoScript = $.grep(HOOK_MANAGER.userscriptRepository, function(item) { return item.id == id; })[0]
    ;

  var author, version, descr, entrySource, deleteCaption;

  self.selectedScriptElement = elem;
  $elem.addClass("selected");

  if (cacheScript) {
    author = e(cacheScript.headers.author);
    version = e(cacheScript.headers.version);
    descr = e(cacheScript.headers.description);
    entrySource = "User Installation";
    deleteCaption = $("#userscript"+id).data("toDelete") ? "UNDELETE" : "DELETE";
  }
  else {
    author = repoScript.author;
    version = repoScript.version || "<i>not installed</i>";
    descr = repoScript.description || "";
    entrySource = "QLHM Repository";
    deleteCaption = "";
  }

  // Always add the repository note if available
  if (repoScript && repoScript.note)
    descr = descr + (!descr ? "" : "<br><br>") + "<b>QLHM NOTE:</b><br>" + repoScript.note;

  $details.append("<div class='table'>"
    + "<div class='row'>"
    + "<div class='cell'><b>Script ID:</b></div>"
    + "<div class='cell'><a href='https://userscripts.org/scripts/show/" + id + "' target='_empty'>" + id + "</a></div>"
    + "</div>"
    + "<div class='row'><div class='cell'><b>Author:</b></div><div class='cell'>" + author + "</div></div>"
    + "<div class='row'><div class='cell'><b>Version:</b></div><div class='cell'>" + version + "</div></div>"
    + "<div class='row'><div class='cell'><b>Listed Due To:</b></div><div class='cell'>" + entrySource + "</div></div>"
    + "</div>"
    + "<br>" + (descr ? ("<p>" + descr + "</p><br>") : "")
  );

  if (cacheScript) {
    $details.append("<a href='javascript:void(0)' data-id='" + id + "' class='del'>[" + deleteCaption + "]</a>"
      + " &nbsp; <a href='javascript:void(0)' data-id='" + id + "' class='viewSource'>[SOURCE]</a>"
    );
  }
  else {
    $details.append("<span class='italic'>Mark the checkbox and hit \"Apply\" to install.</span>");
  }

  $details.find(".viewSource").click(function() {
    // Open a prompt to show the selected userscript's source
    self.showSource($(this).data("id"));
  });

  $details.find(".del").click(function() {
    // Toggle a userscript being marked as deleted
    var $this = $(this)
      , id = $this.data("id")
      , $item = $("#userscript" + id)
      , toDelete = $item.data("toDelete")
      ;

    if (toDelete) {
      $item.data("toDelete", false).find("label").removeClass("strike");
      $this.text("[DELETE]");
    }
    else {
      $item.data("toDelete", true).find("label").addClass("strike");
      $this.text("[UNDELETE]");
    }
  });
};

HudManager.prototype.showSource = function(aScriptID) {
  var self = this;
  qlPrompt({
      id: "qlhmSource"
    , title: "Script Source Code: " + aScriptID
    , alert: true
    , customWidth: self.width - 100
    , customHeight: 850
    , body: "<b>NOTE:</b> Currently read-only</p>"
          + "<textarea class='userscript-source' rows='30'></textarea>"
  });

  setTimeout(function() {
    $("#qlhmSource .userscript-source").text(self.hm.getUserScriptSource(aScriptID))
  }, 0);
}

HudManager.prototype.handleConsoleOk = function() {
  var self = this
    , $con = $("#qlhm_console")
    , $uNew = $con.find("input.userscript-new")
    , ids = $uNew.val()
    ;

  // delete or disable scripts
  $con.find("input.userscript-state").each(function() {
    var $input = $(this)
      , $item = $input.closest("li")
      , id = $item.data("id")
      ;

    // Should this userscript be deleted
    if ($item.data("toDelete")) {
      if (storage.scripts.enabled[id]) {
        webReloadRequired = true;
      }

      self.hm.removeUserScript(id);

      // only remove non-repository scripts from UI
      if (0 === $.grep(HOOK_MANAGER.userscriptRepository, function(item) { return item.id == id; }).length) {
        $item.remove();
      }
      else {
        $item.find("label").removeClass("strike");
        $item.find("a").addClass("notInstalled");
      }

      $input.attr("checked", false);
      $item.data("toDelete", false);
    }
    // ... otherwise just check if disabled or enabled
    else if (self.hm.hasUserScript(id)) {
      webReloadRequired |= self.hm.toggleUserScript(id, $input.prop("checked"));
    }
  });

  // add new scripts
  ids = ids.replace(/https:\/\/userscripts\.org\/scripts\/[a-z]+\//g, "").replace(/[^\d,]/g, "");
  ids = ids.split(",").map(function(id) { return id.trim(); });

  $con.find("input.userscript-state").each(function() {
    var $input = $(this)
      , $item = $input.closest("li")
      , id = $item.data("id")
      ;

    if ($input.prop("checked") && !self.hm.hasUserScript(id)) {
      ids.push(id);
    }
  });

  $.each(ids, function(i, id) {
    // New userscript?
    if (id) {
      if (self.hm.hasUserScript(id)) {
        log("The userscript with ID " + id + " already exists.  Try removing it first.");
      }
      else {
        log("Trying to fetch userscript with ID '" + id + "'");
        var $script = $("#userscript" + id);
        $script.find("a").removeClass("notInstalled");
        $script.find(":checkbox").prop("checked", true);
        self.hm.fetchScript(id, function(aScript) {
          // TODO: manage the userscript list better... this won't necessarily be in the correct position
          if (0 === $("#userscript" + id).length)
            $con.find("#userscripts").append(self.scriptRowFromScript(aScript));
          self.showDetails();
        });
      }
    }
  });

  $uNew.val("");


  if (webReloadRequired) {
    //$("#modal-buttons").append("<span style='color:#c00000; font-size: 12pt'> ... and reload website</span>");
    $("#modal-cancel").prop("value", "Restart");
  }

  self.showDetails();
}

HudManager.prototype.handleConsoleClose = function() {
  if (webReloadRequired) {
    qz_instance.SendGameCommand("web_reload");
  }
  else {
    $("#qlhmPrompt").jqmHide();
  }
}

/**
 * Hook Manager
 */
function HookManager(aProps) {
  readOnly(this, "name", "Quake Live Hook Manager");
  readOnly(this, "version", 0.5);
  readOnly(this, "debug", !!aProps.debug);
}

HookManager.prototype.init = function() {
  log("^2Initializing " + this.name + " v" + this.version);

  $ = aWin.jQuery;

  if (this.debug) {
    debug("^3DEBUG ENABLED.  Press F12 to open Firebug Lite.");
    // Firebug Lite (F12 to open)
    $("body").append("<script type='text/javascript' src='https://getfirebug.com/firebug-lite.js'>");
  }

  readOnly(this, "hud", new HudManager(this));
  storage.init(this.initScripts.bind(this), false);
  setTimeout(this.versionCheck.bind(this), 5E3);
}

HookManager.prototype.initScripts = function() {
  $.ajax({url:config.EXTRAQL_URL + "scripts/extraQL.js", dataType:"script", timeout:1000})
    .done(this.initExtraQL.bind(this))
    .fail(function() { log("Using ^3QLHM^7 repository"); })
    .always(this.loadScripts.bind(this));
}

HookManager.prototype.initExtraQL = function() {
  log("Using ^3extraQL^7 script repository");
  USERSCRIPT_REPOSITORY_URL = config.EXTRAQL_URL + "qlhmUserscriptRepository.js";
  JSONP_PROXY_TEMPLATE = config.EXTRAQL_URL + "uso/{{id}}";

  // after first installation, when no scripts are available yet, activate all scripts
  try {
    var enableAllScripts = true;
    $.each(storage.scripts.available, function() {
      enableAllScripts = false;
    });
    if (!enableAllScripts)
      return;

    var js = $.ajax({
      url: USERSCRIPT_REPOSITORY_URL,
      dataType: "html",
      async: false
    });
    eval(js); // this will set HOOK_MANAGER.userscriptRepository
    $.each(HOOK_MANAGER.userscriptRepository, function(index, script) {
      storage.scripts.enabled[script.ID] = true;
    });
  } catch(ex) {}
}

HookManager.prototype.versionCheck = function() {
  var self = this;
  $.ajax({
      url: VERSION_CHECK_URL
    , data: {currentVersion: self.version}
    , dataType: "jsonp"
  }).done(function(data) {
    if (data.new) {
      log("New version of " + self.name + " found: " + data.new.version);
      var out = "A new version (" + data.new.version + ") of " + self.name + " is available @ <a href='"
              + data.new.url + "' target='_blank'>" + data.new.url + "</a>.<br><br>You will need to manually update your "
              + "\"hook.js\" file, which is currently at version " + self.version + ".";
      self.hud.alert({
          title: self.name + " Update Available"
        , body: out
      });
    }
    else {
      log("On the latest (or newer) " + self.name + " client release");
    }
  });
}

HookManager.prototype.loadScripts = function() {
  var self = this;

  // get sorted list of enabled script IDs (to make execution order a bit less random)
  var scriptIds = [];
  $.each(storage.scripts.enabled, function(scriptID, enabled) {
    if (enabled) scriptIds.push(scriptID);
  });
  scriptIds.sort();

  // Fire off requests for each script
  $.each(scriptIds, function (i, scriptID) {
    var script = storage.scripts.cache[scriptID];

    // TODO: re-enable loading from cache once expiration stuff is in place...
    var USE_CACHE = false;

    // Serve from cache?
    if (USE_CACHE && script) {
      self.injectUserScript(script);
    }
    // ... or pull fresh data
    else {
      log("^7Requesting userscript ^5" + scriptID + "^7");
      self.fetchScript(scriptID);
    }
  });

  // User-specified scripts
  $.each(config.manual, function(i, scriptURL) {
    log("^7Requesting userscript ^5" + scriptURL + "^7");
    $.ajax({
      url: scriptURL
    , dataType: "jsonp"
    }).done(function(aData) {
      injectScript(";(function() {" + aData + "})();");
    });
  });
}

HookManager.prototype.fetchScript = function(aScriptID, aCB) {
  var self = this
    , handleScriptSuccess = this.handleScriptSuccess.bind(this)
    ;

  $.ajax({
      url: JSONP_PROXY_TEMPLATE.replace("{{id}}", aScriptID)
    , headers: {"Accept-Version": "~1"}
    , dataType: "jsonp"
  })
  .done(function(aData) {
    if (aCB) setTimeout(function() { aCB.call(null, aData); }, 0);
    handleScriptSuccess(aData);
  })
  .fail(self.handleScriptError.bind(self, aScriptID));
}

HookManager.prototype.handleScriptSuccess = function(aScript) {
  this.parseScriptHeader(aScript);
  this.addUserScript(aScript);
}

HookManager.prototype.handleScriptError = function(aScriptID, jqXHR, settings, err) {
  log("^1Failed to retrieve script with ID ^5" + aScriptID + "^1 : ^7" + err);
}

HookManager.prototype.parseScriptHeader = function (aScript) {
  // this code is not necessary ATM, but will support direct loading of a script from a URL without a separate .meta.js
  var script = aScript.content;
  var headers = aScript.headers;
  var start = script.indexOf("// ==UserScript==");
  var end = script.indexOf("// ==/UserScript==");
  var headerReset = {};

  if (start >= 0 && end >= 0) {
    var regex = new RegExp("^\\s*//\\s*@(\\w+)\\s+(.*?)\\s*$");
    script.substring(start, end).split("\n").forEach(function (line) {
      var match = line.match(regex);
      if (!match) return;
      var key = match[1];
      var value = match[2].trim();
      if (!(key in headers) || !headerReset[key]) {
        headers[key] = [value];
        headerReset[key] = true;
      }
      else {
        headers[key].push(value);
      }
    });
  }
}

HookManager.prototype.hasUserScript = function(aID) {
  return storage.scripts.available[aID.toString()];
}

HookManager.prototype.addUserScript = function(aScript) {
  var id = aScript._meta.id.toString();
  // Only add entries if this is a new script...
  storage.scripts.available[id] = true;
  storage.scripts.enabled[id] = true;

  storage.scripts.cache[id] = aScript;
  storage.save();
  this.injectUserScript(aScript);
}

HookManager.prototype.removeUserScript = function(aID) {
  aID = aID.toString();
  var name;

  if (!this.hasUserScript(aID)) return false;
  name = storage.scripts.cache[aID].headers.name[0];
  delete storage.scripts.available[aID];
  delete storage.scripts.enabled[aID];
  delete storage.scripts.cache[aID];

  storage.save();

  log("^5" + aID + "^7: ^3" + name + "^7 has been removed, but you must restart QUAKE LIVE for the change to take effect.");

  return true;
}

HookManager.prototype.toggleUserScript = function(aID, aEnable) {
  // return true if web_reload is required to make the change take effect
  var enable = true === aEnable ? aEnable : false
    , script = storage.scripts.cache[aID]
    , name
    ;

  if (!script) return false;
  name = cleanupName(script.headers.name[0]);

  if (enable && !storage.scripts.enabled[aID]) {
    storage.scripts.enabled[aID] = true;
    storage.save();
    this.injectUserScript(script);
    log("^7'^5" + name + "^7' has been enabled and injected.  You might need to restarted QUAKE LIVE to get the expected behaviour.");
    return false;
  }
  else if (!enable && storage.scripts.enabled[aID]) {
    delete storage.scripts.enabled[aID];
    storage.save();
    log("^7'^5" + name + "^7' has been disabled, but you must restart QUAKE LIVE for the change to take effect.");
    return true;
  }
  return false;
}

HookManager.prototype.injectUserScript = function(aScript) {
  log("^7Starting userscript ^5" + aScript._meta.id + "^7: ^3" + cleanupName(aScript.headers.name[0]) + "^7");
  var closure = ";(function() {" + aScript.content + "\n})();";

  // inject script file when possible to preserve file name in log and error messages
  if (aScript._meta.filename && aScript.headers["unwrap"] !== undefined && extraQL && extraQL.isServerRunning()) {
    var url = config.EXTRAQL_URL + "scripts/" + aScript._meta.filename;
    $.ajax({ url: url, dataType: "script", timeout: 1000 }).fail(function () { injectScript(closure); });
  }
  else {
    injectScript(closure);
  }
}

HookManager.prototype.getUserScript = function(aScriptID) {
  return storage.scripts.cache[aScriptID];
}

HookManager.prototype.getUserScriptSource = function(aScriptID) {
  var script = this.getUserScript(aScriptID);
  if (!script) return;
  return script.content;
}

HookManager.prototype.addMenuItem = function(aCaption, aHandler) {
  this.hud.addMenuItem(aCaption, aHandler);
}

// Make init and addMenuItem available
var hm = new HookManager({debug: config.debug});
aWin.HOOK_MANAGER = {
  init: hm.init.bind(hm),
  addMenuItem: hm.addMenuItem.bind(hm)
};

})(window);
