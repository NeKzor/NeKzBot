using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Resources
{
	public static partial class Data
	{
		public static string ListModules;
		public static string FunPublic;
		public static string InfoPublic;
		public static string LeaderboardPublic;
		public static string SpeedrunComPublic;
		public static string OthersPublic;
		public static string LinuxOnly;
		public static string ResourcesPublic;
		public static string RestPublic;
		public static string VipServersOnly;
		public static string BotOwnerOnly;
		public static string SpecialPermissionsOnly;

		public static List<string> TwitterLocations { get; set; } = new List<string>
		{
			"Relaxation Vault", "SPAAAAAAAAACE", "Finale 5", "Incinerator",
			"Calibration Course", "Central AI Chamber", "Out Of Bounds", "Junk Yard"
		};

		public static readonly Color BoardColor = new Color(4, 128, 165);
		public static readonly Color BasicColor = new Color(14, 186, 83);
		public static readonly Color DropboxColor = new Color(0, 126, 229);
		public static readonly Color TwitchColor = new Color(100, 65, 164);
		public static readonly Color TwitterColor = new Color(29, 161, 242);
		public static readonly Color SpeedruncomColor = new Color(229, 227, 87);
		public static readonly Color SteamColor = new Color(22, 26, 33);

		public static readonly string MoreInformation = $"\n\nTry `{Configuration.Default.PrefixCmd}help <command>` for more information.";
		public static readonly string SubscriptionListMessage = $"List of available subscriptions:\n• `{Portal2WebhookKeyword}` updates you about the latest Portal 2 challenge mode world records on board.iverb.me." +
																$"\n• `{SpeedrunComSourceWebhookKeyword}` gets you the latest notifications about GoldSrc and Source Engine on speedrun.com." +
																$"\n• `{SpeedrunComPortal2WebhookKeyword}` gets you the latest Portal 2 notifications on speedrun.com." +
																$"\n• `{TwitchTvWebhookKeyword}` notifies you when somebody from the streaming list goes live (not recommended to use because this is a private list).";
		public static readonly string HiddenMessage = "**[Hidden Commands & Neat Shortcuts]**\n" +
													  $"• You can execute commands at the end of your text too: `this would also work {Configuration.Default.PrefixCmd}{Configuration.Default.BotCmd}`.\n" +
													  "• You can also use a mention to execute commands: `<@user_id> commands`.\n" +
													  "**[VIP Guilds Only]**\n" +
													  "• `10=tick` converts 10 ticks into seconds with the default Portal 2 tickrate 60.\n" +
													  "• `1=sec` converts 1 second into ticks with the default Portal 2 tickrate 60.\n" +
													  "• You can also set a custom tickrate like this: `1=sec66`.\n" +
													  "• You can create a startdemos command very quickly with this: `10->demo_`.\n" +
													  "• You can view the image preview of a Steam workshop item by just sending a valid uri which should have `https` or `http` in the beginning.\n" +
													  "• Source Engine demo files will be parsed automatically if you upload them as an attachment (parsing means that information about that demo will be returned).\n" +
													  $"• If you don't want to parse your demo then put `{Configuration.Default.PrefixCmd}donotparse` at the end of your text message.";

		public static readonly string[] BotGreetings = { "Hey!", "Yo!", "Hi!", "Hello!", "Hei!", "Hej!", "Hallo!", "Beep boop!", "Hola!", "Salut!", "Привет!", "Ciao!" };
		public static readonly string[] BotFeelings = { ":grinning:", ":grimacing:", ":grin:", ":smiley:", ":smile:", ":sweat_smile:", ":wink:", ":slight_smile:", ":rage:", ":yum:", ":blush:", ":robot:", ":thumbsup:", ":ok_hand:", ":v:", ":heart:" };
		public static readonly string[] BotAnswers =
		{
			"Yes.", "Yeah.", "Ye.", ":thumbsup:",
			"No.", "Nope.", "Nah.", ":thumbsdown:",
			"Of course.", ":ok_hand:",
			"Maybe.", "Perhaps.", "Obviously...",
			"I bad understund what you writen here.", "What do you think?"
		};

		public static readonly List<string> SubscriptionList = new List<string>()
		{
			Portal2WebhookKeyword,
			SpeedrunComSourceWebhookKeyword,
			SpeedrunComPortal2WebhookKeyword,
			TwitchTvWebhookKeyword
		};

		public static readonly UserStatus[] BotStatus =
		{
			UserStatus.Online,
			UserStatus.Idle,
			UserStatus.DoNotDisturb
		};

		public const string CheatCommand = "cheat";
		public const string CreditsCommand = "credits";
		public const string ExploitCommand = "exploit";
		public const string SegmentedRunCommand = "segmented";
		public const string Portal2RecordsIconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/NeKzBot/Resources/Public/portal2records_icon.png";
		public const string SpeedrunComIconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/NeKzBot/Resources/Public/speedruncom_icon.png";
		public const string TwitchTvIconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/NeKzBot/Resources/Public/twitchtv_icon.png";
		public const string SteamCommunityIconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/NeKzBot/Resources/Public/steamcommunity_icon.png";
		public const string Portal2RecordsWebhookAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/NeKzBot/Resources/Public/portal2records_webhookavatar.jpg";
		public const string SpeedrunComWebhookAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/NeKzBot/Resources/Public/speedruncom_webhookavatar.png";
		public const string TwitchTvWebhookAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/NeKzBot/Resources/Public/twitchtv_webhookavatar.png";
		public const string ServerSpecs = "**Architecture**\n • ARMv8 64/32-bit\n" +
										  "**SoC**\n • Broadcom BCM2837\n" +
										  "**CPU**\n • 1.2 GHz 64-bit quad-core ARM Cortex-A53\n" +
										  "**GPU**\n • Broadcom VideoCore IV\n• OpenGL ES 2.0\n" +
										  "**RAM**\n • 1GB\n" +
										  "**Network**\n • 10/100 Ethernet\n• 802.11n Wireless";
		public const string Portal2WebhookKeyword = "p2wrs";
		public const string SpeedrunComSourceWebhookKeyword = "source";
		public const string SpeedrunComPortal2WebhookKeyword = "portal2";
		public const string TwitchTvWebhookKeyword = "twitch";
		public const string LatestChangelog = "\n• Improved cleanup command" +
											  "\n• Fixed empty speedruncom resource notification" +
											  "\n• Portal2Board tasks are now 97% faster" +
											  "\n• Removed Portal2Board caching" +
											  "\n• Fixed event exceptions" +
											  "\n• Removed giveaway game" +
											  "\n• Some other cleanup here and there";

		public static async Task GenerateModuleListsAsync()
		{
			ListModules = await Utils.GenerateModuleListAsync("Available Modules", "Invoke one of them to show the full list.", commands: new[] { "fun", "info", "portal2", "speedrun", "other", "raspberry", "resource", "rest", "vip" });
			FunPublic = await Utils.GenerateModuleListAsync("Fun Module", commands: new[] { "cheat", "exploit", "funfact", "hello", "ris", "meme", "routecredit", "question" });
			InfoPublic = await Utils.GenerateModuleListAsync("Info Module", commands: new[] { "when", "idinfo", "bot uptime", "bot location", "bot info", "bot version", "bot changelog", "bot guilds" });
			LeaderboardPublic = await Utils.GenerateModuleListAsync("Portal 2 Leaderboard Module", commands: new[] { "latestwr", "wr", "rank", "player", "latestentry", "compare", "top", "analyze" });
			SpeedrunComPublic = await Utils.GenerateModuleListAsync("SpeedrunCom Module", specialprefix: Configuration.Default.PrefixCmd.ToString(), commands: new[] { "wr", "wrs", "top", "pbs", "game", "player", "moderators", "haswr", "rules", "ilrules", "notification", "category", "categories" });
			OthersPublic = await Utils.GenerateModuleListAsync("Other Module", commands: new[] { "meme", "tool", "link", "quote" });
			LinuxOnly = await Utils.GenerateModuleListAsync("Raspberry Pi Module", specialprefix: "rpi ", commands: new[] { "specs", "date", "uptime", "temperature", "os" });
			ResourcesPublic = await Utils.GenerateModuleListAsync("Resource Module", commands: new[] { "scripts", "dialogue", "segmented" });
			RestPublic = await Utils.GenerateModuleListAsync("Rest Module", commands: new[] { "invite", "join", "view", "credits" });
			VipServersOnly = await Utils.GenerateModuleListAsync("VIP Module", commands: new[] { "sound", "twitch", "upload", "cloud", "dbfolder", "dbdelete", "bot connect", "bot disconnect", "bot stop", "bot subscribe", "bot unsubscribe" });
			BotOwnerOnly = await Utils.GenerateModuleListAsync("Bot Module - Bot Owner Only", specialprefix: $"{Configuration.Default.BotCmd} ", commands: new[] { "newgame", "setgame", "echo", "send", "add", "delete", "reload", "showdata", "datavars", "cleanconfig", "revive", "taskstatus", "watches", "webhooktest", "react", "contestrules", "contestgame", "contestmap", "setcontest", "opencontest", "closecontest" });
			SpecialPermissionsOnly = await Utils.GenerateModuleListAsync("Bot Module - Special Permissions Only", specialprefix: $"{Configuration.Default.BotCmd} ", commands: new[] { "react", "setnickname", "pin", "cleanup", "cleanuprange" });
		}
	}
}