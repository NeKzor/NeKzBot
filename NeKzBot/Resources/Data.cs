using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Modules.Public.Others;
using NeKzBot.Server;
using NeKzBot.Webhooks;

namespace NeKzBot.Resources
{
	public static partial class Data
	{
		public static readonly Color BoardColor = new Color(4, 128, 165);
		public static readonly Color BasicColor = new Color(14, 186, 83);
		public static readonly Color DropboxColor = new Color(0, 126, 229);
		public static readonly Color TwitchColor = new Color(100, 65, 164);
		public static readonly Color TwitterColor = new Color(29, 161, 242);
		public static readonly Color SpeedruncomColor = new Color(229, 227, 87);
		public static readonly Color SteamColor = new Color(22, 26, 33);

		public static readonly string CheatCommand = "cheat";
		public static readonly string CreditsCommand = "credits";
		public static readonly string ExploitCommand = "exploit";
		public static readonly string SegmentedRunCommand = "segmented";
		public static readonly string SubscriptionListMessage = $"List of available subscriptions:\n• `{Portal2WebhookKeyword}` updates you about the latest Portal 2 challenge mode world records on board.iverb.me."
															  + $"\n• `{SpeedrunComSourceWebhookKeyword}` gets you the latest notifications about GoldSrc and Source Engine on speedrun.com."
															  + $"\n• `{SpeedrunComPortal2WebhookKeyword}` gets you the latest Portal 2 notifications on speedrun.com."
															  + $"\n• `{TwitchTvWebhookKeyword}` notifies you when somebody from the streaming list goes live (not recommended to use).";
		public static readonly string Portal2IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"; // <- this link could break
		public static readonly string SpeedrunComIconUrl = "https://www.speedrun.com/themes/default/favicon.png";
		public static readonly string TwitchTvIconUrl = "https://www.twitch.tv/favicon.ico";
		public static readonly string SteamcommunityIconUrl = "https://steamcommunity.com/favicon.ico";

		public static string[] ConsoleCommands { get; private set; }
		public static string[] AudioAliases { get; private set; }
		public static string[] RandomGames { get; private set; }
		public static string[] SpecialThanks { get; private set; }
		public static string[] TwitchStreamers { get; private set; }
		public static string[,] ScriptFiles { get; private set; }
		public static string[,] MemeCommands { get; private set; }
		public static string[,] ToolCommands { get; private set; }
		public static string[,] LinkCommands { get; private set; }
		public static string[,] ProjectNames { get; private set; }
		public static string[,] Portal2Maps { get; private set; }
		public static string[,] QuoteNames { get; private set; }
		public static string[,] SoundNames { get; private set; }
		public static string[,] Portal2Exploits { get; private set; }
		public static string[] VipGuilds { get; private set; }
		public static string[,] Portal2Cvars { get; private set; }

		public static List<WebhookData> P2Subscribers { get; internal set; }
		public static List<WebhookData> SpeedrunComSourceSubscribers { get; internal set; }
		public static List<WebhookData> TwitchTvSubscribers { get; internal set; }
		public static List<WebhookData> SpeedrunComPortal2Subscribers { get; internal set; }
		public static List<string> TwitterLocations { get; set; } = new List<string>
		{
			"Relaxation Vault", "SPAAAAAAAAACE", "Finale 5", "Incinerator",
			"Calibration Course", "Central AI Chamber", "Out Of Bounds", "Junk Yard"
		};
		public static readonly List<string> SubscriptionList = new List<string>()
		{
			Portal2WebhookKeyword,
			SpeedrunComSourceWebhookKeyword,
			SpeedrunComPortal2WebhookKeyword,
			TwitchTvWebhookKeyword
		};

		private static async Task InitConsoleCommandsAsync() => ConsoleCommands = await Utils.ReadFromFileAsync(_fileNameConsoleCommands) as string[];
		private static async Task InitAudioAliasesAsync() => AudioAliases = await Utils.ReadFromFileAsync(_fileNameAudioAliases) as string[];
		private static async Task InitRandomGamesAsync() => RandomGames = await Utils.ReadFromFileAsync(_fileNameRandomGames) as string[];
		private static async Task InitSpecialThanksAsync() => SpecialThanks = await Utils.ReadFromFileAsync(_fileNameSpecialThanks) as string[];
		private static async Task InitTwitchStreamersAsync() => TwitchStreamers = await Utils.ReadFromFileAsync(_fileNameTwitchStreamers) as string[];
		private static async Task InitScriptFilesAsync() => ScriptFiles = await Utils.ReadFromFileAsync(_fileNameScriptFiles) as string[,];
		private static async Task InitMemeCommandsAsync() => MemeCommands = await Utils.ReadFromFileAsync(_fileNameMemeCommands) as string[,];
		private static async Task InitToolCommandsAsync() => ToolCommands = await Utils.ReadFromFileAsync(_fileNameToolCommands) as string[,];
		private static async Task InitLinkCommandsAsync() => LinkCommands = await Utils.ReadFromFileAsync(_fileNameLinkCommands) as string[,];
		private static async Task InitProjectNamesAsync() => ProjectNames = await Utils.ReadFromFileAsync(_fileNameProjectNames) as string[,];
		private static async Task InitPortal2MapsAsync() => Portal2Maps = await Utils.ReadFromFileAsync(_fileNamePortal2Maps) as string[,];
		private static async Task InitQuoteNamesAsync() => QuoteNames = await Utils.ReadFromFileAsync(_fileNameQuoteNames) as string[,];
		private static async Task InitSoundNamesAsync() => SoundNames = await Utils.ReadFromFileAsync(_fileNameSoundNames) as string[,];
		private static async Task InitP2ExploitsAsync() => Portal2Exploits = await Utils.ReadFromFileAsync(_fileNameP2Exploits) as string[,];
		private static async Task InitP2Subscribers() => P2Subscribers = await WebhookData.ParseDataAsync(_fileNameP2Subscribers);
		private static async Task InitSpeedrunComSourceSubscribers() => SpeedrunComSourceSubscribers = await WebhookData.ParseDataAsync(_fileNameSpeedrunComSourceSubscribers);
		private static async Task InitTwitchTvSubscribers() => TwitchTvSubscribers = await WebhookData.ParseDataAsync(_fileNameTwitchTvSubscribers);
		private static async Task InitVipGuilds() => VipGuilds = await Utils.ReadFromFileAsync(_fileNameVipData) as string[];
		private static async Task InitPortal2Cvars() => Portal2Cvars = await Utils.ReadFromFileAsync(_fileNamePortal2Cvars) as string[,];
		private static async Task InitSpeedrunComPortal2Subscribers() => SpeedrunComPortal2Subscribers = await WebhookData.ParseDataAsync(_fileNameSpeedrunComPortal2Subscribers);

		private static readonly string _fileExtension = ".dat";
		private static readonly string _fileNameConsoleCommands = "consolecmds" + _fileExtension;
		private static readonly string _fileNameAudioAliases = "audioaliases" + _fileExtension;
		private static readonly string _fileNameRandomGames = "playingstatus" + _fileExtension;
		private static readonly string _fileNameSpecialThanks = "credits" + _fileExtension;
		private static readonly string _fileNameTwitchStreamers = "streamers" + _fileExtension;
		private static readonly string _fileNameScriptFiles = "scripts" + _fileExtension;
		private static readonly string _fileNameMemeCommands = "memes" + _fileExtension;
		private static readonly string _fileNameToolCommands = "tools" + _fileExtension;
		private static readonly string _fileNameLinkCommands = "links" + _fileExtension;
		private static readonly string _fileNameProjectNames = "runs" + _fileExtension;
		private static readonly string _fileNamePortal2Maps = "p2maps" + _fileExtension;
		private static readonly string _fileNameQuoteNames = "quotes" + _fileExtension;
		private static readonly string _fileNameSoundNames = "sounds" + _fileExtension;
		private static readonly string _fileNameP2Exploits = "exploits" + _fileExtension;
		private static readonly string _fileNameP2Subscribers = "p2subs" + _fileExtension;
		private static readonly string _fileNameSpeedrunComSourceSubscribers = "srsourcesubs" + _fileExtension;
		private static readonly string _fileNameTwitchTvSubscribers = "twtvsubs" + _fileExtension;
		private static readonly string _fileNameVipData = "vip" + _fileExtension;
		private static readonly string _fileNamePortal2Cvars = "p2cvars" + _fileExtension;
		private static readonly string _fileNameSpeedrunComPortal2Subscribers = "srportal2subs" + _fileExtension;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Data", LogColor.Init);
			await InitAudioAliasesAsync();
			await InitConsoleCommandsAsync();
			await InitLinkCommandsAsync();
			await InitMemeCommandsAsync();
			await InitP2ExploitsAsync();
			await InitPortal2MapsAsync();
			await InitProjectNamesAsync();
			await InitQuoteNamesAsync();
			await InitRandomGamesAsync();
			await InitScriptFilesAsync();
			await InitSoundNamesAsync();
			await InitSpecialThanksAsync();
			await InitToolCommandsAsync();
			await InitTwitchStreamersAsync();
			await InitP2Subscribers();
			await InitSpeedrunComSourceSubscribers();
			await InitTwitchTvSubscribers();
			await InitVipGuilds();
			await InitPortal2Cvars();
			await InitSpeedrunComPortal2Subscribers();
			// ^Before manager
			await InitMangerAsync();
		}

		public static async Task InitAsync(int index)
		{
			switch (index)
			{
				case 0: await InitConsoleCommandsAsync(); break;
				case 1: await InitAudioAliasesAsync(); break;
				case 2: await InitRandomGamesAsync(); break;
				case 3: await InitSpecialThanksAsync(); break;
				case 4: await InitTwitchStreamersAsync(); break;
				case 5: await InitScriptFilesAsync(); break;
				case 6: await InitMemeCommandsAsync(); break;
				case 7: await InitToolCommandsAsync(); break;
				case 8: await InitLinkCommandsAsync(); break;
				case 9: await InitProjectNamesAsync(); break;
				case 10: await InitPortal2MapsAsync(); break;
				case 11: await InitQuoteNamesAsync(); break;
				case 12: await InitSoundNamesAsync(); break;
				case 13: await InitP2ExploitsAsync(); break;
				case 14: await InitP2Subscribers(); break;
				case 15: await InitSpeedrunComSourceSubscribers(); break;
				case 16: await InitTwitchTvSubscribers(); break;
				case 17: await InitVipGuilds(); break;
				case 18: await InitPortal2Cvars(); break;
				case 19: await InitSpeedrunComPortal2Subscribers(); break;
			}
		}

		public static async Task InitCommandByIndexAsync(int index)
		{
			switch (index)
			{
				case 0: await Fun.GetRandomCheat(CheatCommand); break;
				case 1: break;
				case 2: break;
				case 3: await Rest.GetCredits(CreditsCommand); break;
				case 4: break;
				case 5: break;
				case 6: await Utils.CommandBuilder(() => Builder.Memes(Utils.CBuilderIndex), 0, MemeCommands); break;
				case 7: await Utils.CommandBuilder(() => Builder.Tools(Utils.CBuilderIndex), 0, ToolCommands); break;
				case 8: await Utils.CommandBuilder(() => Builder.Links(Utils.CBuilderIndex), 0, LinkCommands); break;
				case 9: await Resource.GetSegmentedRun(SegmentedRunCommand); break;
				case 10: break;
				case 11: await Utils.CommandBuilder(() => Builder.Text(Utils.CBuilderIndex), 0, QuoteNames); break;
				case 12: break;
				case 13: await Fun.GetRandomExploit(ExploitCommand); break;
				case 14: break;
				case 15: break;
				case 16: break;
				case 17: break;
				case 18: break;
				case 19: break;
			}
		}

		#region HELP COMMAND LIST
		#region PUBLIC
		public static readonly string FunPublic = "**[Fun Commands]**"
												+ $"\n• `{Configuration.Default.PrefixCmd}cheat`"
												+ $"\n• `{Configuration.Default.PrefixCmd}exploit`"
												+ $"\n• `{Configuration.Default.PrefixCmd}funfact`"
												+ $"\n• `{Configuration.Default.PrefixCmd}hello`"
												+ $"\n• `{Configuration.Default.PrefixCmd}bot`"
												+ $"\n• `{Configuration.Default.PrefixCmd}ris <text>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}meme`"
												+ $"\n• `{Configuration.Default.PrefixCmd}routecredit`"
												+ $"\n• `{Configuration.Default.PrefixCmd}question <question>`";

		public static readonly string InfoPublic = "**[Info Commands]**"
												 + $"\n• `{Configuration.Default.PrefixCmd}when`"
												 + $"\n• `{Configuration.Default.PrefixCmd}idinfo`"
												 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} uptime`"
												 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} location`"
												 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} info`"
												 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} version`"
												 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} changelog`"
												 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} guilds`";

		public static readonly string LeaderboardPublic = "**[Portal 2 Leaderboard Commands]**"
														+ $"\n• `{Configuration.Default.PrefixCmd}latestwr <mapname>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}wr <mapname>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}rank <mapname>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}player <rankname>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}latestentry <mapname>`";

		public static readonly string SpeedrunComPublic = "**[SpeedrunCom Commands]**"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}wr <game>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}wrs <game>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}top <game>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}pbs <player>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}game <name>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}player <name>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}moderators <game>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}haswr <player>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}rules <game>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}ilrules <game>`"
														+ $"\n• `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}notification <count> <type>`";

		public static readonly string OthersPublic = "**[Other Commands]**"
												   + $"\n• `{Configuration.Default.PrefixCmd}meme`"
												   + $"\n• `{Configuration.Default.PrefixCmd}tool`"
												   + $"\n• `{Configuration.Default.PrefixCmd}link`"
												   + $"\n• `{Configuration.Default.PrefixCmd}text`";

		public static readonly string LinuxOnly = "**[Raspberry Pi Commands]**"
												+ $"\n• `{Configuration.Default.PrefixCmd}rpi specs`"
												+ $"\n• `{Configuration.Default.PrefixCmd}rpi date`"
												+ $"\n• `{Configuration.Default.PrefixCmd}rpi uptime`"
												+ $"\n• `{Configuration.Default.PrefixCmd}rpi temperature`"
												+ $"\n• `{Configuration.Default.PrefixCmd}rpi os`";

		public static readonly string ResourcesPublic = "**[Resource Commands]**"
													  + $"\n• `{Configuration.Default.PrefixCmd}scripts <name>`"
													  + $"\n• `{Configuration.Default.PrefixCmd}dialogue <mapname>`"
													  + $"\n• `{Configuration.Default.PrefixCmd}segmented <project>`";

		public static readonly string RestPublic = "**[Rest Commands]**"
												 + $"\n• `{Configuration.Default.PrefixCmd}invite`"
												 + $"\n• `{Configuration.Default.PrefixCmd}devserver`"
												 + $"\n• `{Configuration.Default.PrefixCmd}join`"
												 + $"\n• `{Configuration.Default.PrefixCmd}view <mapname>`"
												 + $"\n• `{Configuration.Default.PrefixCmd}credits`";

		public static readonly string VipServersOnly = "**[Special Commands - VIP Servers Only]**"
													 + $"\n• `{Configuration.Default.PrefixCmd}sound`"
													 + $"\n• `{Configuration.Default.PrefixCmd}stream <channel>`"
													 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} connect`"
													 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} disconnect`"
													 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} stop`"
													 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} subscribe <channel_id> <subscription> <username>`"
													 + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} unsubscribe <id>`";
		#endregion

		#region PRIVATE
		public static readonly string MainServerOnly = "**[Bot Commands - Developer Server Only]**"
													 + $"\n• `{Configuration.Default.PrefixCmd}cloud`"
													 + $"\n• `{Configuration.Default.PrefixCmd}dbfolder`"
													 + $"\n• `{Configuration.Default.PrefixCmd}dbdelete <file>`"
													 + $"\n• `{Configuration.Default.PrefixCmd}giveaway`"
													 + $"\n• `{Configuration.Default.PrefixCmd}giveaway maxtries <tries>`"
													 + $"\n• `{Configuration.Default.PrefixCmd}giveaway setstate <state>`"
													 + $"\n• `{Configuration.Default.PrefixCmd}giveaway status`";

		public static readonly string LeaderboardPrivate = "**[Leaderboard Commands - Bot Owner Only]**"
														 + $"\n• `{Configuration.Default.PrefixCmd}boardparameter <name>`"
														 + $"\n• `{Configuration.Default.PrefixCmd}cachetime`"
														 + $"\n• `{Configuration.Default.PrefixCmd}setcachetime <value>`";

		public static readonly string BotOwnerOnly = "**[Bot Commands - Bot Owner Only]**"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} newgame`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} setgame <name>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} echo <message>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} send <guild_id> <channel> <message>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} add <name> <values>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} delete <name> <value>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} reload`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} showdata <name>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} datavars`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} cleanconfig`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} revive <task>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} taskstatus <name>`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} watches`"
												   + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} webhooktest`";
		#endregion
		#endregion

		#region OTHERS
		public static readonly string ServerSpecs = "**Architecture**\n • ARMv8 64/32-bit"
												  + "\n**SoC**\n • Broadcom BCM2837"
												  + "\n**CPU**\n • 1.2 GHz 64-bit quad-core ARM Cortex-A53"
												  + "\n**GPU**\n • Broadcom VideoCore IV\n• OpenGL ES 2.0"
												  + "\n**RAM**\n • 1GB"
												  + "\n**Network**\n • 10/100 Ethernet\n• 802.11n Wireless";

		public static readonly string MessagEnding = $"\n**Notes**\n• Some commands don't require a parameter.• Some commands require a special permission.\n• Try `{Configuration.Default.PrefixCmd}help <command>` for more information.";
		public static readonly string[] BotGreetings = { "Hey!", "Yo!", "Hi!", "Yoo!", "Hello!", "Hej!", "Hallo!", "Hola!", "Salut!" };
		public static readonly string[] BotFeelings = { ":grinning:", ":grimacing:", ":grin:", ":smiley:", ":smile:", ":sweat_smile:", ":wink:", ":slight_smile:", ":rage:", ":yum:", ":blush:", ":robot:", ":thumbsup:", ":ok_hand:", ":v:", ":heart:" };

		public static readonly string[] BotAnswers =
		{
			"Yes.", "Yeah.", "Ye.", ":thumbsup:",
			"No.", "Nope.", "Nah.", ":thumbsdown:",
			"Of course.", ":ok_hand:",
			"Maybe.", "Perhaps.", "Obviously...", "Good question...",
			"What do you mean?", "I bad understund what you written here.", "What do you think?"
		};

		public static readonly UserStatus[] BotStatus =
		{
			UserStatus.Online,
			UserStatus.Idle,
			UserStatus.DoNotDisturb
		};

		public const string Portal2WebhookKeyword = "p2wrs";
		public const string SpeedrunComSourceWebhookKeyword = "source";
		public const string SpeedrunComPortal2WebhookKeyword = "portal2";
		public const string TwitchTvWebhookKeyword = "twitch";
		public const string LatestChangelog = "\n• Added support for playing audio streams on multiple servers"
											+ "\n• Added VIP guild permission"
											+ "\n• Revised help mode commands"
											+ "\n• Added message embedding extension"
											+ "\n• Changed message format into embeds for leaderboard and speedrun module"
											+ "\n• Added a second check if a user has a nickname for the leaderboard module"
											+ "\n• Added command to list all available sound commands"
											+ "\n• Added a profile comparison command for leaderboard module";
		#endregion
	}
}