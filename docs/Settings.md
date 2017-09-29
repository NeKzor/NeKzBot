# Settings

## [/Server/Configuration.settings](../NeKzBotSelfBot/Server/Configuration.settings)
| Name | Description |
| --- | --- |
| AppName | Name of application, used for user agent. |
| AppUrl | Link to your application website. |
| AppVersion | Current version of this application. |
| PrefixCmd | The prefix which let's you execute commands. |
| DropboxFolderName | Main folder in your app folder on dropbox.com. |
| TwitchChannelLink | The default link of your Twitch channel, if you have one. |

## [/Server/Credentials.settings](../NeKzBotSelfBot/Server/Credentials.settings)
| Name | Description |
| --- | --- |
| DiscordUserToken | Your own Discord account token, be careful with this, DO NOT share it with anyone. |
| SpeedruncomToken | API token from your [speedrun.com Profile](https://speedrun.com/settings) under _API Key_. |
| DropboxToken | API token from your created [Dropbox App](https://www.dropbox.com/developers). |
| DropboxFolderQuery | Part of the shared folder link, leave it empty if you don't want to share this. |
| GoogleApiKey | Generate an API key from [Google Api Manager](https://console.developers.google.com). |
| GoogleSearchEngineId | Create a custom search engine from [Google](https://cse.google.com). |
| TwitchClientId | Client id of your registered [Twitch App](https://www.twitch.tv/settings/connections). |
| TwitterConsumerKey | API token from your created [Twitter App](https://apps.twitter.com/). |
| TwitterConsumerSecret | See above. |
| TwitterAppToken | You need to generate this in order to get access to tweets. See above. |
| TwitterAppTokenSecret | See above. |

## Others
| Name | Description |
| --- | --- |
| /Data/Cache | Create this folder to use the Dropbox upload module, it is used to download your attachment. |
| /Data/Private | Only used once for parsing Portal 2 data. |
| ... /AssemblyInfo.cs | Don't forget to change this too. |