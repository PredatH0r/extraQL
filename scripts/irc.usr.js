// ==UserScript==
// @id             186820
// @name           Quake Live IRC Link
// @version        1.0
// @description    Adds a link to the QuakeNet.org IRC Web Chat to your chat window
// @downloadUrl    https://raw.githubusercontent.com/PredatH0r/extraQL/master/scripts/irc.usr.js
// @author         PredatH0r
// @include        http://*.quakelive.com/*
// @exclude        http://*.quakelive.com/forum*
// @unwrap
// ==/UserScript==

(function () {
  // external global variables
  var quakelive = window.quakelive;
  var extraQL = window.extraQL;

  var URL_IRC = "http://webchat.quakenet.org/?nick=" + quakelive.username + "&channels=quakelive&prompt=1";

  function init() {
    // delay init so that twitch, twitter, ESR and IRC scripts add items to chat menu bar in a defined order
    setTimeout(delayedInit, 2400);
  }

  function delayedInit() {
    onContentLoaded();
    //extraQL.addStyle("#tab_irc { float: right; }");
    quakelive.AddHook("OnContentLoaded", onContentLoaded);
  }

  function onContentLoaded() {
    if ($("#tab_irc").length)
      return;
    extraQL.addTabPage("irc", "IRC", "", function() {
      window.open(URL_IRC);
      window.event.stopPropagation();
    });
  }

  if (extraQL)
    init();
  else
    $.getScript("https://raw.githubusercontent.com/PredatH0r/extraQL/master/scripts/extraQL.js", init);
})();