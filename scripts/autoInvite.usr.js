// ==UserScript==
// @id             186664
// @name           Auto Invite
// @version        2.0
// @author         PredatH0r
// @description    Your friends will be able to write the word "invite" to you, and automatically get a real invite to the server you're on.
// @unwrap
// ==/UserScript==

/*
This script is a modified version of flugsio's "Quake Live Pro Auto Invite"
(http://userscripts.org/scripts/show/107333).
It is now compatible with QLHM and the Quake Live standalone client.
*/

(function (win) { // scope
  var quakelive = win.quakelive;
  var inHook = false;
  function installHook() {
    try {
      quakelive.AddHook('IM_OnMessage', function(json) {
        try {
          if (inHook || !quakelive.IsIngameClient()) {
            return;
          }
          inHook = true;
          var msg = quakelive.Eval(json);
          var friend = quakelive.modules.friends.roster.GetIndexByName(msg.who);
          var roster = quakelive.modules.friends.roster.fullRoster[friend];

          if (msg.what == 'invite') {
            roster.FriendsContextMenuHandler('invite', roster.node);
            qz_instance.SendGameCommand("echo <AUTO-INVITE> for " + msg.who);
          }
        } 
        catch(ex) {
        } 
        finally {
          inHook = false;
        }
      });
    }
    catch(e) {}
  };

  installHook();
})(window); 