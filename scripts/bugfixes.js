
(function () {

  fixFlashMessage();

  function fixFlashMessage() {
    // already fixed in focus build as of 2014-04-10, but not yet in NG0
    if (quakelive.mod_friends.UI_SetMessageAlert.toString().indexOf("clearInterval") >= 0)
      return;
    var module = quakelive.mod_friends;
    module.UI_SetMessageAlert = function() {
      if (quakelive.mod_friends.flashHandle) {
        clearInterval(quakelive.mod_friends.flashHandle)
      }
      module.flashHandle = setInterval(module.UI_FlashMessage, 750);
      module.UI_FlashMessage();
      module.UI_SetChatTitle();
    };
  }

})();