// ==UserScript==
// @name        Quake Live Game Type Picker
// @version     1.3
// @author      PredatH0r
// @description	Select game-type and other server filters without opening the customization area
// @include     http://*.quakelive.com/*
// @exclude     http://*.quakelive.com/forum*
// @unwrap
// ==/UserScript==

/*
  This script replaces the large "Public Matches" caption on the server browser page 
  with quick customization items for gametype and match visibility

  Version 1.3
  - updated extraQL script url to sourceforge

  Version 1.1
  - added "Show filter panel" link
*/

(function() {
  // external variables
  var quakelive = window.quakelive;
  var extraQL = window.extraQL;

  var _GAME_TYPES = [
    ["any", "any", 'tdm', 'All', 'All Game Types'],
    [2, 8, 'ffa', 'FFA', 'Free For All'],
    [4, 14, 'ca', 'CA', 'Clan Arena'],
    [7, 7, 'duel', 'Duel', 'One On One'],
    [6, 11, 'tdm', 'TDM', 'Team Deathmatch'],
    [3, 9, 'ctf', 'CTF', 'Capture The Flag'],
    [5, 10, 'ft', 'FT', 'Freeze Tag'],
    [16, 21, 'fctf', '1CTF', '1-Flag CTF'],
    [18, 23, 'ad', 'A&D', 'Attack & Defend'],
    [15, 20, 'dom', 'DOM', 'Domination'],
    [17, 22, 'harvester', 'HAR', 'Harvester'],
    [19, 24, 'rr', 'RR', 'Red Rover'],
    [25, 25, 'race', 'Race', 'Race Mode']
  ];
  var gameTypes = []; // transformed version of _GAME_TYPES: array of { index, regular, instagib, icon, text, hint }

  var currentGameTypeIndex = 0;
  var isInstagib = 0;

  function init() {
    if (extraQL.isOldUi) {
      extraQL.log("Sorry, but gametype.js is not compatible with this version of the QL UI.");
      return;
    }
      
    $.each(_GAME_TYPES, function(i, gameType) {
      gameTypes.push({ index: i, regular: gameType[0], instagib: gameType[1], icon: gameType[2], text: gameType[3], hint: gameType[4] });
    });
    extraQL.addStyle(
      "#gameTypeSwitcher { color: black; margin-left: 12px; font-family: Arial; display: inline-block; }",
      "#gameTypeSwitcher1 .gametype { display: inline-block; margin-right: 16px; }",
      "#gameTypeSwitcher1 a { color: inherit; text-decoration: none; }",
      "#gameTypeSwitcher1 a.active { color: #CC220B; text-decoration: underline; }",
      "#gameTypeSwitcher1 img { float: left; margin-right: 3px; width: 16px; height: 16px; }",
      "#gameTypeSwitcher1 span { vertical-align: middle; }",
      "#gameTypeSwitcher2 { margin: 3px 0; color: black; }",
      "#gameTypeSwitcher2 input { vertical-align: middle; }",
      "#gameTypeSwitcher2 label { margin: 0 20px 0 3px; vertical-align: middle; }",
      "#gameTypeSwitcher2 a { margin-left: 100px; color: black; text-decoration: underline; cursor: pointer; }",
      "#quickPublic { margin-left: 50px; }"
    );
    quakelive.AddHook("OnContentLoaded", onContentLoaded);
  }

  function onContentLoaded() {
    if (!quakelive.activeModule || quakelive.activeModule.GetTitle() != "Home")
      return;

    var $matchlistHeader = $("#matchlist_header_text");
    if ($matchlistHeader.length == 0) return;
    $matchlistHeader.empty();
    $matchlistHeader.css({ "display": "none", "height": "auto" });

    $matchlistHeader = $("#matchlist_header");

    var html1 = "";
    $.each(gameTypes, function(i, gameType) {
      html1 += "<div class='gametype' title='" + gameType.hint + "'><a href='javascript:void(0);' data-index='" + i + "'>"
      + "<img src='" + quakelive.resource('/images/gametypes/xsm/' + gameType.icon + '.png') + "'><span>" + gameType.text + "</span></a></div>";
    });
    var html2 = "<input type='checkbox' id='quickInsta'><label for='quickInsta'>Instagib</label>"
      + "<input type='checkbox' id='quickFriends'><label for='quickFriends'>Friends only</label>"
      + "<input type='checkbox' id='quickPrem'><label for='quickPrem'>Premium only</label>"
      + "<input type='radio' name='quickPrivate' id='quickPublic' value='0'><label for='quickPublic'>Public</label>"
      + "<input type='radio' name='quickPrivate' id='quickPrivate' value='1'><label for='quickPrivate'>Private</label>"
      + "<input type='radio' name='quickPrivate' id='quickInvited' value='2'><label for='quickInvited'>Invited</label>";
    $matchlistHeader.prepend(
      "<div id='gameTypeSwitcher'>"
      + "<div id='gameTypeSwitcher1'>" + html1 + "</div>"
      + "<div id='gameTypeSwitcher2'>" + html2 + "</div>"
      +"</div>"
      );
    $("#matchlist_header_controls").css({ "width": "auto", "float": "right" });

    $("#gameTypeSwitcher1 a").click(function () {
      currentGameTypeIndex = $(this).data("index");
      updateCustomizationFormGameType();
    });

    $("#quickInsta").change(function() {
      isInstagib = $(this).prop("checked");
      updateCustomizationFormGameType();
    });

    $("#quickFriends").change(function () {
      $("#ctrl_filter_social").val($("#quickFriends").prop("checked") ? "friends" : "any").trigger("chosen:updated").trigger("change");
    });

    $("#quickPrem").change(function () {
      $("#premium_only").prop("checked", $("#quickPrem").prop("checked")).trigger("change");
    });

    $("#gameTypeSwitcher2 input:radio").click(function () {
      var privateValue = $(this).prop("value");
      $("#publicServer").parent().find("input:radio[value=" + privateValue + "]").prop("checked", "true").trigger("click");
    });

    parseCustomizationForm();
  }

  function parseCustomizationForm() {
    var $field = $("#ctrl_filter_gametype");
    var currentGameTypeValue = $field.val();

    currentGameTypeIndex = -1;
    isInstagib = false;
    $.each(gameTypes, function (i, gameType) {
      if (gameType.regular == currentGameTypeValue || gameType.instagib == currentGameTypeValue) {
        currentGameTypeIndex = i;
        isInstagib = currentGameTypeValue != gameType.regular;
      }
    });
    $("#quickPrem").prop("checked", $("#premium_only").prop("checked"));
    var privateValue = $("#publicServer").parent().find("input:radio:checked").val();
    $("#gameTypeSwitcher2").find("input:radio[value=" + privateValue + "]").prop("checked", "true");
    highlightActiveGametype();
  }

  function updateCustomizationFormGameType() {
    if (currentGameTypeIndex < 0) return;
    var gameType = gameTypes[currentGameTypeIndex];
    $("#ctrl_filter_gametype").val(isInstagib ? gameType.instagib : gameType.regular).trigger("chosen:updated").trigger("change");
    highlightActiveGametype();
  }

  function highlightActiveGametype() {
    $("#gameTypeSwitcher1 .gametype .active").removeClass("active");
    $("#gameTypeSwitcher1 .gametype [data-index=" + currentGameTypeIndex + "]").addClass("active");
    var gameType = currentGameTypeIndex >= 0 ? gameTypes[currentGameTypeIndex] : null;
    var allowInsta = gameType && gameType.instagib != gameType.regular;
    $("#quickInsta").prop("disabled", !allowInsta);
    $("#quickInsta").prop("checked", allowInsta && isInstagib);
  }

  if (extraQL)
    init();
  else
    $.getScript("http://sourceforge.net/p/extraql/source/ci/master/tree/scripts/extraQL.js?format=raw", init);
})();