Ultima Toolkit - tools for forex algorithm development
===================================
--------------------------------------

Ultima.Toolkit is a package of libraries created to ease development of forex (and possibly not only) algorithms. It is currently in progress (and will be for quite some time).

Why it was created?
-------------------
I created few systems for automated trading. Beside core algorithms, many functionalities had to be rewritten, and often improved. This framework is an effort to collect useful functionalities and allow to easily develop custom algorithms based on them.

What is (will be) part of a toolkit?
------------------------------------

Things which I would like to include in this project:

 * Synchronization of trades to database (supported for MT4 platform)
 * Connectors to different platform vendors, so it is possible to use 1 algorithm versus many different platforms - currently MT4 is supported. Not all functionalities, but streaming prices and trading is implemented.
 * Notification manager (xmpp, sms, mail) - implemented in different projects, not ported yet
 * Web site with live statistics of running algorithms, trades spread through all brokers - not ported yet
 * Plugin management - in basic form
 * GPU accelerated functions (like indicators) - planned


How to use?
-----------

Connectors - connectors are plugins for broker platforms, so that they can connect to main Ultima server.

For MT4, MQL4 scripts located in Connectors/Scripts. They need to be copied to appropriate directories (see [Install-MT4-Connector.fsx](Install-MT4-Connector.fsx)) for more info. Also, UltimaConnector is a c++ plugin, which is needed by MT4 terminal. To build it, Boost 1.55 is needed.

Sample server, which starts Ultima on port 6300 is located in Ultima project. It supports running from console or as a Windows service.

To write plugins, reference Ultima.Core.API for an API and copy dlls and their references (without Ultima.Core.API) to plugins/ subdirectory. They will be loaded on Ultima server start.


For quick and easy install:

```
Build.cmd
Tools\FAKE\tools\FAKE.exe Install-MT4-Connector.fsx MT4Path=<MT4 installation dir>
<run mt4 terminal>
Ultima\bin\Debug\Ultima.exe
```

Sample plugin (TickLogger) will be loaded and started.

Copyright and License
---------------------
----------
Copyright 2014 Marcin Deptu³a

Licensed under the [MIT License](/LICENSE)