// ==UserScript==
// @name        Quake Live Layout Resizer
// @version     1.2
// @author      PredatH0r
// @description	
// @include     http://*.quakelive.com/*
// @exclude     http://*.quakelive.com/forum*
// @unwrap
// ==/UserScript==

/*

This script finds the optimal layout for the QL chat popup, based on the current window size.

When the window is less then 1310 pixels wide, the chat will overlap the content area, 
but leaves some space on the top (configurable by web_chatOverlapIndent, default 150)
so you can access the navigation menus and "Customize" in the server browser.

If the window is wider, the chat will be shown full-height outside the content area.

Version 1.2
- removed re-appearing "send" button in chat window (again)

Version 1.1
- removed re-appearing "send" button in chat window

Version 1.0
- twitter iframe is now also resized to use full screen height
- fixed in-game chat/friend list

Version 0.9
- chat is now correctly resizing
- server browser detail window is now correctly resizing
- centered pages are pushed to the left if they would be hidden by the chat window

Version 0.8
- restored most of the original functions that got lost due to cross-domain CSS loading

Version 0.7
- hotfix to prevent endless-loop when loading QL

Version 0.6
- updated extraQL script url to sourceforge

Version 0.5
- fixed z-index to work with standard content, chat window, drop-down menus and dialog boxes

Version 0.4
- switched back to horizontal "Chat" bar, always located on the bottom edge
- introduced cvar web_chatOverlapIndent to customize how much space should be left on
  top, when the chat is overlapping the main content area
- adjusts z-index of chat and navigation menu to prevent undesired overlapping


CVARS:
  - web_chatOverlapIndent: number of pixels to skip from top-edge that won't be overlapped
    by an expanded chat window

*/

(function () {
  // external variables
  var quakelive = window.quakelive;
  var extraQL = window.extraQL;

  // constants
  var RIGHT_MARGIN = 0;
  var CVAR_chatOverlapIndent = "web_chatOverlapIndent";

  // variables
  var oldOnResize;
  var oldOnCvarChanged;
  var oldSelectContact;
  var oldRenderMatchDetails;
  var styleFullHeight;


  function init() {
    extraQL.addStyle(".chatBox { " +
      "border-left: 3px solid #444; " +
      "border-top: 3px solid #444; " +
      "border-right: 3px solid #444; " +
      "position: fixed; " +
      "bottom: 27px; " +
      "right: 0px; " +
      "}");
    extraQL.addStyle(
      "#chatContainer.expanded #collapsableChat { background-color: rgb(114,24,8); }",
      "#chatContainer .fullHeight { height: 550px; }",
      "#im-chat #im-chat-send { left: 300px; }" // "display" gets overruled, so use "left" to move it off-screen
    );
    
    // z-index adjustments
    $("#qlv_content").css("z-index", "auto"); // will be toggled between 1/auto to bring the chat in front/behind the drop down menus
    $("#newnav_top").css("z-index", "2103");
    $("ul.sf-menu *").css("z-index", "2102");
    $("#lgi_cli").css("z-index", "10003");    // has 1003 and would be overlapped by menu bar items in #newnav_top

    // chat
    $("#chatContainer").width(3 + 300 + 3).css("right", RIGHT_MARGIN + "px");
    $("#collapsableChat").addClass("bottomDockBar");
    $("#qlv_chatControl").css("height", "auto");
    $("#qlv_chatControl").addClass("chatBox");

    if (quakelive.cvars.Get(CVAR_chatOverlapIndent).value == "")
      quakelive.cvars.Set(CVAR_chatOverlapIndent, 140);

    oldOnResize = window.onresize;
    window.onresize = onResize;
    oldOnCvarChanged = window.OnCvarChanged;
    window.OnCvarChanged = onCvarChanged;
    quakelive.mod_friends.FitToParent = updateChatAndContentLayout;

    oldSelectContact = quakelive.mod_friends.roster.SelectContact.bind(quakelive.mod_friends.roster);
    quakelive.mod_friends.roster.SelectContact = function(contact) {
      oldSelectContact(contact);
      updateChatAndContentLayout();
    };

    oldRenderMatchDetails = quakelive.matchcolumn.RenderMatchDetails;
    quakelive.matchcolumn.RenderMatchDetails = function () {
      oldRenderMatchDetails.apply(quakelive.matchcolumn, arguments);
      resizeBrowserDetails();      
    }

    findCssRules();
    updateChatAndContentLayout();
  }

  function findCssRules() {
    var i, j;
    for (i = 0; i < document.styleSheets.length; i++) {
      var sheet = document.styleSheets[i];
      if (!sheet.cssRules) continue;
      for (j = 0; j < sheet.cssRules.length; j++) {
        try {
          var rule = sheet.rules[j];
          if (rule.cssText.indexOf("#chatContainer .fullHeight") == 0)
            styleFullHeight = rule.style;
        }
        catch (e) {}
      }
    }
  }

  function onResize(event) {
    if (oldOnResize)
      oldOnResize(event);

    try { updateChatAndContentLayout(); }
    catch (ex) { }
  }

  function onCvarChanged(name, val, replicate) {
    oldOnCvarChanged(name, val, replicate);
    try {
      if (name == CVAR_chatOverlapIndent)
        updateChatAndContentLayout();
    }
    catch (e) { }
  }

  function updateChatAndContentLayout() {
    try {
      var $window = $(window);
      var width = $window.width();

      // reposition background image and content area
      var margin;
      var minExpandedWidth = 3 + 1000 + 7 + 3 + 300 + 3 + RIGHT_MARGIN;
      if (width <= minExpandedWidth) {
        $("body").css("background-position", "-518px 0");
        margin = "0 3px 0 3px";
      } else if (width <= minExpandedWidth + 7 + 3 + 300 + 3) {
        $("body").css("background-position", (-518 + width - minExpandedWidth).toString() + "px 0");
        margin = "0 3px 0 " + (width - 1313).toString() + "px";
      } else {
        $("body").css("background-position", "center top");
        margin = "0 auto";
      }
      $("#qlv_container").css("margin", margin);

      // push profile page and others to the left so they are not hidden by the chat
      $("#qlv_contentBody.twocol_left").css("left", Math.min(155, (155 - (minExpandedWidth - 150 - 7 - width))) + "px");

      // modify height of elements that support it
      var height = $window.height();
      $("div:data(fill-height)").each(function() {
        var $this = $(this);
        $this.height(height - parseInt($this.data("fill-height")));
      });

      // modify height of chat
      if (quakelive.IsGameRunning()) {
        $("#collapsableChat").css("display", "none"); // hide in-game chat bar
        $("#qlv_chatControl").css("display","none");
        height = Math.floor(height) * 0.9 - 35; // 10% clock, 35px buttons
      } else {
        $("#collapsableChat").css("display", "");
        $("#qlv_chatControl").css("display", "");
      }
      height -= 3 + 27 + 14; // height of top border + title bar + bottom bar

      var topOffset = 140; // leave header and menu visible when chat is overlapping the content area
      if (width < minExpandedWidth - 7) {
        try {
          topOffset = parseInt(quakelive.cvars.Get(CVAR_chatOverlapIndent).value);
        } catch (e) {
        }
        height -= topOffset;

      } else {
        height -= 7; // leave some gap from top edge
      }

      // create more space for "Active Chat"
      var footerHeight = 400; // 210 by default
      if (height - footerHeight < 300)
        footerHeight = height - 300;
      $("#im").css("height", "auto");
      $("#im-body").height(height - footerHeight);
      $("#im-footer").css({ "background": "#222", "padding": "0 5px", height: footerHeight + "px" });
      $("#im-chat").css({ "background-clip": "content-box", "height": (footerHeight - 8) + "px" });
      $("#im-chat-body").css({ left: 0, top: "13px", width: "284px", "background-color": "white", height: (footerHeight - 8 - 13 - 6 - 33 - 6) + "px" });
      $("#im-chat input").css({ width: "282px", left: 0, top: "auto", bottom: "7px" });
      $("#im-overlay-body").css({ "background-color": "white", height: (height - 87) + "px" });

      // resize elements which support a dynamic height
      if (styleFullHeight)
        styleFullHeight.setProperty("height", height + "px");
      $("#chatContainer [data-fill]").each(function() {
        var $this = $(this);
        $this.height(height - parseInt($this.data("fill")));
      });
    

      // modify z-index to deal with drop-down-menus
      $("#qlv_content").css("z-index", topOffset >= 110 ? "auto" : "1"); // #chatContainer has z-index 999
      $("#newnav_top").css("z-index", "2103");
      $("ul.sf-menu *").css("z-index", "2102");
      $("#lgi_cli").css("z-index", "10003");

      resizeBrowserDetails();

    } catch (ex) {
      extraQL.log(ex);
    }
  }

  function resizeBrowserDetails() {
    $("#browser_details ul.players.miniscroll").css("max-height", ($(window).height() - 496) + "px");
  }

  init();
})();