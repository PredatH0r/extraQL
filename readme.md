![screenshot](https://a.fsdn.com/con/app/proj/extraql/screenshots/001-extraQL.png/182/137)

What is extraQL?
================

- an all-in-one package for installing and running QuakeLive enhancements (user scripts)
- a tool for QL Focus members (beta testers) to quickly access test environments
- a collection of popular userscripts
- a local HTTP server that makes the scripts from your HDD available to QL
- a HTTP proxy that allows user scripts to fetch resources from other websites
- a middleware that allows scripts to utilize certain windows functions that are not accessible from within QL
- a small javascript library that provides commonly used functions to user scripts

[Download extraQL](https://sourceforge.net/projects/extraql/files/)

Find out more about extraQL in the [Wiki](https://sourceforge.net/p/extraql/wiki/Home/) or [Discussion Forum](https://sourceforge.net/p/extraql/discussion/)

Changelog
=========

2014-07-29
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