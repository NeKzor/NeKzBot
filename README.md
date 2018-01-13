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
| Command | Module | Aliases |
| --- | --- | --- |
| `.services.?` | ServiceModule | `info`, `help` |
| `.services.portal2boards.subscribe` | ServiceModule | `sub`, `create`, `hook` |
| `.services.portal2boards.unsubscribe` | ServiceModule | `unsub`, `delete`, `unhook` |
| `.services.speedruncom.subscribe` | ServiceModule | `sub`, `create`, `hook` |
| `.services.speedruncom.unsubscribe` | ServiceModule | `unsub`, `delete`, `unhook` |
| `.ris <text:String...>` | FunModule | - |
| `.info` | InfoModule | - |
| `.portal2boards.?` | Portal2Module | `info`, `help` |
| `.portal2boards.leaderboard (mapName:String...)` | Portal2Module | `lb` |
| `.portal2boards.changelog (mapName:String...)` | Portal2Module | `cl`, `clog` |
| `.portal2boards.profile (userNameOrSteamId64:String...)` | Portal2Module | `pro`, `user` |
| `.portal2boards.aggregated` | Portal2Module | - |
| `.cvars.halflife2 <cvar:String>` | SourceModule | `hl2` |
| `.cvars.portal <cvar:String>` | SourceModule | `p` |
| `.cvars.portal2 <cvar:String>` | SourceModule | `p2` |
| `.demo.parser` | SourceModule | `info` |
| `.demo.?` | SourceModule | `o`, `help` |
| `.demo.filestamp` | SourceModule | `magic` |
| `.demo.protocol` | SourceModule | `protoc` |
| `.demo.servername` | SourceModule | `server` |
| `.demo.clientname` | SourceModule | `client` |
| `.demo.mapname` | SourceModule | `map` |
| `.demo.gamedirectory` | SourceModule | `dir` |
| `.demo.playbacktime` | SourceModule | `time` |
| `.demo.playbackticks` | SourceModule | `ticks` |
| `.demo.playbackframes` | SourceModule | `frames` |
| `.demo.signonlength` | SourceModule | `signon` |
| `.demo.messages` | SourceModule | `msg` |
| `.demo.messages <index:Int32>` | SourceModule | `msg` |
| `.demo.gettickrate` | SourceModule | `tickrate` |
| `.demo.gettickspersecond` | SourceModule | `tps`, `intervalpersecond`, `ips` |
| `.demo.adjustexact` | SourceModule | `adj` |
| `.demo.adjustflag` | SourceModule | `adjf` |
| `.demo.adjust` | SourceModule | `adj2` |

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