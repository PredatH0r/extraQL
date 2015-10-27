// ==UserScript==
// @id             postalDump
// @name           postalDump
// @version        1.0
// @author         PredatH0r
// @description    logs all postal messages
// @unwrap
// ==/UserScript==

/*

for developers only.
uncomment the last line of this script to get all "postal" notifications logged in your console

*/

(function () {
  // external global variables
  var qz_instance = window.qz_instance;
  var console = window.console;

  function init() {    
    var postal = window.req("postal");
    var channel = postal.channel();
    channel.subscribe("#", onPostalEvent);
    log("postalDump.js installed");
  }

  function log(msg) {
    console.log(msg);
  }

  function echo(msg) {
    qz_instance.SendGameCommand("echo \"" + msg + "\"");
  }

  function onPostalEvent(data, envelope) {
    echo("postal data: " + JSON.stringify(data));
    echo("postal envelope: " + JSON.stringify(envelope));
  }

  // there is a race condition between QL's bundle.js and the userscripts.
  // if window.req was published by bundle.js, we're good to go.
  // otherwise add a callback to main_hook_v2, which will be called by bundle.js later
  if (window.req)
    init();
  else {
    var oldHook = window.main_hook_v2;
    window.main_hook_v2 = function() {
      if (typeof oldHook == "function")
        oldHook();
      init();
    }
  }
})
//();

