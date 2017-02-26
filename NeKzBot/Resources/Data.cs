using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using NeKzBot.Server;
using NeKzBot.Modules;
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

		public static readonly string cheatCmd = "cheat";
		public static readonly string creditCmd = "credits";
		public static readonly string exploitCmd = "exploit";
		public static readonly string srunCmd = "segmented";

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

		public static List<string> TwitterLocations { get; set; } = new List<string> { "Relaxation Vault", "SPAAAAAAAAACE", "Finale 5", "Incinerator", "Calibration Course", "Central AI Chamber", "Out Of Bounds", "Junk Yard" };
		public static List<WebhookData> P2Subscribers { get; internal set; }
		public static List<WebhookData> SRComSubscribers { get; internal set; }
		public static List<WebhookData> TwitchTvSubscribers { get; internal set; }

		private static async Task InitConsoleCommandsAsync() => ConsoleCommands = await Utils.ReadFromFileAsync(fileNameConsoleCommands) as string[];
		private static async Task InitAudioAliasesAsync() => AudioAliases = await Utils.ReadFromFileAsync(fileNameAudioAliases) as string[];
		private static async Task InitRandomGamesAsync() => RandomGames = await Utils.ReadFromFileAsync(fileNameRandomGames) as string[];
		private static async Task InitSpecialThanksAsync() => SpecialThanks = await Utils.ReadFromFileAsync(fileNameSpecialThanks) as string[];
		private static async Task InitTwitchStreamersAsync() => TwitchStreamers = await Utils.ReadFromFileAsync(fileNameTwitchStreamers) as string[];
		private static async Task InitScriptFilesAsync() => ScriptFiles = await Utils.ReadFromFileAsync(fileNameScriptFiles) as string[,];
		private static async Task InitMemeCommandsAsync() => MemeCommands = await Utils.ReadFromFileAsync(fileNameMemeCommands) as string[,];
		private static async Task InitToolCommandsAsync() => ToolCommands = await Utils.ReadFromFileAsync(fileNameToolCommands) as string[,];
		private static async Task InitLinkCommandsAsync() => LinkCommands = await Utils.ReadFromFileAsync(fileNameLinkCommands) as string[,];
		private static async Task InitProjectNamesAsync() => ProjectNames = await Utils.ReadFromFileAsync(fileNameProjectNames) as string[,];
		private static async Task InitPortal2MapsAsync() => Portal2Maps = await Utils.ReadFromFileAsync(fileNamePortal2Maps) as string[,];
		private static async Task InitQuoteNamesAsync() => QuoteNames = await Utils.ReadFromFileAsync(fileNameQuoteNames) as string[,];
		private static async Task InitSoundNamesAsync() => SoundNames = await Utils.ReadFromFileAsync(fileNameSoundNames) as string[,];
		private static async Task InitP2ExploitsAsync() => Portal2Exploits = await Utils.ReadFromFileAsync(fileNameP2Exploits) as string[,];
		private static async Task InitP2Subscribers() => P2Subscribers = await WebhookData.ParseDataAsync(fileNameP2Subscribers);
		private static async Task InitSpeedrunComSubscribers() => SRComSubscribers = await WebhookData.ParseDataAsync(fileNameSRComSubscribers);
		private static async Task InitTwitchTvSubscribers() => TwitchTvSubscribers = await WebhookData.ParseDataAsync(fileNameTwitchTvSubscribers);

		private static readonly string fileExtension = ".dat";
		private static readonly string fileNameConsoleCommands = "consolecmds" + fileExtension;
		private static readonly string fileNameAudioAliases = "audioaliases" + fileExtension;
		private static readonly string fileNameRandomGames = "playingstatus" + fileExtension;
		private static readonly string fileNameSpecialThanks = "credits" + fileExtension;
		private static readonly string fileNameTwitchStreamers = "streamers" + fileExtension;
		private static readonly string fileNameScriptFiles = "scripts" + fileExtension;
		private static readonly string fileNameMemeCommands = "memes" + fileExtension;
		private static readonly string fileNameToolCommands = "tools" + fileExtension;
		private static readonly string fileNameLinkCommands = "links" + fileExtension;
		private static readonly string fileNameProjectNames = "runs" + fileExtension;
		private static readonly string fileNamePortal2Maps = "p2maps" + fileExtension;
		private static readonly string fileNameQuoteNames = "quotes" + fileExtension;
		private static readonly string fileNameSoundNames = "sounds" + fileExtension;
		private static readonly string fileNameP2Exploits = "exploits" + fileExtension;
		private static readonly string fileNameP2Subscribers = "p2subs" + fileExtension;
		private static readonly string fileNameSRComSubscribers = "srsubs" + fileExtension;
		private static readonly string fileNameTwitchTvSubscribers = "twtvsubs" + fileExtension;

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
			await InitSpeedrunComSubscribers();
			await InitTwitchTvSubscribers();
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
				case 15: await InitSpeedrunComSubscribers(); break;
				case 16: await InitTwitchTvSubscribers(); break;
			}
		}

		public static async Task InitCommandByIndexAsync(int index)
		{
			switch (index)
			{
				case 0: await Others.GetRandomCheat(cheatCmd); break;
				case 1: break;
				case 2: break;
				case 3: await Others.GetCredits(creditCmd); break;
				case 4: break;
				case 5: break;
				case 6: await Utils.CommandCreator(() => Others.Memes(Utils.CCIndex), 0, MemeCommands); break;
				case 7: await Utils.CommandCreator(() => Others.Tools(Utils.CCIndex), 0, ToolCommands); break;
				case 8: await Utils.CommandCreator(() => Others.Links(Utils.CCIndex), 0, LinkCommands); break;
				case 9: await Others.GetSegmentedRunAsync(srunCmd); break;
				case 10: break;
				case 11: await Utils.CommandCreator(() => Others.Text(Utils.CCIndex), 0, QuoteNames); break;
				case 12: break;
				case 13: await Others.GetRandomExploit(exploitCmd); break;
				case 14: break;
				case 15: break;
				case 16: break;
			}
		}

		#region HELP COMMAND LIST
		// 1.0 is sooo much better, why did I do this :(
		#region EVERYBODY
		internal static readonly string funMsg =  "**[Fun & Useful Commands]**"
												+ $"\n• `{Configuration.Default.PrefixCmd}hello`"
												+ $"\n• `{Configuration.Default.PrefixCmd}cheat`"
												+ $"\n• `{Configuration.Default.PrefixCmd}exploit`"
												+ $"\n• `{Configuration.Default.PrefixCmd}script <name>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}dialogue <mapname>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}segmented <projectname>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}funfact`"
												+ $"\n• `{Configuration.Default.PrefixCmd}when`"
												+ $"\n• `{Configuration.Default.PrefixCmd}ris <text>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}credits`"
												+ $"\n• `{Configuration.Default.PrefixCmd}routecredit`"
												+ $"\n• `{Configuration.Default.PrefixCmd}<meme>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}<tool>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}quote <name>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}invite`"
												+ $"\n• `{Configuration.Default.PrefixCmd}idinfo`"
												+ $"\n• `{Configuration.Default.PrefixCmd}question <question>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}view <mapname>`"
												+ $"\n• `{Configuration.Default.PrefixCmd}stream <channel>`";

		internal static readonly string lbMsg = "\n**[Leaderboard Commands]**"
									 + $"\n• `{Configuration.Default.PrefixCmd}latestwr <mapname>`"
									 + $"\n• `{Configuration.Default.PrefixCmd}wr <mapname>`"
									 + $"\n• `{Configuration.Default.PrefixCmd}rank <mapname>`"
									 + $"\n• `{Configuration.Default.PrefixCmd}player <rankname>`"
									 + $"\n• `{Configuration.Default.PrefixCmd}latestentry <mapname>`";

		internal static readonly string vcMsg = "\n**[Voice Channel Commands]**"
									 + $"\n• `{Configuration.Default.PrefixCmd}<soundname>`"
									 + $"\n• `{Configuration.Default.PrefixCmd}p2`"
									 + $"\n• `{Configuration.Default.PrefixCmd}yanni`";

		internal static readonly string rpiMsg = "\n**[Raspberry Pi Commands]**"
									  + $"\n• `{Configuration.Default.PrefixCmd}rpi specs`"
									  + $"\n• `{Configuration.Default.PrefixCmd}rpi date`"
									  + $"\n• `{Configuration.Default.PrefixCmd}rpi uptime`"
									  + $"\n• `{Configuration.Default.PrefixCmd}rpi temperature`"
									  + $"\n• `{Configuration.Default.PrefixCmd}rpi os`";

		internal static readonly string srcomMsg = "\n**[SpeedrunCom Commands]**"
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

		internal static readonly string dropboxMsg = "\n**[Dropbox Commands]**"
										  + $"\n• `{Configuration.Default.PrefixCmd}cloud`"
										  + $"\n• `{Configuration.Default.PrefixCmd}dbfolder`"
										  + $"\n• `{Configuration.Default.PrefixCmd}dbdelete <file>`";

		internal static readonly string botMsg = "\n**[Bot Commands]**"
									  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} uptime`"
									  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} location`"
									  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} info`"
									  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} settings`"
									  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} version`"
									  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} changelog`";
		#endregion

		#region MAIN SERVER ONLY
		internal static readonly string mainServerMsg = "\n**[Bot Commands - Main Server Only]**"
													  + $"\n• `{Configuration.Default.PrefixCmd}giveaway`"
													  + $"\n• `{Configuration.Default.PrefixCmd}giveaway resetwhen`"
													  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} connect`"
													  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} disconnect`"
													  + $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} stop`";
		#endregion

		#region BOT OWNER
		internal static readonly string botOwnerLbMsg = "\n**[Leaderboard Commands - Bot Owner Only]**"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} refreshtime`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} setrefreshtime <time>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} updatestate <state>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} boardparameter <parameter>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} toggleupdate`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} refreshnow`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} cleanentrycache`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} cachetime`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} setcachetime <time>`";

		internal static readonly string botOwnerGameMsg = "\n**[Game Commands - Bot Owner Only]**"
			+ $"\n• `{Configuration.Default.PrefixCmd}giveaway resettime`"
			+ $"\n• `{Configuration.Default.PrefixCmd}giveaway maxtries`"
			+ $"\n• `{Configuration.Default.PrefixCmd}giveaway resetnow`"
			+ $"\n• `{Configuration.Default.PrefixCmd}giveaway togglereset`"
			+ $"\n• `{Configuration.Default.PrefixCmd}giveaway setstate`"
			+ $"\n• `{Configuration.Default.PrefixCmd}giveaway status`";

		internal static readonly string botOwnerDropboxMsg =
			"\n**[Dropbox Commands - Bot Owner Only]**"
			+ $"\n• `{Configuration.Default.PrefixCmd}dbdelete <folder> <file>`";

		internal static readonly string botOwnerBotMsg =
			"\n**[Bot Commands - Bot Owner Only]**"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} newgame`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} cleanconfig`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} settings`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} setgame <name>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} add <cmdname> <value1> <value2> <etc.>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} delete <cmdname> <value>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} revive <name>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} reload`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} showdata <name>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} data`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} say <channel> <server> <message>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} taskstatus <name>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} watches`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} subscribe <channel_id> <subscription> <username>`"
			+ $"\n• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} unsubscribe <channel_id> <subscription> <username>`";
		#endregion
		#endregion

		#region OTHERS
		internal static readonly string settingsMsg = "**[SCOPE - *APPLICATION*]**"
													+ $"\n**AppName** • {Configuration.Default.AppName}"
													+ $"\n**AppVersion** • {Configuration.Default.AppVersion}"
													+ $"\n**AppUrl** • <{Configuration.Default.AppUrl}>"
													+ $"\n**PrefixCmd** • {Configuration.Default.PrefixCmd}"
													+ $"\n**BotCmd** • {Configuration.Default.BotCmd}"
													+ $"\n**AudioPath** • {Configuration.Default.AudioPath}"
													+ $"\n**DataPath** • {Configuration.Default.DataPath}"
													+ $"\n**LeaderboardCmd** • {Configuration.Default.LeaderboardCmd}"
													+ $"\n**DropboxFolderName** • {Configuration.Default.DropboxFolderName}"
													+ $"\n**LogChannelName** • {Configuration.Default.LogChannelName}"
													+ $"\n**StreamingRoleName** • {Configuration.Default.StreamingRoleName}"
													+ $"\n**WorldRecordRoleName** • {Configuration.Default.WorldRecordRoleName}"
													+ $"\n**TwitterDescription** • {Configuration.Default.TwitterDescription}"
													+ "\n**[SCOPE - *USER*]**"
													+ $"\n**RefreshTime** • {Configuration.Default.RefreshTime}"
													+ $"\n**BoardParameter** • {Configuration.Default.BoardParameter}"
													+ $"\n**AutoUpdate** • {Configuration.Default.AutoUpdate}"
													+ $"\n**GiveawayResetTime** • {Configuration.Default.GiveawayResetTime}"
													+ $"\n**GiveawayMaxTries** • {Configuration.Default.GiveawayMaxTries}"
													+ $"\n**GiveawayEnabled** • {Configuration.Default.GiveawayEnabled}"
													+ $"\n**CachingTime** • {Configuration.Default.CachingTime}";

		internal static readonly string serverSpecs = "**Architecture**\n • ARMv8 64/32-bit"
													+ "\n**SoC**\n • Broadcom BCM2837"
													+ "\n**CPU**\n • 1.2 GHz 64-bit quad-core ARM Cortex-A53"
													+ "\n**GPU**\n • Broadcom VideoCore IV\n• OpenGL ES 2.0"
													+ "\n**RAM**\n • 1GB"
													+ "\n**Network**\n • 10/100 Ethernet\n• 802.11n Wireless";

		internal static readonly string msgEnd = $"\n**Notes**\n• Some commands don't require a parameter.\n• Try `{Configuration.Default.PrefixCmd}help <command>` for more infromation.";
		internal static readonly string[] botGreetings = { "Hey!", "Yo!", "Hi!", "Yoo!", "Hello!", "Hej!", "Hallo!", "Hola!", "Salut!" };
		internal static readonly string[] botFeelings = { ":grinning:", ":grimacing:", ":grin:", ":smiley:", ":smile:", ":sweat_smile:", ":wink:", ":slight_smile:", ":rage:", ":yum:", ":blush:", ":robot:", ":thumbsup:", ":ok_hand:", ":v:", ":heart:" };

		internal static readonly string[] botAnswers =
		{
			"Yes.", "Yeah.", "Ye.", ":thumbsup:",
			"No.", "**NO.**", "Nah.", ":thumbsdown:",
			"Oke.", "OK.", "Of course.", ":ok_hand:",
			"Maybe.", "Perhaps.", "Obviously...", "Good question...",
			"What do yo mean?", "I Bad understund what you written here.", "What do you think?"
		};

		internal static readonly UserStatus[] botStatus =
		{
			UserStatus.Online,
			UserStatus.Idle,
			UserStatus.DoNotDisturb
		};
		#endregion
	}
}