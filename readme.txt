What is extraQL?

- an all-in-one package that allows you to install, choose and run user scripts to exhance your QuakeLive experience
- for QL Focus members (beta testers) a tool to quickly switch between public and test environments
- a local HTTP server that makes the scripts from your HDD available to QL
- a HTTP proxy that allows user scripts to fetch resources from other websites
- a middleware that allows scripts to utilize certain windows functions that are not accessible from within QL
- a small javascript library that provides commonly used functions to userscripts

extraQL and QLHM (Quake Live Hook Manager):

These two work hand in hand. extraQL installs the QLHM "hook.js" file in your QL directory, so you will be able to
use QLHM from within QL to chose which scripts you want to use.
extraQL acts as a local script server for QLHM, so it loads the scripts from your local installation instead of the internet.
This makes it easy to customize existing scripts or write new ones.

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


Getting started with QL scripting:

1) Install Google Chrome browser. You'll need its built-in developer tools.
2) Download and install Awesomium for Windows from http://www.awesomium.com/download/
3) Copy C:\Program Files (x86)\Awesomium Technologies LLC\Awesomium SDK\1.7.3.0\build\bin\inspector.pak 
   in the directory that holds your quakelive.exe, which is "%APPDATA%\..\Local\id Software\quakelive\".
   If you are a "QL Focus" member (beta-tester), copy it also to "%APPDATA%\..\Local\id Software\focus\"
4) If you want, you can uninstall Awesomium again. You only need the inspector.pak
5) Start QL and execute this in the console: /set web_console 1
   This will enable log messages in your QL console and enables port to look into your QL UI.
   You have to restart QL for these changes to take effect.
6) Open http://localhost:42666/ in Chrome and click on "- QUAKE LIVE Home"
   The "Elements" view will show you the HTML code and CSS rules that make up the QL web UI.
   In the "Console" you can run any javascript commands, including jQuery expressions.
7) Click on "Scripts" and the little folder icon in the upper left corner (under "Elements")
   Locate www.quakelive.com / compiled_v2010080501.0.js, click it and select all the text in the window.
   Paste that text into some editor that is able to "prettify" this compressed Javascript code back
   into a readable format. I used Visual Studio 2010 and 2013 with ReSharper code cleanup (Ctrl+E, Ctrl+C).
   This file contains the majority of the QL web interface and you'll have to dig through it to find out
   how things work.
