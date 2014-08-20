[extraQL Download Page](https://sourceforge.net/projects/extraql/files/) | [Help / Wiki](https://sourceforge.net/p/extraql/wiki/Home/) | [Discussion Forum](https://sourceforge.net/p/extraql/discussion/)

Installation
============

A) Full local installation (recommended)
---
- download the latest extraQL ZIP file
- unzip the extraQL ZIP file to a folder of your liking, e.g. "c:\\program files (x86)"
- right-click on extraQL.exe and select "Create shortcut"
- right-click the "Shortcut to extraQL.exe" and select "Cut"
- right-click on your desktop and select "Paste"
- rename the shortcut to "extraQL"

B) hook.js only (reduced functionality)
---
- download hook.js (or if you have the ZIP, use the extraQL\\scripts\\hook.js file)
- save hook.js to your Quake Live config directory:
   * For the standalone launcher on Windows XP this is: %appdata%\\id software\\quakelive\\home\\baseq3
   * For the standalone launcher on Win Vista and later: %appdata%\\..\\LocalLow\\id software\\home\\baseq3
   * For Steam version of QL this is: <steamdir>\\SteamApps\\Common\\Quake Live\\baseq3


Changelog
=========

Version 1.6
---
- to prevent startup crashes, the Windows Crypto API is no longer used, when not running in HTTPS mode
- fixed web requests timing out on Vista/Win7 64bit (workaround for a .NET bug on that OS)
- fixed UI scaling when using large system fonts or DPI settings
- fixed "jumping" window when moving it under Linux/Wine (thx [id]Sponge for the solution!)

Version 1.5
---
- extraQL.exe can now update itself
- fixed startup error on Windows XP/2003 (caused by Crypto API limitations)
- fixed slow script update/download
- changed "Check for Updates" into "Download updates" (extraQL will now always check if there are updates)

Version 1.4
---
- implemented HTTPS support
- improved Log window

Version 1.3
---
- "Account Settings" page didn't open due to https/http scripting protection
- script names in "Script Management" are cleansed again (removing leading "Quake Live" or "QL ")
- script authors can use a @downloadURL script header to tell extraQL where to get updates from
- automatic newline correction when downloading script updates

Version 1.2
---
- unmerged hook.js and extraQL.exe
- rewritten script update logic to improve performance and stability

Version 0.111
---
- fixed Launcher username and password were not filled after starting a 2nd time
- QLranks.com script improved with Team Extensions

Version 0.110
---
- fixed error installing "hook.js" when the file didn't exist before
- increased timeout for script update checks
- added "links.usr.js" script

Version 0.109
---
- automatically click on "Play" in the QL Launcher
- allow user defined command line for starting Launcher.exe
- removing eventual read-only flag before updating hook.js
- activate already running extraQL.exe, when a 2nd instance is started

Version 0.108
---
- added options to auto-start Launcher.exe or Steam QL
- added "Quit" to context menu in system tray
- improved detection of path to Launcher.exe
- fixed hook.js installation path for Steam
- clear log messages after reaching allowed max length

Version 0.107
---
- added option to show extraQL in the system tray instead of the task bar
- added option to start extraQL minimized (useful when started through Windows Autostart)
- added option to disable checking for extraQL.exe updates
- added checks to prevent abuse/DOS attacks on public extraQL server
- allow adding scripts locally by simply putting them in the scripts/ folder

Version 0.106
---
- added version check for extraQL.exe on sourceforge
- switched to INI file for storing settings (due to reported issues with the standard .NET AppSettings system)
- added ability to download new scripts
- added script "Auto-Open chat when starting QL"
- added script "Start Page" to set a start page and open it after starting QL
- included latest version of hook.js, which supports offline mode and notification about updates

Version 0.103
---
- fixed downloading of updated scripts (character encoding issue)

Version 0.102
---
- "hook.js" will now load the scripts from a remote extraQL.exe server if no local server is running 
- updated some more stale github references to sourceforge

Version 0.101
---
- more simplifications to hook.js and the HTTP servlet for script retrieval

Version 0.100
---
- rewritten and simplified hook.js
- works now under Linux Wine/.NET 2.0
- removed more references to userscripts.org

Version 0.99
---
- fixed downloading of updated scripts (double UTF-8 byte-order-mark)
- modified hook.js and gametype script to work with latest QL UI changes

Version 0.98
---
- disabling userscripts now deletes hook.js
- fixed broken @downloadUrl in some scripts that still pointed to GitHub
- fixed default script update URL to use SourceForge

Version 0.97
---
- moved Git repository from GitHub to SourceForge
- moved changelog into readme.md

Version 0.96
---
- replaced logo
- after first installation, all userscripts are enabled by default
- updated meta-information about the userscripts
- moved unsupported scripts to the attic folder

Version 0.95
---
- added support to start QuakeLive via Steam
- always copy hook.js to target directory if it's file timestamp is older

Version 0.94
---
- UI design similar to Quake Live Launcher
- allow multiple email/password combinations
- allow arbitrary URLs as realms
- renamed "Hook" menu to "Userscripts"

Version 0.93
---
- fixed exception when installing hook.js or starting QL
- added QuakeCon channel to twitch script

Version 0.92
---
- grouped QL Focus related fields together in a section that can be hidden/shown
- minor code cleanup

Version 0.91
---
- showing more user friendly names instead of realm URLs
- added links for QL beta testers to login to QL Focus and open QL Focus forum

Version 0.90
---
- using new version of QLHM hook.js which supports selecting and loading scripts from local extraQL HTTP server

Version 0.18
---
- improved raceboard.js to show leader name and score in all-maps view

Version 0.17
---
- added raceboard.js, which shows alternative race leader boards from QLStats database

Version 0.16
---
- fixed raceTop10, which caused other scripts to not work properly

Version 0.15
---
- added "friendCommands.js" with rulex' ingame-friend-commands script (userscript.org ID 152168)

Version 0.14
---
- added "bugfixes.js" with fixes for issues in the official QL source
- added "samPreset.js" with quick access to saved start-a-match presets

Version 0.13
---
- added support for new chat bar (number of friends online, flashing when there are unread messages)

Version 0.12
---
- added header bar on top of chat / twitch / twitter / ESR popup
- removed number of streams from game caption in "twitch" popup
- chat "send" button will no longer re-appear after entering+leaving a game
- HTTP POST to /data now creates the "/data" directory if it's missing

Version 0.11
---
- HTTP POST to /data now writes files without a UTF-8 BOM
- crawler.js script now creates an !ndex.json file which maps game-ids to maps.