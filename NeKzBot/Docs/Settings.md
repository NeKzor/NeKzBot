# Settings

## [/Server/Configuration.settings](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Server/Configuration.settings)
| Name | Scope | Description |
| --- | :-: | --- |
| AppName | Application | Name of application, used for user agents. |
| AppVersion | Application | Current version of this application. |
| AppUrl | Application | Link to your application website. |
| PrefixCmd | Application | The prefix which let's you execute commands. |
| BotCmd | Application | The group prefix for bot specific commands. |
| AudioPath | Application | Folder path to the audio files. |
| RefreshTime | User | Task delay of the Portal 2 auto updater in minutes. |
| BoardParameter | User | Portal 2 auto updater link query where it checks for new updates. |
| AutoUpdate | User | State of the Portal 2 auto updater. |
| GiveawayCode | Application | Puzzle code of the giveaway mini game, numbers only, the length will decide how long it will take to solve it. |
| GiveawayResetTime | User | Giveaway reset time in milliseconds. |
| GiveawayMaxTries | User | Maximum user tries per reset time to solve the puzzle. |
| GiveawayEnabled | User | State of the giveaway game. |
| DataPath | Application | Folder path to the [data files](#data-example). |
| CachingTime | User | Task delay of the Portal 2 internal cache reset timer in minutes.  |
| LeaderboardCmd | Application | The group prefix to use specific leaderboard commands. |
| DropboxFolderName | Application | Main folder in the Dropbox cloud application folder.  |
| LogChannelName | Application | Name of the channel where events and exceptions will be logged. |
| StreamingRoleName | Application | Role name to give when a user stream on Twitch. |
| WorldRecordRoleName | Application | Role name to give when a user has a world record. |
| TwitterDescription | Application | Static Twitter description for the online-offline event updater. |

## [/Server/Credentials.settings](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Server/Credentials.settings)
| Name | Description |
| --- | --- |
| DiscordBotOwnerId | Your Discord user id. |
| DiscordBotToken | The bot user token of your [Discord application](https://discordapp.com/developers). |
| DiscordMainServerId | The id of your server. |
| DiscordMainServerLinkId | The static link id of your server. |
| DropboxToken | API token from your created [Dropbox App](https://www.dropbox.com/developers). |
| DropboxFolderQuery | Part of the shared folder link, leave it empty if you don't want to share this. |
| GiveawayPrizeKey | Put any information in here to notify a user that he has solved the giveaway puzzle. |
| SpeedruncomToken | API token from your [speedrun.com profile](https://speedrun.com/settings) under _API Key_. |
| TwitchClientId | Client id of your registered [Twitch App](https://www.twitch.tv/settings/connections). |
| TwitterConsumerKey | API token from your created [Twitter App](https://apps.twitter.com/). |
| TwitterConsumerSecret | See above. |
| TwitterAppToken | You'll need to generate this in order to get access to tweets. See above. |
| TwitterAppTokenSecret | See above. |

## Folders
| Name | Description |
| --- | --- |
| /Resources/Cache | Create this folder for the file caching system. |
| /Resources/Private | All the data files are in there, [see below](#data-example). |



## Other Settings
* The default internal separator '|' can be changed in this class [Utils.cs](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Resources/Utils.cs#25).
* The default caching extension '.tmp' can be changed in this class [Caching.cs](https://github.com/NeKzor/blob/master/NeKzBot/NeKzBot/Server/Caching.cs#49).
* The default data file extension '.dat' can be changed in this class [Data.cs](https://github.com/NeKzor/blob/master/NeKzBot/NeKzBot/Resources/Data.cs#62).
* Don't forget to change the [assembly information](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Properties/AssemblyInfo.cs) too.

## Data Example
| Type | Array |
| --- | :-: |
| Single-Dimensional | Channel1<br>Channel2<br>Channel3 |
| Multidimensional | Command1\|Description1\|Something1<br>Command2\|Description2\|Something2<br>Command3\|Description3\|Something3|

Oh well, not the best data [parsing algorithm](https://github.com/NeKzor/NeKzBot/blob/master/NeKzBot/Resources/Utils.cs#149) but it works.