// ==UserScript==
// @id             qlAltBrowser
// @name           Quake Live Alt Browser
// @version        0.16
// @description    A different Quake Live server browser with filtering and other tweaks
// @namespace      phob.net
// @author         wn
// ==/UserScript==


(function($) {

/**
 * Common/config
 */
var config = {
    name: {
        short: "QLAB"
      , long: "Quake Live Alt Browser"
    }
  , debug: false
  , list: {
        width: 639 // px
    }
  , filters: {
        include: {id: "qlab-filter-include"}
      , exclude: {id: "qlab-filter-exclude"}
    }
};


/**
 * Utils
 */
function log() {
  var args = Array.prototype.slice.call(arguments);
  if (!args.length) return;
  if (console.firebuglite) console.log.apply(console, args);
  qz_instance.SendGameCommand("echo ^2" + config.name.short + " INFO: ^7" + (1 === args.length ? args[0] : JSON.stringify(args)));
}

function debug() {
  if (!config.debug) return;
  var args = Array.prototype.slice.call(arguments);
  if (!args.length) return;
  if (console.firebuglite) console.debug.apply(console, args);
  qz_instance.SendGameCommand("echo ^3" + config.name.short + " DEBUG: ^7" + (1 === args.length ? args[0] : JSON.stringify(args)));
}

function addStyle() {
  var args = Array.prototype.slice.call(arguments);
  $("body").append("<style type='text/css'>" + args.join("\n") + "</style>");
}

function fullRefresh() {
  quakelive.serverManager.FlushCache();
  quakelive.mod_home.ReloadServerList();
  return false;
}


/**
 * Enable debugging stuff if needed
 */
if (config.debug) {
  debug("Injecting Firebug Lite");
  $("body").append("<script type='text/javascript' src='https://getfirebug.com/firebug-lite.js'>");
}


/**
 * Styles
 */
addStyle(
    "#qlab-filterbar { width: "+config.list.width+"px; display: inline-block; background: rgba(204, 204, 204, 0.35); margin-bottom: 4px; }"
  , "#qlab-filterbar:hover { background: rgba(204, 204, 204, 0.8); }"
  , "#qlab-filterbar > span { width: "+config.list.width+"px; display: inline-block; padding: 4px 0 4px 4px; cursor: pointer; color: #B4260A; font-weight: bold; font-family: HandelGothic; font-size: 12pt; }"
  , "#qlab-filterbar > .qlab-filter-container { display: none; background: white; color: black; padding: 4px; }"
  , "#qlab-filterbar > .qlab-filter-container > .filter { display: block; width: 100%; margin-bottom: 4px; }"
  , "#qlab-filterbar > .qlab-filter-container > .filter > label { display: inline-block; width: 50px; }"
  , "#qlab-filterbar > .qlab-filter-container > .filter > input { width: 90%; border: 1px solid #ccc; }"
  , "#qlab-filterbar > .qlab-filter-container > .qlab-help a { color: red; }"
  , "#qlab-help-prompt #modal-content { text-align: left; }"
  , "#qlab-help-prompt #modal-content strong { font-weight: bold; }"
  , "#qlab-help-prompt #modal-content code { display: block; padding: 5px; }"
  , "#qlab-help-prompt #modal-content ul { margin: 10px 0; padding-left: 20px; list-style: disc outside none; }"
  , "#qlab-customColumns-prompt .column { display: inline-block; padding: 5px; margin: 0 5px; border: 1px solid #ccc; font-weight: bold; background: #eee; color: #000; cursor: col-resize !important; }"
  , "#qlv_postlogin_matches > table { width: "+config.list.width+"px; color: black; background-color: rgba(255, 255, 255, 0.4); }"
  , "#qlv_postlogin_matches > table > thead > tr { background: black; }"
  , "#qlv_postlogin_matches > table > thead > tr > th { padding: 4px 0; cursor: pointer; color: white !important; font-weight: bold; text-shadow: 1px 1px 5px #666; filter: dropshadow(color=#666, offx=1, offy=1); }"
  , "#qlv_postlogin_matches > table > thead > tr .qlab-sort-asc { text-shadow: 1px 1px 5px #fb4; filter: dropshadow(color=#fb4, offx=1, offy=1); }"
  , "#qlv_postlogin_matches > table > thead > tr .qlab-sort-desc { text-shadow: 1px 1px 5px #f00; filter: dropshadow(color=#f00, offx=1, offy=1); }"
  , "#qlv_postlogin_matches > table > tbody > tr > td { cursor: default; }"
  , ".qlab-server-row { background-color: rgba(255, 255, 255, 0.35); }"
  , ".qlab-server-row:hover { background-color: rgba(255, 255, 255, 0.8); }"
  , ".qlab-server-row:nth-of-type(odd) { background-color: rgba(204, 204, 204, 0.35); }"
  , ".qlab-server-row:nth-of-type(odd):hover { background-color: rgba(204, 204, 204, 0.8); }"
  , ".qlab-server-row.selected { background-color: rgba(180, 38, 10, 0.35); }"
  , ".qlab-server-row.selected:hover { background-color: rgba(180, 38, 10, 0.8); }"
  , ".qlab-server-row.selected:hover .qlab-hostname { color: #ccc; }"
  , ".qlab-col-location { width: 130px; padding-left: 2px !important; }"
  , ".qlab-col-ping { width: 50px; }"
  , ".qlab-col-gametype { width: 32px; }"
  , ".qlab-col-gametype .qlab-gametype-icon { height: 16px; }"
  , ".qlab-col-main {}"
  , ".qlab-col-main .qlab-hostname { color: #666; }"
  , ".qlab-col-icons {}"
  , "th.qlab-col-icons img[data-sort=passworded] { -webkit-filter: invert(100%); }"
  , ".qlab-col-icons > img { display: inline-block; height: 14px; width; 14px; padding: 0 1px; }"
  , ".qlab-col-players { text-align: center; }"
  , ".hidden { visibility: hidden; }"
);


/**
 * Common strings
 */
// Help text
var TMPL_help = [
    "<p>Enter comma-delimited values (partial or complete) that servers must meet to be included or excluded.  Supported value types include:</p>"
  , "<ul>"
  ,   "<li><b>Location:</b> 'New York', 'Frankfurt', 'Russia', etc. (country or city; hover over column)</li>"
  ,   "<li><b>Map name:</b> 'Campgrounds', 'Blood Run', etc.</li>"
  ,   "<li><b>Game mode:</b> 'ffa', 'ctf', 'clan arena', etc. (hover over icons)</li>"
  ,   "<li><b>Server size:</b> '/16', '/4', etc.</li>"
  ,   "<li><b>Ranking:</b> 'ranked', 'unranked'</li>"
  ,   "<li><b>Skill level:</b> 'Unrestricted', 'Skill Matched', etc. (hover over icons)</li>"
  ,   "<li><b>Passworded:</b> 'password', 'passworded', 'password protected'</li>"
  ,   "<li><b>Modifications:</b> 'ruleset: classic', 'ruleset: turbo', 'mods', 'modifications', 'air control', 'headshots', etc. (hover over icons)</li>"
  ,   "<li><b>Special keywords:</b> 'qlab.open' (playable slots available)</li>"
  , "</ul>"
  , "<p>It is possible to combine keywords to make a filter segment more specific.  For example:</p>"
  , "<p><code>New York + Campgrounds, Warsaw + qlab.open, ffa, /16</code><p>"
  , "<p>The filter above matches New York servers on Campgrounds <strong>OR</strong> Warsaw servers with playable slots <strong>OR</strong> FFA servers <strong>OR</strong> servers with 16 slots.</p>"
].join("\n");


/**
 * Custom column stuff
 */
var colDisplay = {
    "location": "Location"
  , "ping": "Ping"
  , "gametype": "Gametype"
  , "main": "Map (Hostname)"
  , "icons": "Details"
  , "players": "Players"
};
var defaultColOrder = ["location", "ping", "gametype", "main", "icons", "players"];
var _rowTemplate = _headerRow = undefined;
var _colOrder = defaultColOrder;
try {
  _colOrder = JSON.parse(localStorage["qlab-customCols"]);
}
catch(e) {}

HOOK_MANAGER.addMenuItem("Custom Columns", function() {
  var body = "<div class='columns'>";
  for (var i = 0, e = _colOrder.length; i < e; ++i) {
    body += "<span class='column' data-col='" + _colOrder[i] + "'>" + colDisplay[_colOrder[i]] + "</span>";
  }
  body += "<br><br><a href='#' class='reset'>reset</a></div>";

  qlPrompt({
      id: "qlab-customColumns-prompt"
    , title: config.name.long + ": Customize Columns"
    , body: body
    , alert: true
    , ok: function() {
        $("#qlab-customColumns-prompt").jqmHide();
        fullRefresh();
      }
  });

  setTimeout(function() {
    $("#qlab-customColumns-prompt .reset").on("click", function() {
      $("#qlab-customColumns-prompt").jqmHide();
      _colOrder = defaultColOrder;
      localStorage["qlab-customCols"] = JSON.stringify(_colOrder);
      _rowTemplate = _headerRow = undefined;
      // TODO: just update the column ordering, rather than doing a refresh
      fullRefresh();
      return false;
    });

    $("#qlab-customColumns-prompt .columns").dragsort({
        dragSelector: ".column"
      , placeHolderTemplate: "<span class='column'></span>"
      , dragEnd: function() {
          _colOrder = $("#qlab-customColumns-prompt .columns .column").map(function() { return $(this).data("col") }).get();
          localStorage["qlab-customCols"] = JSON.stringify(_colOrder);
          _rowTemplate = _headerRow = undefined;
        }
    });
  }, 0);
});


/**
 * AltServerListView
 *
 * NOTE: duplicated here since it isn't exposed externally in QL JS
 */
// Config specific to the view
var SHARED_DEFAULT_PROPS = {
  "targetSelector": "#qlv_postlogin_matches",
  "listSelector": "#qlv_postlogin_matches > table > tbody",
  "max": 0,
  "hide_best_pick": false,
  "group_cache_size": 6
};

// Constructor
function AltServerListView(aProps) {
  this.defaultProps = $.extend({}, SHARED_DEFAULT_PROPS, aProps);

  this.selectedServer = undefined;

  this.sortBy = localStorage["qlab-sortBy"] || "players";
  this.sortReverse = localStorage["qlab-sortReverse"] ? ("true" === localStorage["qlab-sortReverse"]) : true;
}
AltServerListView.prototype = new quakelive.ServerManagerListener();


// Allows external process to set display details
// Maybe remove?
AltServerListView.prototype.SetDisplayProps = function(aProps) {
  this.props = $.extend({}, this.defaultProps, aProps);
};

// ID of element containing the view
AltServerListView.prototype.GetContainer = function() {
  return $(this.props.targetSelector);
};

// ID of element containing server nodes
AltServerListView.prototype.GetListContainer = function() {
  return $(this.props.listSelector);
};

// Format a server node ID
AltServerListView.prototype.GetServerNodeId = function(aServer) {
  return "match_" + aServer.public_id;
};

AltServerListView.prototype.OnBeforeUpdateServerNode = function(aServer, $aNode) {};

// Given a ServerEntry object and jQuery object for a server's node, generate a data object and use
// it to re-render the server template
AltServerListView.prototype.UpdateServerNode = function(aServer, $aNode) {
  $aNode.attr("data-publicid", aServer.public_id);
  $aNode.addClass("qlab-server-row");

  var o = {
    server: aServer
  };

  o.isModified = (aServer.owner && (aServer.ruleset != 3 || aServer.g_customSettings != 0));
  o.modifiedInfo = "Server Modified:\n" + aServer.GetModifiedSettings().join("\n");

  o.mapinfo = mapdb.getBySysName(aServer.map);
  if (!o.mapinfo) {
    o.mapinfo = {
      name: aServer.map,
      sysname: aServer.map
    }
  }

  var loc = locdb.GetByID(aServer.location_id);
  o.fullLocation = loc ? (loc.country + " - " + loc.GetCityState()): "QUAKE LIVE";
  o.cityName = loc ? loc.GetCityState() : "QUAKE LIVE";
  o.flagIconURL = quakelive.resource(loc ? loc.GetFlagIcon() : "/images/flags3cc/usa.gif");
  o.hostAddress = aServer.host_address;

  o.ping = aServer.GetPing();
  o.pingQualityURL = quakelive.resource("/images/lgi/quality_" + aServer.GetPingQuality() + ".png");

  o.skill = GetSkillRankInfo(aServer.skillDelta);
  o.skill.iconURL = quakelive.resource("/images/sqranks/rank_" + o.skill.delta + ".png");

  o.gametype = mapdb.getGameTypeByID(aServer.game_type);
  o.gtIconURL = quakelive.resource("/images/gametypes/md/" + o.gametype.name + ".png");
  o.gtIconTitle = o.gametype.name.toUpperCase() + " - " + o.gametype.title;

  if (aServer.teamsize > 0) {
    o.num_players = aServer.num_players;
    o.max_size = aServer.teamsize * (o.gametype.team ? 2 : 1);
    if (o.max_size > aServer.max_clients) o.max_size = aServer.max_clients;
  }
  else {
    o.num_players = aServer.num_clients;
    o.max_size = aServer.max_clients;
  }
  o.upsell = aServer.premium && quakelive.siteConfig.premiumStatus == "standard" && !aServer.invitation;

  // Render HTML
  $aNode.html(this.GetRowTemplate().render(o));

  if (this.selectedServer == aServer.public_id) {
    $aNode.addClass("selected");
  }

  var loadOptions = {
    pacifier: "/images/levelshots/sm/default.jpg",
    resource: "/images/levelshots/sm/" + aServer.map.toLowerCase() + ".jpg",
    target: $aNode.find("img.shot")
  };
  quakelive.resourceLoader.Load(loadOptions);

  return $aNode;
};

// Unused?
AltServerListView.prototype.ServerJoin = function(aServer) {
  if (!aServer) return;
  var gameParams = new LaunchGameParams();
  gameParams.Append("+connect " + aServer.host_address);
  LaunchGame(gameParams, aServer);
};

// Called after quakelive.serverManager retrieves new server list JSON
// Kicks off re-rendering of the container's contents (list, etc.)
AltServerListView.prototype.OnRefreshServersSuccess = function(aManager, aJSON) {
  quakelive.SendModuleMessage("OnServerListReload", aManager);
  if ("undefined" !== typeof aJSON && "undefined" !== typeof aJSON.player_tier) {
    $("#debug_tier").text("[DEV] Your tier is " + aJSON.player_tier);
  }
  this.DisplayServerList();
};

// Show a "sorry" view if quakelive.serverManager either failed to retrieve new server list JSON
// or the JSON wasn't valid (i.e. malformed or missing '.servers')
// See quakelive.serverManager.RefreshServersError and quakelive.serverManager.RefreshServersSuccess
AltServerListView.prototype.OnRefreshServersError = function(aManager) {
  this.GetListContainer().empty();

  var $container = this.GetContainer();
  $container.append(
      "<p class='qlab-servers-issue refreshable tc TwentyPxTxt midGrayTxt' style='width: 80%; margin: auto; color: #f00'>"
    + "We've encountered an error loading the list of games. Click this message to try a refresh."
    + "</p>"
  );

  $container.find(".qlab-servers-issue.refreshable").click(fullRefresh);
};

// Remove a server node
// Called for each server identified needing removal
// See quakelive.serverManager.UpdateServers
AltServerListView.prototype.OnRemoveServer = function(aManager, aServer) {
  $("#" + this.GetServerNodeId(aServer)).remove();
  quakelive.matchtip.HideMatchTooltip(aServer.public_id);
};

// Add or Update a server node
// Called for each server identified needing adding/updating
// See quakelive.serverManager.UpdateServers and quakelive.serverManager.OnPing
AltServerListView.prototype.OnUpdateServer = function(aManager, aServer) {
  if (aServer.hidden) return;

  var $rowNode = $("#" + this.GetServerNodeId(aServer));
  if ($rowNode.length == 0) {
    $rowNode = $("<tr id='" + this.GetServerNodeId(aServer) + "'></tr>");
    aServer.node = $rowNode;
  }

  this.OnBeforeUpdateServerNode(aServer, $rowNode);
  this.UpdateServerNode(aServer, $rowNode);

  return $rowNode;
};

// Display the filter bar
AltServerListView.prototype.DisplayFilterBar = function($aContainer) {
  var self = this
    , $qlabFilterBar = $("#qlab-filterbar")
    ;

  if (0 === $qlabFilterBar.length) {
    var incID = config.filters.include.id
      , incPlaceholder = "Keywords servers must match".replace("'", "&apos;")
      , excID = config.filters.exclude.id
      , excPlaceholder = "Keywords servers must not match".replace("'", "&apos;")
      ;

    $qlabFilterBar = $(
        "<div id='qlab-filterbar'><span title='Shift+F'>&gt; Flexible Filter</span>"
      +   "<div class='qlab-filter-container'>"
      +     "<div class='filter'><label for='"+incID+"'>Include:</label> <input type='text' id='"+incID+"' placeholder='"+incPlaceholder+"'/></div>"
      +     "<div class='filter'><label for='"+excID+"'>Exclude:</label> <input type='text' id='"+excID+"' placeholder='"+excPlaceholder+"'/></div>"
      +     "<div class='qlab-help'><a href='javascript:;'>Help</a></div>"
      +   "</div>"
      + "</div>"
    );

    // Initialize the filters
    $qlabFilterBar.find(".filter input").each(function() {
      this.value = localStorage[this.id] || "";
    });

    // Always persist filter values
    $qlabFilterBar.on("change keyup", ".filter > input", function(aEvent) {
      localStorage[this.id] = this.value || "";
    });

    $qlabFilterBar.on("keypress", ".filter > input", function(aEvent) {
      // Hit "Enter" to reload the list with new criteria
      if (13 == aEvent.which) {
        // TODO: decide if we'll do a full list reload or just re-display... currently using re-display since it's much faster
        //fullRefresh();
        self.DisplayServerList();
        this.focus();
      }
    });

    $qlabFilterBar.on("keydown", "input, textarea", function(aEvent) {
      // Suppress backtick (99.999% intended for the QL console)
      if (192 == aEvent.which) aEvent.preventDefault();
    });

    // Toggle filterbar view
    $qlabFilterBar.on("click", "> span", function() {
      $(this).siblings("div.qlab-filter-container").slideToggle(100);
    });

    // Help view
    $qlabFilterBar.on("click", ".qlab-help a", function() {
      qlPrompt({
          id: "qlab-help-prompt"
        , title: config.name.long + " Help"
        , body: TMPL_help
        , alert: true
      });
    });
  }
  $aContainer.prepend($qlabFilterBar);
}

// Called immediately before server nodes are added (but after filter bar and list container)
AltServerListView.prototype.OnBeforeDisplayServerList = function($aContainer, aServers) {
  // Add indicator for refresh keyboard shortcut, and make it do a full refresh
  $("#matchlist_header_controls a.refresh_browser").attr("title", "Shift+R").off().on("click", fullRefresh);
};

AltServerListView.prototype.UpdateColumnHighlight = function() {
  var $thead = this.GetListContainer().parent();
  $thead.find(".qlab-sort-asc, .qlab-sort-desc").removeClass("qlab-sort-asc qlab-sort-desc");
  $thead.find("thead [data-sort="+this.sortBy+"]").addClass(this.sortReverse ? "qlab-sort-desc" : "qlab-sort-asc");
}

// Constructs (or returns cached) header row based upon user-defined column config
AltServerListView.prototype.GetHeaderRow = function() {
  if (_headerRow) return _headerRow;

  _headerRow = "<tr>";
  for (var i = 0, e = _colOrder.length; i < e; ++i) {
    switch (_colOrder[i]) {
      case "location":
        _headerRow += "<th class='qlab-col-location' data-sort='location'>Location</th>";
        break;
      case "ping":
        _headerRow += "<th class='qlab-col-ping' data-sort='ping'>Ping</th>";
        break;
      case "gametype":
        _headerRow += "<th class='qlab-col-gametype' title='Gametype' data-sort='gametype'>GT</th>";
        break;
      case "main":
        _headerRow += "<th class='qlab-col-main'><span data-sort='map'>Map</span> (<span data-sort='servername'>Hostname</span>)</th>";
        break;
      case "icons":
        // NOTE: keep single space character before icon images to match row spacing... do this in CSS at some point
        _headerRow += "<th class='qlab-col-icons'>"
        _headerRow +=  "<img data-sort='skill' src='" + quakelive.resource("/images/sqranks/rank_1.png") + "' title='Skill'/>"
        _headerRow += " <img data-sort='premium' src='" + quakelive.resource("/images/lgi/premium_icon.png") + "' title='Premium Match'/>"
        _headerRow += " <img data-sort='unranked' src='" + quakelive.resource("/images/modules/browser/unrank_icon.png") + "' title='Unranked Match'/>"
        _headerRow += " <img data-sort='modified' src='" + quakelive.resource("/images/modules/browser/modified_icon.png") + "' title='Server Modified'/>"
        _headerRow += " <img data-sort='passworded' src='" + quakelive.resource("/images/lgi/server_details_ranked.png") + "' title='Password Protected'/>"
        _headerRow += "</th>"
        break;
      case "players":
        _headerRow += "<th class='qlab-col-players' data-sort='players'>Players</th>";
        break;
    }
  }
  _headerRow += "</tr>";

  return _headerRow;
}

// Constructs (or returns cached) server row template based upon user-defined column config
// TODO: fix icon fixed block size... currently just setting img visibility to 'hidden'... seems hacky
AltServerListView.prototype.GetRowTemplate = function() {
  if (_rowTemplate) { return _rowTemplate; }

  var content = "";

  for (var i = 0, e = _colOrder.length; i < e; ++i) {
    switch (_colOrder[i]) {
      case "location":
        content += "<td class='qlab-col-location' title='<%= fullLocation %>\n<%= hostAddress %>'><img src='<%= flagIconURL %>'/> <%= cityName %></td>";
        break;
      case "ping":
        content += "<td class='qlab-col-ping'><img src='<%= pingQualityURL %>'/> <span><%= ping %></span></td>";
        break;
      case "gametype":
        content += "<td class='qlab-col-gametype'><img class='qlab-gametype-icon' src='<%= gtIconURL %>' title='<%= gtIconTitle %>'/></td>";
        break;
      case "main":
        content += "<td class='qlab-col-main'><%= mapinfo.name %> (<span class='qlab-hostname'><%= server.host_name %></span>)</td>";
        break;
      case "icons":
        content += "<td class='qlab-col-icons'>"
        content +=  "<img src='<%= skill.iconURL %>' title='<%= skill.desc %>'/>"
        content += " <img src='" + quakelive.resource("/images/lgi/premium_icon.png") + "' title='Premium Match' class='<%= server.premium ? '' : 'hidden' %>'/>"
        content += " <img src='" + quakelive.resource("/images/modules/browser/unrank_icon.png") + "' title='Unranked Match' class='<%= !server.ranked ? '' : 'hidden' %>'/>"
        content += " <img src='" + quakelive.resource("/images/modules/browser/modified_icon.png") + "' title='<%= modifiedInfo %>' class='<%= isModified ? '' : 'hidden' %>'/>"
        content += " <img src='" + quakelive.resource("/images/lgi/server_details_ranked.png") + "' title='Password Protected' class='<%= server.g_needpass ? '' : 'hidden' %>'/>"
        content += "</td>"
        break;
      case "players":
        content += "<td class='qlab-col-players'><%= num_players %>/<%= max_size %></td>";
        break;
    }
  }

  _rowTemplate = new EJS({text: content});

  return _rowTemplate;
}

// Rebuilds the entire server list container and contents
AltServerListView.prototype.DisplayServerList = function() {
  var self = this
    , $container = this.GetContainer()
    , $listContainer = this.GetListContainer()
    , servers = this.FilterServerList(quakelive.serverManager.GetServers())
    ;

  // Wipe out the list container (the table) and any "no servers found" or error messages
  $listContainer.parent().remove();
  $container.find(".qlab-servers-issue").remove();

  this.DisplayFilterBar($container);

  // Add the list container
  $container.append("<table><thead>" + this.GetHeaderRow() + "</thead><tbody></tbody></table>");

  $listContainer = this.GetListContainer();
  this.UpdateColumnHighlight();

  // Clicking table headers (and their subelements) sorts stuff
  $listContainer.parent().on("click", "thead [data-sort]", function(aEvent) {
    var $this = $(this)
      , sortType = $this.data("sort")
      ;

    self.sortReverse = (sortType === self.sortBy) ? !self.sortReverse : ("players" === sortType);
    localStorage["qlab-sortReverse"] = self.sortReverse;

    self.sortBy = sortType;
    localStorage["qlab-sortBy"] = self.sortBy;

    self.UpdateColumnHighlight();

    self.SortServerList();
    return false;
  });

  this.OnBeforeDisplayServerList($container, servers);

  // If there are servers...
  if (servers.length > 0) {
    var self = this;
    var cachedServerIds = [];
    var cacheCtr = 0;

    // Chunk up server array into cache groups for updating
    // Working as intended?  Seems to result in quakelive.serverManager.RefreshServerDetails
    // getting [[public_id],[public_id]] rather than [public_id,public_id] of all server IDs
    for (var i = 0; i < servers.length; i += this.props.group_cache_size) {
      var tCache = [];
      for (var g = 0; g < this.props.group_cache_size && i + g < servers.length; g++) {
        var server = servers[i + g];
        tCache[g] = server.public_id;
        cachedServerIds[cacheCtr] = tCache
      }
      cacheCtr++
    }

    var maxDisplay = this.props.max == 0 ? servers.length : this.props.max;
    for (var i = 0; i < maxDisplay; ++i) {
      var server = servers[i];

      server.node.on("mouseenter", function() {
        var public_id = this.getAttribute("data-publicid");
        return self.OnMatchHover(this, public_id);
      });

      server.node.on("mouseleave", function() {
        var public_id = this.getAttribute("data-publicid");
        return self.OnMatchHoverOff(this, public_id);
      });

      // Clicking a row toggles displaying server details in the match column
      server.node.on("click", function() {
        var public_id = this.getAttribute("data-publicid");

        $listContainer.find(".qlab-server-row.selected").removeClass("selected");

        // Clicking previously-selected?
        if (public_id === self.selectedServer) {
          self.selectedServer = undefined;
          $("#browser_details").empty();
        }
        else {
          self.selectedServer = public_id;

          var OnRefreshServerDetailsSuccess = function(aServer, aIsCached) {
            // NOTE: RenderMatchDetails requires the first argument be the actual element
            quakelive.matchcolumn.RenderMatchDetails($("#browser_details")[0], aServer);
          };
          var OnRefreshServerDetailsError = function(aServer) {
            $("#browser_details").text("We're sorry, but we cannot load the data for this match.");
          };

          // Update the selected server's details
          // The request includes a chunk (or all) of other servers to update (NOTE: disabled for now... long delay)
          quakelive.serverManager.RefreshServerDetails(public_id, {
            "onSuccess": OnRefreshServerDetailsSuccess,
            "onError": OnRefreshServerDetailsError,
            "cacheTime": self.MATCH_CACHE_TIME, // NOTE: this is undefined... revisit later
            "cachedServerIds": []//cachedServerIds
          });

          // Ensure the selected server row is highlighted
          $(this).addClass("selected");
        }

        return self.OnMatchClick(this, public_id);
      });

      // Clicking the "play" button triggers a server join
      server.node.on("click", ".play-button", function(aEvent) {
        var public_id = aEvent.delegateTarget.getAttribute("data-publicid");
        var server = quakelive.serverManager.GetServerInfo(public_id);
        if (!server || server.error) return false;
        if (server.skillTooHigh || server.disableJoin) return false;
        join_server(server.host_address, server);
        return self.OnMatchClick(this, public_id);
      });

      // Add the server node to the container
      $listContainer.append(server.node);
    }
  }

  this.OnAfterDisplayServerList($container, servers);
};

// Called immediately after all server nodes are added
// Displays "nothing found" view
AltServerListView.prototype.OnAfterDisplayServerList = function($aContainer, aServers) {
  if (0 !== aServers.length) return;

  var type = "No Public Matches Found";
  if (quakelive.mod_home.filter.filters.invitation_only != 0) {
    type = "No Invited Matches Found";
  }
  else if (quakelive.mod_home.filter.filters["private"] != 0) {
    type = "No Private Matches Found";
  }

  $aContainer.append(
      "<div class='qlab-servers-issue'>"
    + "  <p class='tc thirtyPxTxt sixtypxv midGrayTxt'>" + type + "</p>"
    + "  <p class='tc TwentyPxTxt midGrayTxt'>"
    + "    <a href='#' class='TwentyPxTxt midGrayTxt customize_link'>Check your filters</a> or "
    + "    <a href='#' class='TwentyPxTxt midGrayTxt reset_link'>clear all filters</a> to continue."
    + "  </p>"
    + "</div>"
  );

  $aContainer.find(".customize_link").click(function() {
    quakelive.mod_home.ToggleFilterBar();
    return false;
  });

  $aContainer.find(".reset_link").click(function() {
    quakelive.mod_home.ResetBrowserFilter();
    return false;
  });
};


/**
 * Helpers for FilterServerList
 */
var RE_comma = /\s*,\s*/
    , RE_plus = /\s*\+\s*/
    , RE_escapeMe = /([\^\$\\\/\(\)\|\?\+\*\[\]\{\}\,\.])/g
    , RE_ping = /(?:\-|ping<(\d+))/i
    ;

// SkillSettings.desc => [skillDelta]
var skills = {
    "Your Skill Too Low": [-3]
  , "Your Skill Too High": [-2]
  , "Unrestricted Match": [-1]
  , "Your Skill Higher": [0]
  , "Skill Matched": [1]
  , "More Challenging": [2]
  , "Very Difficult": [3,4]
}

// ["a +  b", "c+d", "", "e"] --> [["a","b"],["c","d], ["e"]]
function parseFilter(aFilter) {
  var subfilters = [];
  // For each subfilter...
  for (var i = 0, e = aFilter.length; i < e; ++i) {
    if (0 === aFilter[i].length) continue;
    // ... split on +'s to get components
    subfilters.push(aFilter[i].split(RE_plus));
  }
  return subfilters;
}

function getServerMax(aServer) {
  var numTeams = mapdb.getGameTypeByID(aServer.game_type).team ? 2 : 1;
  var m = aServer.max_clients;
  if (aServer.teamsize > 0) {
    m = aServer.teamsize * numTeams;
    if (m > aServer.max_clients) m = aServer.max_clients;
  }
  return m;
}

function componentMatchesServer(aComponent, aServer) {
  var RE_component = new RegExp(aComponent.replace(RE_escapeMe, "\\$1"), "i")
    , maxPlayers = getServerMax(aServer)
    ;

  // Match keyword "qlab.open", indicating the server has playable slots
  if ("qlab.open" == aComponent && aServer.num_players < getServerMax(aServer)) return true;

  // Match location
  var loc = locdb.GetByID(aServer.location_id);
  if ((loc ? (loc.country + " - " + loc.GetCityState()) : "QUAKE LIVE").match(RE_component)) return true;

  // Match gametype name or title
  var gt = mapdb.getGameTypeByID(aServer.game_type);
  var gtTitle = gt.name + " " + gt.title;
  if (gtTitle.match(RE_component)) return true;

  // Match map name
  var map = mapdb.getBySysName(aServer.map.toLowerCase());
  map = map ? map.name : "Unknown";
  if (aServer.map.match(RE_component) || map.match(RE_component)) return true;

  // Match premium
  if (aServer.premium && "premium" === aComponent) return true;

  // Match ranked/unranked
  if (aServer.ranked && "ranked" === aComponent) return true;
  if (!aServer.ranked && "unranked" === aComponent) return true;

  // Match password(ed)
  if (aServer.g_needpass && ("password" === aComponent || "passworded" === aComponent || "password protected" === aComponent)) return true;

  // Match skill/rank
  for (var s in skills) {
    if (s.match(RE_component)) {
      for (var i = 0, e = skills[s].length; i < e; ++i) {
        if (aServer.skillDelta === skills[s][i]) return true;
      }
    }
  }

  // Match a specific modification or special modification keywords
  var usesMods = !!aServer.owner && (1 !== aServer.ruleset || 0 !== aServer.g_customSettings);
  if (usesMods) {
    var modsFilter = aServer.GetModifiedSettings().slice(0);
    modsFilter.push("mods"); // special keyword
    modsFilter.push("modifications"); // special keyword
    modsFilter = modsFilter.join("\t");
    if (modsFilter.match(RE_component)) return true;
  }

  // Match hostname
  if (aServer.host_name.match(RE_component)) return true;

  // Match "/X", indicating server has max player count of X
  if ("/" + maxPlayers === aComponent) return true;

  // Match "ping<X", indicating user has a ping less (or equal to... <=) X
  // NOTE: this assumes the server manager's "no ping value" indicator is "-", and should be included
  // NOTE 2: odd behavior due to availability of ping data... commenting out for now
  /*var ping = aServer.GetPing();
  var p = RE_ping.exec(aComponent);
  if (null !== p) {
    if ("-" === ping) return true;
    return (ping <= parseInt(p[1]));
  }*/

  // Otherwise no match
  return false;
}

function serverMatchesFilter(aServer, aFilter) {
  // Check each subfilter (each in format ["keyword1","keyword2"])
  for (var i = 0, e = aFilter.length; i < e; ++i) {
    // Assume a match unless one of this subfilter's components doesn't match
    var subFilterMatched = true;

    // Check each of its components (e.g. "keyword1") for a match
    for (var x = 0, y = aFilter[i].length; x < y; ++x) {
      if (0 === aFilter[i][x].length) continue;
      if (!componentMatchesServer(aFilter[i][x], aServer)) {
        subFilterMatched = false;
        break;
      }
    }

    // Return if the subfilter was completely matched
    //showServer = showServer || submatch;
    if (subFilterMatched) return true;
  }

  return false;
}


// Filters servers to those matching the include, and not matching the exclude, criteria
AltServerListView.prototype.FilterServerList = function(aServers) {
  var includeTxt = (localStorage[config.filters.include.id] || "").trim().toLowerCase()
    , excludeTxt = (localStorage[config.filters.exclude.id] ||  "").trim().toLowerCase()
    ;

  // Filter format:  keyword1, keyword2 + keyword3, , keyword5 + keyword6 + keyword7
  var includes = parseFilter(includeTxt.split(RE_comma))
    , excludes = parseFilter(excludeTxt.split(RE_comma))
    ;

  // TODO: mark servers as filtered (or not), invalidating the markers when include/exclude filters change
  return aServers.filter(function(aServer) {
    // If we have an exclude filter that matches we always hide the server
    if (excludes.length && serverMatchesFilter(aServer, excludes)) return false;
    // If we have an include filter the server must match it
    if (includes.length) return serverMatchesFilter(aServer, includes);
    // Default is to show the server
    return true;
  });
}

/**
 * Sorters for SortServerList
 */
function locationSort(a, b) {
  a = locdb.GetByID(a.location_id); a = a.country + " " + a.GetCityState();
  b = locdb.GetByID(b.location_id); b = b.country + " " + b.GetCityState();
  return a > b ? 1 : (a < b ? -1 : 0);
}
function pingSort(a, b) {
  a = ("-" === a.ping || "undefined" === typeof a.ping) ? 999 : a.ping;
  b = ("-" === b.ping || "undefined" === typeof b.ping) ? 999 : b.ping;
  return b - a;
}
function skillSort(a, b) { return b.skillDelta - a.skillDelta; }
function premiumSort(a, b) { return b.premium - a.premium; }
function rankedSort(a, b) { return b.ranked - a.ranked; }
function passwordedSort(a, b) { return b.g_needpass - a.g_needpass; }
function modifiedSort(a, b) {
  var aIsModified = (!!a.owner && (a.ruleset != 1 || a.g_customSettings != 0));
  var bIsModified = (!!b.owner && (b.ruleset != 1 || b.g_customSettings != 0));
  return bIsModified - aIsModified;
}

// Sort (or delegate sorting of) the servers array
// Called by quakelive.serverManager.SortServerList
AltServerListView.prototype.SortServerList = function() {
  var letMgrHandle = [/*"location",*/ /*"ping",*/ "gametype", "map", "servername", "players"];

  // If we delegated sorting the server manager will make a call to our SortServerList.  To avoid
  // getting into a bad state we set a flag to ignore that call. Hacky, but gets the job done.
  if (this.ignoreNextSortServerList) {
    this.ignoreNextSortServerList = false;
    return;
  }

  // Delegate sorting of the servers to the server manager?
  if (-1 !== letMgrHandle.indexOf(this.sortBy)) {
    this.ignoreNextSortServerList = true;

    quakelive.serverManager.sortBy = this.sortBy;
    quakelive.serverManager.sortReverse = this.sortReverse;
    quakelive.serverManager.SortServerList();
    quakelive.serverManager.sortBy = undefined;
    quakelive.serverManager.sortReverse = undefined;

    this.SortServerListNodes();
    return;
  }

  // Custom sorting
  var servers = quakelive.serverManager.GetServers()
    , sorter
    ;

  switch (this.sortBy) {
    case "location": sorter = locationSort; break;
    // NOTE: quakelive.Manager has ping sorting, but currently assumes numeric values
    case "ping": sorter = pingSort; break;
    case "skill": sorter = skillSort; break;
    case "premium": sorter = premiumSort; break;
    case "unranked": sorter = rankedSort; break;
    case "passworded": sorter = passwordedSort; break;
    case "modified": sorter = modifiedSort;
  }

  servers.sort(sorter);
  if (this.sortReverse) servers.reverse();

  this.SortServerListNodes();
}

// Detaches and appends all servers in the order the manager has them
AltServerListView.prototype.SortServerListNodes = function() {
  var $list = this.GetListContainer();
  var servers = quakelive.serverManager.GetServers();
  for (var i = 0; i < servers.length; i++) {
    $("#" + this.GetServerNodeId(servers[i])).detach().appendTo($list);
  }
}

// 
AltServerListView.prototype.OnMatchHover = function(aNode, aPublicID) {};
AltServerListView.prototype.OnMatchHoverOff = function(aNode, aPublicID) {};
AltServerListView.prototype.OnMatchClick = function() {};
AltServerListView.prototype.OnMatchDoubleClick = function() {};


/**
 * Wrap quakelive.mod_home.SetBrowserView to inject our own list view when applicable
 */
var oldSetBrowserView = quakelive.mod_home.SetBrowserView;
quakelive.mod_home.SetBrowserView = function(aView) {
  if (aView instanceof quakelive.ServerListView) {
    aView = new AltServerListView();
  }
  oldSetBrowserView.call(quakelive.mod_home, aView);
}


/**
 * Keypress event handling
 */
var inputTags = {"INPUT": true, "TEXTAREA": true};
$(document).keypress(function(aEvent) {
  // Only interested in the home module
  if (quakelive.mod_home !== quakelive.activeModule) return;

  // Events where we ignore input elements...
  if (!inputTags[aEvent.target.tagName]) {
    // shift+R to do a full server list refresh
    if (82 === aEvent.which) {
      fullRefresh();
    }
    // shift+F to activate the filter list
    else if (70 === aEvent.which) {
      var wasVisible = $("#"+config.filters.include.id+":visible").length;
      $("#qlab-filterbar > span").click();
      if (!wasVisible)
        setTimeout(function() { $("#"+config.filters.include.id).focus(); }, 100);
      else
        $(document).focus();
    }
  }
});


/**
 * Keyup event handling
 */
$(document).keyup(function(aEvent) {
  // Only interested in the home module
  if (quakelive.mod_home !== quakelive.activeModule) return;

  // Events where we ignore input elements...
  if (inputTags[aEvent.target.tagName]) {
    // Escape blurs the input
    if (27 === aEvent.which) $(aEvent.target).blur();
  }
});

})(jQuery);
