What is extraQL?
================

- an all-in-one package for installing and running QuakeLive enhancements (user scripts)
- for QL Focus members (beta testers) a tool to quickly switch between public and test environments
- a local HTTP server that makes the scripts from your HDD available to QL
- a HTTP proxy that allows user scripts to fetch resources from other websites
- a middleware that allows scripts to utilize certain windows functions that are not accessible from within QL
- a small javascript library that provides commonly used functions to user scripts

extraQL and QLHM
================

extraQL and the QuakeLive Hook Manager (QLHM) work hand in hand. extraQL installs the QLHM "hook.js" file in 
your QL directory, so you will be able to use QLHM from within QL to chose which scripts you want to use.
extraQL acts as a local script server for QLHM, so it loads the scripts from your local installation instead of the internet.
This makes it easy to customize existing scripts or write new ones.


[Find out more about extraQL in the Wiki](https://github.com/PredatH0r/extraQL/wiki)