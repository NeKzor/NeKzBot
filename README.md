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
| .dem files of users | [SourceModule](https://github.com/NeKzor/NeKzBot/blob/master/src/NeKzBot/Services/CommandHandlingService.cs#L59) | Allows users to analyze their latest uploaded Source Engine recording |
| Webhook data of channels | [ServiceModule](https://github.com/NeKzor/NeKzBot/blob/master/src/NeKzBot/Services/Notifications/NotificationService.cs#L57) | Allows to send notification updates |

### Modules
TODO: Generate this

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