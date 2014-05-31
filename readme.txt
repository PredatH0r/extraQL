Scripts included in this package:

- hook.js: modified version of QLHM 0.4pre which will be copied into your %APPDATA%\..\LocalLow\id Software\<realm>\home\baseq3
  This script is loaded by QL and will then load all other scripts.
- autoExec: adds a menu item to "Hook" and allows you to define actions to be executed when
  entering or exitting game-mode
- autoInvite: when a friend of yours sends you a chat message with the word "invite" you'll 
  automatically invite him the the premium server you're currently playing on.
- chatDock: automatically adjusts the site layout and the chat size to best fit your window size 
  and take advantage of bigger screen resolutions
- ingameAjax: special purpose script not recommended for general use and not enabled by default.
  This script will disable all HTTP requests sent from your QL web-UI to quakelive.com while you're in-game.
  It might solve fixing some lag issues during the game, but may also screw up your UI so you need to
  /web_reload after you exit game-mode.
- irc: adds an "IRC" link to quakenet.org Web Chat to your Chat bar
- linkify: scans chat messages and all text on your QL web UI for plain-text-URLs and turns them into links.
- resize: adds a menu between "Hook" and "Settings" which lets you toggle fullscreen, move the QL window
  into a corner of your screen, dock it to a side or set its size to 1024x768
- serverPopup: makes sure the server in-game information popup is not truncated at the right screen border
- toolTip: replaces QL's broken tool tips with HTML tool tips. can be enabled via cvar /web_tooltip 1
- twitter: adds the @quakelive Twitter feed to your Chat bar


How to write userscripts for Quake Live:

0) Install Google Chrome browser. You'll need its built-in developer tools.
1) Download and install Awesomium for Windows from http://www.awesomium.com/download/
2) Copy C:\Program Files (x86)\Awesomium Technologies LLC\Awesomium SDK\1.7.3.0\build\bin\inspector.pak 
   in the directory that holds your quakelive.exe, which is "%APPDATA%\..\Local\id Software\quakelive\".
   If you have access to "QL Focus", copy it also to "%APPDATA%\..\Local\id Software\focus\"
3) If you want, you can uninstall Awesomium again. You only need the inspector.pak
4) Start QL and execute this in the console: /set web_console 1
   This will enable log messages in your QL console and enables port to look into your QL UI.
5) Open http://localhost:42666/ in Chrome and click on "- QUAKE LIVE Home"
   The "Elements" view will show you the HTML code and CSS rules that make up the QL web UI.
   In the "Console" you can run any javascript commands, including jQuery expressions.
6) Click on "Scripts" and the little folder icon in the upper left corner (under "Elements")
   Locate www.quakelive.com / compiled_v2010080501.0.js, click it and select all the text in the window.
   Paste that text into some editor that is able to "prettify" this compressed Javascript code back
   into a readable format. I used Visual Studio 2010 and 2013 with ReSharper code cleanup (Ctrl+E, Ctrl+C).
   This file contains the majority of the QL web interface and you'll have to dig through it to find out
   how things work.
