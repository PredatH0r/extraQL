// ==UserScript==
// @id          187258
// @name        Linkify
// @version     1.4
// @author      PredatH0r
// @description	Turn plain text URLs into links
// @unwrap
// ==/UserScript==

/*******************************************************************************

Linkify Plus modified by PredatH0r, based on modified version from kry.
This version is tailored to work with Quake Live standalone client and QLHM.

Version 1.4
- fixed cannot call getElementById of undefined

Version 1.3
- compatibility with redesigned QL UI
- removed code for color selection 

Version 1.2
- fixed link detection in chat after receiving a couple messages

Version 1.1
- now also handles links in chat messages from non-active chats

Version 1.0
- fixed drop down list for color selection
- replaced DOM node event listeners with Quake Live specific handlers


Linkifier Plus modified by kry. Licensed for unlimited modification
and redistribution as long as this notice is kept intact.

Only modified Linkify Plus to change the link color to red because
in Quake Live the links appeared in white with white background.
Everything else is written by Anthony Liuallen of http://arantius.com/

Also I have made minor bugfixes to make it work better in Quake Live.

Link to Linkify Plus
  http://userscripts.org/scripts/show/1352

** Linkify plus comments below
Loosely based on the Linkify script located at:
  http://downloads.mozdev.org/greasemonkey/linkify.user.js

Originally written by Anthony Lieuallen of http://arantius.com/
Licensed for unlimited modification and redistribution as long as
this notice is kept intact.

If possible, please contact me regarding new features, bugfixes
or changes that I could integrate into the existing code instead of
creating a different script.  Thank you

Linkify Plus Red Links Version history:
 Version 1.1.9
  - Replaced /r/ from the links with /#! - if someone gave you a join link or a link to game stats before it could have reloaded QL again but this prevents it
 Version 1.1.8
  - Compatibility fix - had UI problems with custom map launcher
 Version 1.1.7
  - Fixed last version and other small fixes
 Version 1.1.6
  - Changed where links are opened. All Quake Live links that are not to forums are opened in the same window and the rest will open in a new tab or window.
 Version 1.1.5
  - Rewrote functions that gave the text to linkifycontainer, now the script should be a lot lighter
 Version 1.1.4
  - Another bugfix and stopped looking for email addresses.
 Version 1.1.3
  - Bugfix.
 Version 1.1.2
  - Removed commands from Greasemonkey "User Script Commands..." menu and added a dropdown selection to change color for better compatibility.
 Version 1.1.1
  - Still better lagfix.
 Version 1.1.0
  - Even better lagfix.
 Version 1.0.9
  - Attemp to fix lag.
 Version 1.0.8
  - Little improvement to lagfix.
 Version 1.0.7
  - Improved lagfix.
 Version 1.0.6
  - Lagfix.
 Version 1.0.5
  - Made the color changing work better.
 Version 1.0.4
  - Possibility to change color through Greasemonkey "User Script Commands..." menu.
 Version 1.0.3
  - Limited the use to Quake Live.
 Version 1.0.2
  - Changed event listener type. Sometimes a node was not inserted, but modified.
 Version 1.0.1
  - Parenthesis fix.
 Version 1.0.0
  - Changed link color to red.

Using the Linkify Plus version 2.0.2. as a base.

*******************************************************************************/

(function() {

  var notInTags = [ 'a', 'head', 'noscript', 'option', 'script', 'style', 'title', 'textarea' ];
  var textNodeXpath = ".//text()[not(ancestor::" + notInTags.join(') and not(ancestor::') + ")]";
  var urlRE = /((?:https?|ftp):\/\/[^\s'"'<>]+|www\.[^\s'"'<>]+)/gi;

  var linkcolor = "#CA3827";

/******************************************************************************/
  var oldSelectContact;
  var inOnMessage = false;

  function init() {
    quakelive.AddOnceHook("OnLayoutLoaded", onLayoutLoaded);
  }

  function onLayoutLoaded() {
    // handle incoming chat messages
    quakelive.AddHook('IM_OnMessage', IM_OnMessage);
    oldSelectContact = quakelive.mod_friends.roster.SelectContact;
    quakelive.mod_friends.roster.SelectContact = SelectContact;

    // handle content changes
    quakelive.AddHook("OnContentLoaded", replaceLinks);
  }

/******************************************************************************/

  function SelectContact(contact) {
    try {
      oldSelectContact.call(quakelive.mod_friends.roster, contact);
      var node = document.getElementById("im-chat-body");
      linkifyContainer(node);
    } catch (ex) {}
  }

  function IM_OnMessage() {
    try {
      if (inOnMessage) 
        return;
      inOnMessage = true;
      
      // need to delay this action a bit because the message has not yet been added to the chat window
      window.setTimeout(function() {
        var node = document.getElementById("im-chat-body");
        if (node)
          linkifyContainer(node);
      }, 300);
    } 
    catch(e) {
    } 
    finally {
      inOnMessage = false;
    }
  }
  
  function replaceLinks() {
    if (document.getElementById("qlv_profileTopLeft"))
      linkifyContainer(document.getElementById("qlv_profileTopLeft"));    
  }

  function linkifyContainer(container) {
    var xpathResult = document.evaluate(
      textNodeXpath, container, null,
      XPathResult.UNORDERED_NODE_SNAPSHOT_TYPE, null
    );

    var i = 0;

    function continuation() {
      var node, limit = 0;
      while (node = xpathResult.snapshotItem(i++)) {
        linkifyTextNode(node);

        if (++limit > 50) {
          return setTimeout(continuation, 0);
        }
      }
    }

    setTimeout(continuation, 0);
  }

  function linkifyTextNode(node) {
    var l, m;
    var txt = node.textContent;
    var span = null;
    var p = 0;

    while (m = urlRE.exec(txt)) {

      if (null == span) {
        //create a span to hold the new text with links in it
        span = document.createElement('span');
      }

      //get the link without trailing dots
      l = m[0].replace(/\.*$/, '');
      l = l.replace('/r/', '/#!');
      //put in text up to the link
      span.appendChild(document.createTextNode(txt.substring(p, m.index)));
      //create a link and put it in the span
      a = document.createElement('a');
      a.className = 'linkifyplus';
      a.appendChild(document.createTextNode(l));
      if (l.match(/^www/i)) {
        l = 'http://' + l;
      } else if (-1 == l.indexOf('://')) {
        l = 'mailto:' + l;
      }
      a.setAttribute('href', l);
      a.style.color = linkcolor;
      if (l.match(/^http:\/\/www\.quakelive\.com\/forum/i) || !(l.match(/^http:\/\/www\.quakelive\.com/i) || l.match(/^https:\/\/secure\.quakelive\.com/i))) {
        a.setAttribute('target', '_blank');
      }
      span.appendChild(a);
      //track insertion point
      p = m.index + m[0].length;
    }
    if (span) {
      //take the text after the last link
      span.appendChild(document.createTextNode(txt.substring(p, txt.length)));
      //replace the original text with the new span
      try {
        if (node && node.parentNode)
          node.parentNode.replaceChild(span, node);
      } catch(e) {
        console.error(e);
        console.log(node);
      }
    }
  }

  try {
    init();
  } catch (e) {
    console.log(e);
  }
})();