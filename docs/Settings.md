# Settings

## [/Server/Configuration.settings](../NeKzBot/Server/Configuration.settings)
| Name | Scope | Description |
| --- | :-: | --- |
| AppName | Application | Name of application, used for user agents. |
| AppVersion | Application | Current version of this application. |
| AppUrl | Application | Link to your application website. |
| PrefixCmd | Application | The prefix which let's you execute commands. |
| BotCmd | Application | The group prefix for bot specific commands. |
| AudioPath | Application | Folder path to the audio files. |
| DataPath | Application | Folder path to the [data files](#data-example). |
| LeaderboardCmd | Application | The group prefix to use specific leaderboard commands. |
| DropboxFolderName | Application | Main folder in the Dropbox cloud application folder.  |
| LogChannelName | Application | Name of the channel where events and exceptions will be logged. |
| TwitterDescription | Application | Static Twitter description for the online-offline event updater. |
| BotPermissions | Application | Integer of the permissions invite link value. Here's a nice [calculator](https://finitereality.github.io/permissions/?v=0) for that. |

## [/Server/Credentials.settings](../NeKzBot/Server/Credentials.settings)
| Name | Description |
| --- | --- |
| DiscordBotOwnerId | Your Discord user id. |
| DiscordBotToken | The bot user token of your [Discord application](https://discordapp.com/developers). |
| DiscordMainServerId | The id of your server. |
| DiscordMainServerLinkId | The static link id of your server. |
| DropboxToken | API token from your created [Dropbox App](https://www.dropbox.com/developers). |
| DropboxFolderQuery | Part of the shared folder link, leave it empty if you don't want to share this. |
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
| /Resources/Private | All your json files are in there. |

## Other Settings
* The default internal separator '|' can be changed in this class [Utils.cs](../NeKzBot/Utilities/FileUtils.cs#L15).
* The default caching extension '.tmp' can be changed in this class [Caching.cs](../NeKzBot/Server/Caching.cs#L37).
* Don't forget to change the [assembly information](../NeKzBot/Properties/AssemblyInfo.cs) too.