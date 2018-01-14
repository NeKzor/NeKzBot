[![Build Version](https://img.shields.io/badge/version-v2.0-yellow.svg)](https://github.com/NeKzor/NeKzBot/projects/2)

**NeKzBot** is a bot for [Discord](https://discordapp.com) which is focused on unique services.

### Overview
* [Services](#services)
* [Permissions](#permissions)
* [Data](#data)
* [Modules](#modules)
* [Credits](#credits)
* [Libraries](#libraries)

### Services
* Notifications
  * [board.iverb.me](https://board.iverb.me)
  * [speedrun.com](https://speedrun.com)
* Source Engine
  * [Demo Parser](https://github.com/NeKzor/SourceDemoParser.Net)
  * [Cvar Dictionary](https://github.com/NeKzor/NeKzBot/tree/master/src/gen)

### Permissions
#### Required
* READ_MESSAGES
* SEND_MESSAGES

#### Optional
* MANAGE_WEBHOOKS

### Data
NeKzBot does not log any potential sensitive user data. However, these informations will be saved in the bot's database:

| Data | Module | Why |
| --- | --- | --- |
| .dem files of users | [SourceModule](https://github.com/NeKzor/NeKzBot/blob/master/src/NeKzBot/Services/SourceDemoService.cs#L63) | Allows users to analyze their latest uploaded Source Engine recording |
| Webhook data of channels | [ServiceModule](https://github.com/NeKzor/NeKzBot/blob/master/src/NeKzBot/Services/Notifications/NotificationService.cs#L64) | Allows to send notification updates |

### Modules
| Command | Alias | Module |
| --- | --- | --- |
| `.services.` | `service` | ServiceModule |
| `.services.?` | `info`, `help` | ServiceModule |
| `.services.portal2boards.` | `portal2` | ServiceModule |
| `.services.portal2boards.subscribe` | `sub`, `create`, `hook` | ServiceModule |
| `.services.portal2boards.unsubscribe` | `unsub`, `delete`, `unhook` | ServiceModule |
| `.services.speedruncom.` | `srcom` | ServiceModule |
| `.services.speedruncom.subscribe` | `sub`, `create`, `hook` | ServiceModule |
| `.services.speedruncom.unsubscribe` | `unsub`, `delete`, `unhook` | ServiceModule |
| `.ris <text:String...>` | - | FunModule |
| `.info` | `?` | InfoModule |
| `.stats` | - | InfoModule |
| `.invite` | - | InfoModule |
| `.modules` | `help` | InfoModule |
| `.portal2boards.` | `p2b`, `p2` | Portal2Module |
| `.portal2boards.?` | `info`, `help` | Portal2Module |
| `.portal2boards.leaderboard (mapName:String...)` | `lb` | Portal2Module |
| `.portal2boards.changelog (mapName:String...)` | `cl`, `clog` | Portal2Module |
| `.portal2boards.profile (userNameOrSteamId64:String...)` | `pro`, `user` | Portal2Module |
| `.portal2boards.aggregated` | - | Portal2Module |
| `.cvars.` | `cvar` | SourceModule |
| `.cvars.halflife2 <cvar:String>` | `hl2` | SourceModule |
| `.cvars.portal <cvar:String>` | `p` | SourceModule |
| `.cvars.portal2 <cvar:String>` | `p2` | SourceModule |
| `.demo.` | `dem` | SourceModule |
| `.demo.parser` | `info` | SourceModule |
| `.demo.?` | `o`, `help` | SourceModule |
| `.demo.filestamp` | `magic` | SourceModule |
| `.demo.protocol` | `protoc` | SourceModule |
| `.demo.servername` | `server` | SourceModule |
| `.demo.clientname` | `client` | SourceModule |
| `.demo.mapname` | `map` | SourceModule |
| `.demo.gamedirectory` | `dir` | SourceModule |
| `.demo.playbacktime` | `time` | SourceModule |
| `.demo.playbackticks` | `ticks` | SourceModule |
| `.demo.playbackframes` | `frames` | SourceModule |
| `.demo.signonlength` | `signon` | SourceModule |
| `.demo.messages` | `msg` | SourceModule |
| `.demo.messages <index:Int32>` | `msg` | SourceModule |
| `.demo.gettickrate` | `tickrate` | SourceModule |
| `.demo.gettickspersecond` | `tps`, `intervalpersecond`, `ips` | SourceModule |
| `.demo.adjustexact` | `adj` | SourceModule |
| `.demo.adjustflag` | `adjf` | SourceModule |
| `.demo.adjust` | `adj2` | SourceModule |
| `.guild` | `server` | StatsModule |
| `.hierarchy` | - | StatsModule |
| `.channel` | - | StatsModule |
| `.id (ascending:Boolean)` | - | StatsModule |
| `.disc (ascending:Boolean)` | `discriminator` | StatsModule |
| `.joined (ascending:Boolean)` | - | StatsModule |
| `.created (ascending:Boolean)` | - | StatsModule |
| `.score (ascending:Boolean)` | - | StatsModule |

### Credits
* [Discord](https://discordapp.com/developers)
* [Discord.Net](https://github.com/RogueException/Discord.Net)

### Libraries
* [Discord.Addons.Interactive](https://github.com/foxbot/Discord.Addons.Interactive)
* [Discord.Addons.Preconditions](https://github.com/Joe4evr/Discord.Addons/tree/master/src/Discord.Addons.Preconditions)
* [LiteDB](https://github.com/mbdavid/LiteDB)
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* [Portal2Boards.Net](https://github.com/NeKzor/Portal2Boards.Net)
* [SourceDemoParser.Net](https://github.com/NeKzor/SourceDemoParser.Net)