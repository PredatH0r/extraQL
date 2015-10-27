// ==UserScript==
// @id          demoBrowser
// @name        Demo Browser
// @version     1.0
// @author      PredatH0r
// @description	Browse your local demos and play them with a click. Adds an item to the "Play" menu.
// @unwrap
// ==/UserScript==

/*

Version 1.0
- first attempt

*/

(function (win) {
  var VERSION = "1.0";

  // external globals
  var window = win;
  var $ = win.jQuery;
  var extraQL = win.extraQL;
  var qz_instance = win.qz_instance;

  // private variables
  var demos = [];

  function init() {
    try {
      extraQL.addStyle(
        "#demoBrowserBody { text-align: left; }",
        "#demoBrowserBody tbody tr:hover { background-color: #CCC; }",
        "#demoBrowserList { max-height: 600px; overflow: auto; }"
      );
      window.demoBrowser_showConsole = showConsole;
      window.nav.navbar["Play"].submenu["Browse Demos"] = { "class": "ingame_only", callback: "demoBrowser_showConsole()" };     
    }
    catch (e) { error(e); }
  }

  function showConsole() {
    try {
      var out = [];
      out.push("<div id='demoBrowserBody'>");
      out.push("<label for='demoBrowser_filter'>Filter: </label>");
      out.push("<input type='text' id='demoBrowser_filter' size='20'>");
      out.push(" (press ENTER to search; use SPACE to separate multiple keywords)");
      out.push("<br><br>");
      out.push("<div id='demoBrowserList'>");
      out.push("<table width='700px' border='0' cellspacing='0' cellpadding='0'>");
      out.push("<colgroup><col width='550' align='left'/><col width='150' align='left'/></colgroup>");
      out.push("<thead><tr><th>File</th><th>Date</th></tr></thead>");
      out.push("<tbody></tbody>");
      out.push("</table>");
      out.push("</div>");
      out.push("</div>");
      $.getJSON(extraQL.BASE_URL + "demos", fillDemoList);

      // Inject the console
      qlPrompt({
        id: "demoBrowser",
        title: "Demo Browser" + " <small>(v" + VERSION + ")</small>",
        customWidth: 800,
        body: out.join(""),
        hideButtons: false,
        cancelLabel: "Close"
    });

      // Wait for the prompt to get inserted then do stuff...
      window.setTimeout(function () {
        var $filter = $("#demoBrowser_filter");
        $filter.focus();
        $filter.keydown(function(e) {
          if (e.keyCode == 13) {
            fillDemoList(demos);
            return false;
          }
          return true;
        });
        $("#demoBrowser #modal-ok").css("display", "none");
        $("#demoBrowser").css("top", "80px");
      });
    }
    catch (e) {
      extraQL.error(e);
      $("#demoBrowser").jqmHide();
    }
  }

  function fillDemoList(data) {
    demos = data;

    var cond = $("#demoBrowser_filter").val();
    var ands = cond.split(" ");
    var keywords = [];
    $.each(ands, function(i, word) {
      word = word.trim();
      if (word)
        keywords.push(word);
    });

    var $tbody = $("#demoBrowserList tbody");
    $tbody.empty();
    $.each(demos, function (idx, demo) {
      for (var i = 0; i < keywords.length; i++) {
        if (demo.file.indexOf(keywords[i]) < 0)
          return;
      }
      $tbody.append("<tr><td><a href='javascript:void(0);'>" + demo.file + "</a></td><td>" + demo.date.replace("T", " &nbsp; ") + "</td></tr>");
    });
    $tbody.find("a").click(function () {
      playDemo($(this).text());
    });
  }

  function playDemo(file) {
    qz_instance.SendGameCommand("demo \"" + file + "\"");
  }


  init();

})(window);