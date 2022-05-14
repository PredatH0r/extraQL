[Download](http://steamcommunity.com/sharedfiles/filedetails/?id=539252269) | [Help / Wiki](https://github.com/PredatH0r/extraQL/wiki) | [Discussion Forum](https://github.com/PredatH0r/extraQL/issues)

![screenshot](http://i.imgur.com/fLDu2sK.png)

Installation
============

Subscribe to the [Steam Workshop item](http://steamcommunity.com/sharedfiles/filedetails/?id=539252269) to download and automatically update extraQL.

After the download you can find the extraQL files under:  
c:\program files (x86)\Steam\SteamApps\workshop\content\282440\539252269


Changelog
=========

Version 2.27
---
- changed requirement to .NET Framework 4.7.2 so that it can run on Win10 without additional installation of .NET 2.0/3.5

Version 2.25
---
- improved log output when steam initialisation failes due to starting extraQL directly from the steam library
- added latest version of qlstats.js

Version 2.24
---
- improved auto-detection of steam ID and allow manual override in extraQL.ini

Version 2.23
---
- use Steamworks API to determine current user's Steam-ID. Use that Steam-ID for QL's user config folder (\<steamid\>\baseq3)
- added qlstats userscript (formerly the inofficial _elo userscript)
- added indicator for deactivated accounts to /whois (cheaters or otherwise special people)

Version 2.22
---
- quakeTV 1.1: fixed couple issues with auto-following

Version 2.21
---
- quakeTV 1.0 now capable to track CA matches and switch POV when a player dies

Version 2.20
---
- added quakeTV prototype script (can't handle full or password protected servers yet)
- /whois: switched back to /players now that QL no longer returns duplicate steam IDs

Version 2.19
---
- /whois: switched back to /configstrings to work around the bugged duplicate steamids in /players output

Version 2.18
---
- fixed /whois when console dump includes timestamps (also using the less spammy /players command now to get player info)

Version 2.17
---
- show warning when the offical UI in web.pak is newer than the currently selected alternative UI
- added /whois userscript to show alias nicknames

Version 2.16
---
- improved detection of Quake Live directory when it's installed outside the standard steam folder. 
  (Only works when extraQL is started from the workshop item folder.)

Version 2.15
---
- improved forwarding focus to an already running QL window when re-starting extraQL with enabled option to auto-start QL

Version 2.14
---
- When option "Auto-Start QL" is active and you try to start a 2nd extraQL, extraQL will now bring the QL window to the foreground 
  or start QL instead of bringing the extraQL GUI to the foreground. You can now always use the extraQL desktop shortcut to start QL.
- When option "Auto-Start QL" is active and the Steam Client wasn't running, extraQL no longer tries to start Steam and QL at the same
  time to avoid the misleading Steam popup about 2 Steam Clients running under different user accounts.

Version 2.13
---
- Added config screen to enable/disable individual scripts.
- Increased font size
- When extraQL cannot initialize steam_api.dll, there is now a fallback to AppID 1007, which in some scenarios can fix the problem.
  If that happens, the "Steamworks SDK Redist" will show up in your Steam Client's "Recent" list.

Version 2.12.2
----
- ability to disable instaBounce auto-detection with /seta cg_instaBounce -1
- setting default weapon to railgun in InstaBounce

Version 2.12.1
---
- fixed instaBounce config being reset after map change

Version 2.12
---
- fixed issues with instaBounce.js not restoring the original config and printing messages on non-InstaBounce servers.

Version 2.11
---
- added instaBounce.js script, which automatically sets up aliases and key binds for +hook and +rock.
  extraQL.exe creates the assisting ibounce_on.cfg file in your QL config folder, which you should customize with your preferred binds.

Version 2.10.1
---
- fixed Auto-start QL / ServerBrowser when extraQL was started minimized

Version 2.10
---
- added Russian translation of extraQL UI (thanks to lmiol for the translation!)
- added link to workshop item for Chinese translation of QL UI
- added extraQL.ini setting "locale", which can be used to override the default extraQL UI language (en, de, ru)

Version 2.9
---
- allows to select local expanded web.pak "web" folder as alternative QL UI
- added link to German translation of QL UI
- start Steam Client when it is not running

Version 2.8.1
---
- setting current directory to .exe folder (hoping that will resolve some steam_api.dll init failures)
- logging specific error message if SteamAPI_IsSteamRunning() returns false

Version 2.8
---
- added German translation of extraQL UI
- added Alternative Quake Live UI selection to choose between the installed translated/modified web UIs

Version 2.7.1
---
- fixed restoring extraQL window after it was minimized to system tray (e.g. when starting a 2nd extraQL instance)
- added tray menu items for "Open extraQL" and "Start Server Browser" (if present)

Version 2.7
---
- added command line parameter /background to start extraQL minimized (used by SteamServerBrowser when auto-starting extraQL)
- added option to start / quit Steam Server Browser
- fixed the "start minimized" option
- removed option "allow other computers to use this extraQL server" (relic from old hook.js using a public extraQL master server)
- removed option "install scripts to <steam-id>/baseq3/js" (not needed)

Version 2.6
---
- using long living steam API session to prevent massive FPS drops after running the application for a while

Version 2.5.1
---
- fixed incorrect detection of workshop folder

Version 2.5
---
- improved check for correct Quake Live and Steam Workshop directory to prevent installing scripts to a wrong folder.

Version 2.4.1
---
- fixed starting multiple instances of extraQL

Version 2.4
---
- better error logging in case steam_api.dll could not be initialized
- added /sn_suffix to append a text to the steam nickname

Version 2.3
---
- added option to install script in "Steam Apps/common/Quake Live/<steam-id>/baseq3/js" instead of the workshop folder
  (to track down issues where users reported that script are not executed)
- fixed wrong window size when starting and the options pane was hidden
- scripts echo to the QL console when they were started
- code cleanup


Version 2.2
---
- added options to change steam nickname when starting/quitting QL in the GUI. This works even with /quit

Version 2.1
---
- fixed: Quake Live could not be restarted after extraQL loaded the steam_api.dll and used QL's application ID
  (it's now using the QL Dedicated Server ID instead)
- added cvars sn_clan and sn_name which can be individually changed and are combined into /steamnick
- inside sn_clan, sn_name and steamnick a \ can be used in addition to ^ to start a color code

Version 2.0.2
---
- fixed UI getting larger every time the options were hidden and shown again
- removed HTTPS server code

Version 2.0.1
---
- added Unicode support for /steamnick

Version 2.0
---
- new steam-exclusive version of QuakeLive made all existing scripts obsolete. This is a restart of extraQL
- everything related to the old QL Launcher.exe was removed
- auto-update handling was removed. This is now handled through the Steam Workshop

Version 1.21
---
- added a /steamnick servlet for the steamNick userscript, which can be used with the steam-exlusive QL build

Version 1.20
---
- moved source code repository from SourceForge to Github

Version 1.19
---
- improved timeout handling for web requests (e.g. to extraQL master server for update check)
- faster script update from sourceforge.net

Version 1.18
---
- added "Demo Browser" servlet and userscript. Accessible through the "Play" menu
- added servlet and userscript to handle http://127.0.0.1:27963/join/91.198.152.211:27003/passwd URLs and connect to the game when clicked

Version 1.17
---
- fixed "/elo score" and "/elo shuffle" commands (condump file was read before QL finished writing it)


Version 1.16
---
- /serverinfo servlet (used by QLranks /elo shuffle) now works with >16 players on the server (Silent Night allows 28)
- QLranks script updated to allow customizing colors with /elo colors

Version 1.15
---
- extraQL.exe can run as a windows service (UI is not supported). See WinService.cmd to install/uninstall/start/stop the service
- added /condump and /serverinfo servlets which load/parse a condump.txt file
- QLranks userscript in-game commands like "/elo shuffle" now use up-to-date player and team information (requires extraQL.exe 1.15)

Version 1.14
---
- when using "Show in System Tray" the minimize button will be hidden and X acts as minimize instead.
- the "Quake Live Account" section is now also visible when the "QL Focus Member" is visible (even if Steam is selected)
- fixed tab order of controls
- default to "Steam" after upgrade when the previous config had the setting "Autostart Steam QL" set

Version 1.13
---
- improved handling of Steam vs. Standalone Launcher in UI
- added option to manually configure QL's steam base directory
- no longer showing floating title bar on desktop when using "Show in System Tray"

Version 1.12
---
- fixed: no longer trying to log in with empty username/password when starting standalone QL Launcher

Version 1.11
---
- added links in extraQL.exe UI to open standalone and steam config directories

Version 1.10
---
- "Focus Member" check box and section are no longer visible by default, if "focus" is empty in .ini file
- fixed detection for running QL Steam build for "auto quit extraQL when QL quits"

Version 1.9
---
- fixed detection for running QL Steam build (for docker userscript)

Version 1.8
---
- support for latest steam build (with configs and hook.js in SteamApps\\common\\Quake Live\\<steam-user-id>\\baseq3

Version 1.7
---
- added option "Autoquit when QL quits"
- includes updated versions of userscripts

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