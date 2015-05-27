// ==UserScript==
// @name        QL Twitter integration
// @version     1.2
// @author      PredatH0r
// @description	Show @quakelive tweets in chat bar
// @unwrap
// ==/UserScript==

/*

This script integrates the official "@quakelive" Twitter channel as a separate tab in the QL chat window.
If the "QL Chat Dock" script is also installed, the Twitter popup will automatically adjust to the QL window's size.

Version 1.2
- ensuring consistent order of tabs in the chat bar

Version 1.1
- updated extraQL script url to sourceforge

Version 1.0
- first public release

*/

(function () {
  // external variables
  var quakelive = window.quakelive;
  var extraQL = window.extraQL;

  function init() {
    // delay init so that twitch, twitter, ESR and IRC scripts add items to chat menu bar in a defined order
    if (extraQL.hookVersion) // introduced at the same time as the addTabPage() "priority" param
      delayedInit();
    else
      setTimeout(delayedInit, 800);
  }

  function delayedInit() {
    onContentLoaded();
    quakelive.AddHook("OnContentLoaded", onContentLoaded);

    // the resizeLayout script's event handler will resize the <div> for us
    if (typeof (window.onresize) == "function")
      window.onresize();
  }

  function onContentLoaded() {
    if ($("#twitterFeed").length)
      return;

    extraQL.addStyle("#twitterFeed { width: 300px; background-color: white; display: none; }");

    var html =
      '<a class="twitter-timeline" href="https://twitter.com/quakelive" data-widget-id="445033815178625024">Tweets by @quakelive</a>' +
      '<script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?"http":"https";if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+"://platform.twitter.com/widgets.js";fjs.parentNode.insertBefore(js,fjs);}}(document,"script","twitter-wjs");</script>';

    var page = "<div id='twitterFeed' class='chatBox'>" + html + "</div>";
    extraQL.addTabPage("twitterFeed", "Twitter", page, showTwitterTab, 200);
  }

  function showTwitterTab() {
    $("#twitter-widget-0").addClass("fullHeight"); // this enables automatic resizing via resizeLayout.usr.js
    extraQL.showTabPage("twitterFeed");
  }

  init();
})();
