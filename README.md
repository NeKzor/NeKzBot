[![Build Status](https://travis-ci.org/NeKzor/NeKzBot.svg?branch=master)](https://travis-ci.org/NeKzor/NeKzBot)
[![Build Version](https://img.shields.io/badge/version-v2.0-brightgreen.svg)](https://github.com/NeKzor/NeKzBot/projects/2)

**NeKzBot** is a bot for [Discord](https://discordapp.com) which is focused on providing unique services.

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
  * [Cvar Dictionary](https://github.com/NeKzor/SourceAutoRecord)

### Permissions
#### Required
* VIEW_CHANNEL
* SEND_MESSAGES
* ADD_REACTIONS
* ATTACH_FILES

#### Optional
* MANAGE_WEBHOOKS

### Data
NeKzBot does not log any potential sensitive user data. However, these informations will be saved in the bot's database:

| Data | Module | Why |
| --- | --- | --- |
| .dem files of users | [SourceModule](https://github.com/NeKzor/NeKzBot/blob/master/src/Services/SourceDemoService.cs#L97) | Allows users to analyze their latest uploaded Source Engine recording |
| Webhook data of channels | [ServiceModule](https://github.com/NeKzor/NeKzBot/blob/master/src/Services/Notifications/NotificationService.cs#L158) | Allows to send notification updates |

Demo files usually have a life time of 21+ days. Webhook data will be deleted automatically if it doesn't
exist anymore e.g.: somebody with valid permissions deleted it, channel or guild got deleted.

### Modules

| Command | Alias | Module |
| --- | --- | --- |
| `.services.` | `service` | ServiceModule |
| `.services.?` | `info`, `help` | ServiceModule |
| `.services.speedruncom.` | `srcom` | ServiceModule |
| `.services.speedruncom.subscribe` | `sub`, `create`, `hook` | ServiceModule |
| `.services.speedruncom.unsubscribe` | `unsub`, `delete`, `unhook` | ServiceModule |
| `.ris <text:String...>` | - | FunModule |
| `.meme (imageName:String)` | - | FunModule |
| `.info` | `?` | InfoModule |
| `.stats` | - | InfoModule |
| `.invite` | - | InfoModule |
| `.modules` | `help` | InfoModule |
| `.portal2boards.` | `p2b`, `p2` | Portal2Module |
| `.portal2boards.map (mapName:String...)` | - | Portal2Module |
| `.portal2boards.discovery (discoveryName:String...)` | `exploit`, `glitch` | Portal2Module |
| `.portal2boards.?` | `info`, `help` | Portal2Module |
| `.portal2boards.leaderboard (mapName:String...)` | `lb` | Portal2Module |
| `.portal2boards.changelog <mapName:String...>` | `cl`, `clog` | Portal2Module |
| `.portal2boards.profile (userNameOrSteamId64:String...)` | `pro`, `user` | Portal2Module |
| `.portal2boards.aggregated` | `agg` | Portal2Module |
| `.cvars.` | `cvar` | SourceModule |
| `.cvars.?` | `info`, `help` | SourceModule |
| `.cvars.halflife2 <cvar:String>` | `hl2` | SourceModule |
| `.cvars.portal <cvar:String>` | `p`, `p1` | SourceModule |
| `.cvars.portal2 <cvar:String>` | `p2` | SourceModule |
| `.cvars.thebeginnersguide <cvar:String>` | `beginnersguide`, `tbg` | SourceModule |
| `.cvars.thestanleyparable <cvar:String>` | `stanley`, `tsp` | SourceModule |
| `.guild` | `server` | StatsModule |
| `.hierarchy` | - | StatsModule |
| `.channel` | - | StatsModule |
| `.id (ascending:Boolean=True)` | - | StatsModule |
| `.disc (ascending:Boolean=True)` | `discriminator` | StatsModule |
| `.joined (ascending:Boolean=True)` | - | StatsModule |
| `.created (ascending:Boolean=True)` | - | StatsModule |
| `.score (ascending:Boolean=True)` | - | StatsModule |

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
