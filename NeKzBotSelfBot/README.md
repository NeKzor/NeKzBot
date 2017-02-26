# NeKzBotSelfBot
[![NeKz Server](https://discordapp.com/api/guilds/208570520805965826/embed.png?style=banner2)](https://discord.gg/rEazbJn)

## Features
* Edit any message with an embed
* Static caching system
* File caching system
* Cache reset timer
* Data fetching system
* Custom typereader for parsing the [Uri class](https://msdn.microsoft.com/en-us/library/system.uri(v=vs.110).aspx)

## Documentation
* [Modules](https://github.com/NeKzor/NeKzBot/NeKzBotSelfBot/Docs/blob/master/Modules.md)
* [Settings](https://github.com/NeKzor/NeKzBot/NeKzBotSelfBot/Docs/blob/master/Settings.md)
* [Changelog](https://github.com/NeKzor/NeKzBot/NeKzBotSelfBot/Docs/blob/master/Changelog.md)

## Building
* Made with [Visual Studio 2017 RC](https://www.visualstudio.com/vs/visual-studio-2017-rc)
* You should be able to port it to anything that is new since it only requires [.NET Framework 4.6.2](https://www.microsoft.com/net/download/framework)
* You would have to change some things and replace some libraries (or even make your own ones) if you want to port this to .NET Core 1.0+

## Libraries
| Use the NuGet Package Manager under _Tools_. |
| --- |
| [Discord.Net](https://github.com/RogueException/Discord.Net/tree/dev) |
| [Discord.Addons.EmojiTools](https://github.com/foxbot/Discord.Addons.EmojiTools) |
| [SpeedrunComSharp](https://github.com/LiveSplit/SpeedrunComSharp) |
| [google-api-dotnet-client](https://github.com/google/google-api-dotnet-client) |
| [roslyn](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples) |
| [tweetsharp](https://github.com/Yortw/tweetmoasharp) |
| [dropbox-sdk-dotnet](https://github.com/dropbox/dropbox-sdk-dotnet) |
| [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack) |

## Other Important Notes
* Only execute commands written by yourself because this is a selfbot with your user token
* You might get your Discord account banned if you abuse this for other uses (e.g. execute commands from other users)
* It's recommended to edit your message rather than sending one because otherwise you might get into and execution loop, be careful with that