using NeKzBot.Server;
using System.Threading.Tasks;

namespace NeKzBot.Resources
{
	public class Data
	{
		public static string[] consoleCommands;
		public static string[] audioAliases;
		public static string[] randomGames;
		public static string[] killBotAliases;
		public static string[] specialThanks;
		public static string[] twitchStreamers;
		public static string[,] scriptFiles;
		public static string[,] memeCommands;
		public static string[,] toolCommands;
		public static string[,] linkCommands;
		public static string[,] projectNames;
		public static string[,] portal2Maps;
		public static string[,] quoteNames;
		public static string[,] soundNames;
		public static string[,] p2Exploits;

		public static readonly string cheatCmd = "cheat";
		public static readonly string creditCmd = "credits";
		public static readonly string exploitCmd = "exploit";
		public static readonly string killCmd = "kill";
		public static readonly string srunCmd = "segmented";

		public static readonly string fileExtension = ".dat";
		public static readonly string fileNameConsoleCommands = "consolecmds" + fileExtension;
		public static readonly string fileNameAudioAliases = "audioaliases" + fileExtension;
		public static readonly string fileNameRandomGames = "playingstatus" + fileExtension;
		public static readonly string fileNameKillBotAliases = "killbot" + fileExtension;
		public static readonly string fileNameSpecialThanks = "credits" + fileExtension;
		public static readonly string fileNameTwitchStreamers = "streamers" + fileExtension;
		public static readonly string fileNameScriptFiles = "scripts" + fileExtension;
		public static readonly string fileNameMemeCommands = "memes" + fileExtension;
		public static readonly string fileNameToolCommands = "tools" + fileExtension;
		public static readonly string fileNameLinkCommands = "links" + fileExtension;
		public static readonly string fileNameProjectNames = "runs" + fileExtension;
		public static readonly string fileNamePortal2Maps = "p2maps" + fileExtension;
		public static readonly string fileNameQuoteNames = "quotes" + fileExtension;
		public static readonly string fileNameSoundNames = "sounds" + fileExtension;
		public static readonly string fileNameP2Exploits = "exploits" + fileExtension;

		public static async Task InitConsoleCommands() => consoleCommands = (string[])(await Utils.ReadFromFile(fileNameConsoleCommands));
		public static async Task InitAudioAliases() => audioAliases = (string[])(await Utils.ReadFromFile(fileNameAudioAliases));
		public static async Task InitRandomGames() => randomGames = (string[])(await Utils.ReadFromFile(fileNameRandomGames));
		public static async Task InitkillBotAliases() => killBotAliases = (string[])(await Utils.ReadFromFile(fileNameKillBotAliases));
		public static async Task InitSpecialThanks() => specialThanks = (string[])(await Utils.ReadFromFile(fileNameSpecialThanks));
		public static async Task InitTwitchStreamers() => twitchStreamers = (string[])(await Utils.ReadFromFile(fileNameTwitchStreamers));
		public static async Task InitScriptFiles() => scriptFiles = (string[,])(await Utils.ReadFromFile(fileNameScriptFiles));
		public static async Task InitMemeCommands() => memeCommands = (string[,])(await Utils.ReadFromFile(fileNameMemeCommands));
		public static async Task InitToolCommands() => toolCommands = (string[,])(await Utils.ReadFromFile(fileNameToolCommands));
		public static async Task InitLinkCommands() => linkCommands = (string[,])(await Utils.ReadFromFile(fileNameLinkCommands));
		public static async Task InitProjectNames() => projectNames = (string[,])(await Utils.ReadFromFile(fileNameProjectNames));
		public static async Task InitPortal2Maps() => portal2Maps = (string[,])(await Utils.ReadFromFile(fileNamePortal2Maps));
		public static async Task InitQuoteNames() => quoteNames = (string[,])(await Utils.ReadFromFile(fileNameQuoteNames));
		public static async Task InitSoundNames() => soundNames = (string[,])(await Utils.ReadFromFile(fileNameSoundNames));
		public static async Task InitP2Exploits() => p2Exploits = (string[,])(await Utils.ReadFromFile(fileNameP2Exploits));

		public static async Task Init()
		{
			await Logging.CON("Initializing data", System.ConsoleColor.DarkYellow);
			await InitAudioAliases();
			await InitConsoleCommands();
			await InitkillBotAliases();
			await InitLinkCommands();
			await InitMemeCommands();
			await InitP2Exploits();
			await InitPortal2Maps();
			await InitProjectNames();
			await InitQuoteNames();
			await InitRandomGames();
			await InitScriptFiles();
			await InitSoundNames();
			await InitSpecialThanks();
			await InitToolCommands();
			await InitTwitchStreamers();
		}

		#region HELP COMMAND LIST
		public static string funMsg =
			"**[Fun & Useful Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}hello`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}cheat`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}exploit`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}script <name>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}dialogue <mapname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}segmented <projectname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}funfact`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}when`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}ris <text>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}credits`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}routecredit`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}<meme>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}<tool>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}quote <name>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}invite`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}idinfo`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}?? <question>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}view <mapname>`";

		public static string lbMsg = "\n**[Leaderboard Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}latestwr <mapname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}wr <mapname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rank <mapname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}player <rankname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}latestentry <mapname>`";

		public static string adminLbMsg = $"\n**[Leaderboard Commands - Bot Owner Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} refreshtime`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} setrefreshtime <time>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} setchannel <name>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} updatestate <state>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} boardparameter <parameter>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} toggleupdate`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} refreshnow`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} cleanentrycache`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} cachetime`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} setcachetime <time>`";

		public static string vcMsg = "\n**[Voice Channel Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}<soundname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}p2`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}yanni`";

		public static string botMsg = "\n**[Bot Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} uptime`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} location`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} info`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} help`";

		public static string gameMsg = "\n**[Game Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway resetwhen`";

		public static string adminGameMsg = $"\n**[Game Commands - Bot Owner Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway resettime`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway maxtries`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway resetnow`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway togglereset`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway setstate`";

		public static string masterAdminGameMsg =
			"\n**[Game Commands - Bot Owner Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway status`";

		public static string rpiMsg =
			$"\n**[Raspberry Pi Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi specs`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi date`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi uptime`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi temperature`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi os`";

		public static string srcomMsg =
			$"\n**[SpeedrunCom Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}wr <game>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}wrs <game>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}top <game>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}pbs <player>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}game <name>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}player <name>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}moderators <game>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}haswr <player>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}rules <game>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}ilrules <game>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}{Settings.Default.PrefixCmd}notification <count> <type>`";

		public static string dropboxMsg =
			$"\n**[Dropbox Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}cloud`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}dbfolder`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}dbdelete <file>`";

		public static string masterAdminDropboxMsg =
			$"\n**[Dropbox Commands - Bot Owner Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}dbdelete <folder> <file>`";

		public static string adminBotMsg =
			$"\n**[Bot Commands - Bot Owner Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} connect`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} disconnect`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} stop`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} newgame`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} cleanconfig`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} settings`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} setgame <name>`";

		public static string masterAdminBotMsg =
			"\n**[Bot Commands - Bot Owner Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} kill`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} add <cmdname> <value1> <value2> <etc.>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} delete <cmdname> <value>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} revive <name>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} reload`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} showdata <name>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} data`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} say <channel> <message>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} taskstatus <name>`";

		public static string settingsMsg =
			"**[SCOPE - *APPLICATION*]**"
			+ $"\n**AppName -** {Settings.Default.AppName}"
			+ $"\n**AppVersion -** {Settings.Default.AppVersion}"
			+ $"\n**AppUrl -** {Settings.Default.AppUrl}"
			+ $"\n**PrefixCmd -** {Settings.Default.PrefixCmd.ToString()}"
			+ $"\n**BotCmd -** {Settings.Default.BotCmd}"
			+ $"\n**AudioPath -** {Settings.Default.AudioPath}"
			+ $"\n**DataPath -** {Settings.Default.DataPath}"
			+ $"\n**LeaderboardCmd -** {Settings.Default.LeaderboardCmd}"
			+ $"\n**ApplicationPath -** {Settings.Default.ApplicationPath}"
			+ $"\n**NotificationChannelName -** {Settings.Default.NotificationChannelName}"
			+ $"\n**TwitchChannelName -** {Settings.Default.TwitchChannelName}"
			+ $"\n**DropboxFolderName -** {Settings.Default.DropboxFolderName}"
			+ $"\n**LogChannelName -** {Settings.Default.LogChannelName}"
			+ "\n**[SCOPE - *USER*]**"
			+ $"\n**UpdateChannelName -** {Settings.Default.UpdateChannelName}"
			+ $"\n**RefreshTime -** {Settings.Default.RefreshTime.ToString()}"
			+ $"\n**BoardParameter -** {Settings.Default.BoardParameter}"
			+ $"\n**AutoUpdate -** {Settings.Default.AutoUpdate.ToString()}"
			+ $"\n**GiveawayResetTime -** {Settings.Default.GiveawayResetTime.ToString()}"
			+ $"\n**GiveawayMaxTries -** {Settings.Default.GiveawayMaxTries.ToString()}"
			+ $"\n**GiveawayEnabled -** {Settings.Default.GiveawayEnabled.ToString()}"
			+ $"\n**CachingTime -** {Settings.Default.CachingTime.ToString()}";
		#endregion

		#region OTHERS
		public static string serverSpecs =
			"**Architecture\n-** ARMv8 64/32-bit"
			+ "\n**SoC\n-** Broadcom BCM2837"
			+ "\n**CPU\n-** 1.2 GHz 64-bit quad-core ARM Cortex-A53"
			+ "\n**GPU\n-** Broadcom VideoCore IV\n**-** OpenGL ES 2.0"
			+ "\n**RAM\n-** 1GB"
			+ "\n**Network\n-** 10/100 Ethernet\n**-** 802.11n Wireless";

		public static string msgEnd = $"\n**Note**\n**-** Game commands are in developement.\n**-** Some commands don't require a parameter.\n**-** Try `{Settings.Default.PrefixCmd}help <command>` for more infromation.";
		public static string[] botGreetings = { "Hey!", "Yo!", "Hi!", "Yoo!", "Hello!", "Hej!", "Hallo!", "Hola!", "Salut!" };
		public static string[] botFeelings = { ":grinning:", ":grimacing:", ":grin:", ":smiley:", ":smile:", ":sweat_smile:", ":wink:", ":slight_smile:", ":rage:", ":yum:", ":blush:", ":robot:", ":thumbsup:", ":ok_hand:", ":v:", ":heart:" };
		public static string[] botAnswers = { "Yes.", "**YEEE!**", "Yeah.", "Ye", "Oke", "OK", ":ok_hand:", ":thumbsup:", "No.", "**NO.**", "Nah.", ":thumbsdown:", "Maybe.", "Perhaps.", "What do yo mean?", "I not understund you.", "What do you think?" };
		#endregion
	}
}