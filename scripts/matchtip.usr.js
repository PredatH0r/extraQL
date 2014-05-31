// ==UserScript==
// @id             186527
// @name           Quake Live Left-Hand-Side Playerlist Popup
// @version        1.5
// @description    Displays a QuakeLive server's player list on the left side of the cursor
// @author         PredatH0r
// @include        http://*.quakelive.com/*
// @exclude        http://*.quakelive.com/forum*
// @unwrap
// ==/UserScript==

/*

Version 1.5
- compatibility with "join URL" and fixed location "chat"

Version 1.4
- fixed clipping of tool tip with low y-coord and flicker when scrolling window

Version 1.3
- moved popup up so that current server browser like is fully visible

Version 1.2:
- added try/catch
- fixed timing gap between read/write of DisplayMatchPlayers

*/

(function (win) { // limit scope of variables and functions

  var quakelive = win.quakelive;
  var oldShowTooltip;
  var oldDisplayMatchPlayers;
  var hasReposition = typeof quakelive.matchtip.RepositionTip == "function";

  function init() {
    oldShowTooltip = quakelive.matchtip.ShowTooltip;
    oldDisplayMatchPlayers = quakelive.matchtip.DisplayMatchPlayers;

    $(window).off("scroll", quakelive.matchtip.RepositionTip);

    quakelive.matchtip.ShowTooltip = function(tip, node, content, footer, showArrow) {
      this.reposnode = node; // NG0 code doesn't set this yet, while FOCUS does. Can be removed later.
      this.repostip = tip; // NG0 code doesn't set this yet, while FOCUS does. Can be removed later.
      this.reposshowArrow = showArrow; // NG0 code doesn't set this yet, while FOCUS does. Can be removed later.
      this.reposForChat = node.parents(".rosteritem").length > 0;

      var ret = oldShowTooltip.call(this, tip, node, content, footer, showArrow); // FOCUS code calls this.RepositionTip()
      $(window).off("scroll", this.RepositionTip); // disable the FOCUS scroll code
      if (!hasReposition)
        this.RepositionTip(); // NG0 code doesn't call this yet
      return ret;
    }

    quakelive.matchtip.RepositionTip = function () {      
      try {
        var ARROW_HEIGHT = 175;
        var ctx = quakelive.matchtip;
        var node = ctx.reposnode;
        var tip = ctx.repostip;
        var showArrow = ctx.reposshowArrow;

        var nodeCenter = node.offset().top + node.innerHeight() / 2;
        var deltaY = $(document).scrollTop();

        var top, left, $arrow;
        if (ctx.reposForChat) {
          // position the tip window in chat area on left-hand side of the node
          left = node.offset().left;
          top = nodeCenter - tip.outerHeight() / 2 - deltaY;
          if (top < 0) top = 0;
          tip.css({ "position": extraQL.isOldUi ? "absolute" : "fixed", "left": (left - 21 - tip.outerWidth()) + "px", "top": top + "px" });

          // position the arrow
          $("#lgi_arrow_left").remove();
          $arrow = $("#lgi_arrow_right");
          if (showArrow) {
            if ($arrow.length == 0) {
              $arrow = $("<div id='lgi_arrow_right'></div>");
              $("#lgi_srv_fill").append($arrow);
            }
            if (extraQL.isOldUi) {
              top = nodeCenter - ARROW_HEIGHT / 2 - deltaY - top;
              $arrow.css({ "position": "absolute", "left": (tip.outerWidth() - 4) + "px", "top": top + "px" });
            } else {
              top = nodeCenter - ARROW_HEIGHT / 2 - deltaY;
              $arrow.css({ "position": "fixed", "left": (left - 25) + "px", "top": top + "px" });
            }
          }
        }
        else {
          // position the tip window in content area on right-hand side of the node
          left = node.offset().left + node.outerWidth() + 22;
          top = nodeCenter - 309 / 2 - deltaY;
          if (top < 0) top = 0;
          tip.css({ "position": "absolute", "left": left + "px", "top": top + "px" });

          // position the arrow
          $("#lgi_arrow_right").remove();
          if (showArrow) {
            $arrow = $("#lgi_arrow_left");
            if ($arrow.length == 0) {
              $arrow = $("<div id='lgi_arrow_left'></div>");
              $("#lgi_srv_fill").append($arrow);
            }
            top = nodeCenter - ARROW_HEIGHT / 2 - deltaY - top;
            $arrow.css({ "position": "absolute", "left": "-21px", "top": top + "px" });
          }
        }
      } catch (e) {
        console.log("^1" + e.stack + "^7");
      }
    }

    quakelive.matchtip.DisplayMatchPlayers = function (server) {
      var ret = oldDisplayMatchPlayers.call(this, server);
      try {
        // add a close button to the player list window
        var closeBtn = $("#lgi_headcol_close");
        if (closeBtn.length == 0)
          $("#lgi_cli_top").append('<div id="lgi_headcol_close" style="position:absolute; right: 12px; top:2px"><a href="javascript:;" onclick="quakelive.matchtip.HideMatchTooltip(-1); return false" class="lgi_btn_close"></a></div>');

        // position player list window
        var $tip = $('#lgi_tip');
        var top = parseInt($tip.css("top")) + 3;
        var $cli = $("#lgi_cli");
        if (this.reposForChat)
          $cli.css({ "position": extraQL.isOldUi ? "absolute" : "fixed", "left": ($tip.offset().left - $cli.outerWidth() + 4) + "px", "top": top + "px", "z-index": 22000 });
        else {
          var left = $tip.offset().left + $tip.outerWidth();
          $cli.css({ "position": "absolute", "left": left + "px", "right": "auto", "top": top + "px", "z-index": 22000 });
        }
      }
      catch(e) {}
      return ret;
    };
  }

  // New Alt Browser script (http://userscripts.org/scripts/review/73076) overwrites quakelive.DisplayMatchPlayer
  // without calling the base implementation. So we delay the init a bit and hope other scripts are done by then.
  win.setTimeout(init, 7000);
})(window);