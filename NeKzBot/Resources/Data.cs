using NeKzBot.Properties;

namespace NeKzBot
{
	public class Data
	{
		public static string[] consoleCommands;
		public static string[] audioAliases;
		public static string[] randomGames;
		public static string[] killBotAliases;
		public static string[] specialThanks;
		public static string[,] scriptFiles;
		public static string[,] memeCommands;
		public static string[,] toolCommands;
		public static string[,] linkCommands;
		public static string[,] projectNames;
		public static string[,] portal2Maps;
		public static string[,] quoteNames;
		public static string[,] soundNames;
		public static string[,] p2Exploits;

		public static void InitConsoleCommands() => consoleCommands = (string[])Utils.ReadFromFile("consolecmds.dat");
		public static void InitAudioAliases() => audioAliases = (string[])Utils.ReadFromFile("audioaliases.dat");
		public static void InitRandomGames() => randomGames = (string[])Utils.ReadFromFile("playingstatus.dat");
		public static void InitkillBotAliases() => killBotAliases = (string[])Utils.ReadFromFile("killbot.dat");
		public static void InitSpecialThanks() => specialThanks = (string[])Utils.ReadFromFile("credits.dat");
		public static void InitScriptFiles() => scriptFiles = (string[,])Utils.ReadFromFile("scripts.dat");
		public static void InitMemeCommands() => memeCommands = (string[,])Utils.ReadFromFile("memes.dat");
		public static void InitToolCommands() => toolCommands = (string[,])Utils.ReadFromFile("tools.dat");
		public static void InitLinkCommands() => linkCommands = (string[,])Utils.ReadFromFile("links.dat");
		public static void InitProjectNames() => projectNames = (string[,])Utils.ReadFromFile("runs.dat");
		public static void InitPortal2Maps() => portal2Maps = (string[,])Utils.ReadFromFile("p2maps.dat");
		public static void InitQuoteNames() => quoteNames = (string[,])Utils.ReadFromFile("quotes.dat");
		public static void InitSoundNames() => soundNames = (string[,])Utils.ReadFromFile("sounds.dat");
		public static void InitP2Exploits() => p2Exploits = (string[,])Utils.ReadFromFile("exploits.dat");

		public static readonly string cheatCmd = "cheat";
		public static readonly string creditCmd = "credits";
		public static readonly string exploitCmd = "exploit";
		public static readonly string killCmd = "kill";
		public static readonly string srunCmd = "segmented";

		public static void Init()
		{
			Logging.CON("Initializing data", System.ConsoleColor.DarkYellow);
			InitAudioAliases();
			InitConsoleCommands();
			InitkillBotAliases();
			InitLinkCommands();
			InitMemeCommands();
			InitP2Exploits();
			InitPortal2Maps();
			InitProjectNames();
			InitQuoteNames();
			InitRandomGames();
			InitScriptFiles();
			InitSoundNames();
			InitSpecialThanks();
			InitToolCommands();
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
					+ $"\n**-** `{Settings.Default.PrefixCmd}quote <name>`";

		public static string lbMsg = "\n**[Leaderboard Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}latestwr <mapname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}wr <mapname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rank <mapname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}player <rankname>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}latestentry <mapname>`";

		public static string adminLbMsg = $"\n**[Leaderboard Commands - {Utils.CollectionToList(Settings.Default.AllowedRoles)} Only]**"
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

		public static string adminGameMsg = $"\n**[Game Commands - {Utils.CollectionToList(Settings.Default.AllowedRoles)} Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway resettime`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway maxtries`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway resetnow`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway togglereset`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway setstate`";

		public static string masterAdminGameMsg =
			"\n**[Game Commands - Master-Server Admin Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}giveaway status`";

		public static string rpiMsg =
			$"\n**[Raspberry Pi Commands]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi specs`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi date`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi uptime`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi temperature`"
			+ $"\n**-** `{Settings.Default.PrefixCmd}rpi os`";

		public static string adminBotMsg =
			$"\n**[Bot Commands - {Utils.CollectionToList(Settings.Default.AllowedRoles)} Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} connect`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} disconnect`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} stop`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} newgame`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} cleanconfig`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} settings`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} setgame <name>`";

		public static string masterAdminBotMsg =
			"\n**[Bot Commands - Master-Server Admin Only]**"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} kill`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} add <cmdname> <value1> <value2> <etc.>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} delete <cmdname> <value>`"
			+ $"\n**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} data`";

		public static string settingsMsg =
							"**[Scope - *Application*]**"
							+ "\n**Token -** *hidden*"
							+ $"\n**AppName -** {Settings.Default.AppName}"
							+ $"\n**AppVersion -** {Settings.Default.AppVersion}"
							+ $"\n**AppUrl -** {Settings.Default.AppUrl}"
							+ $"\n**PrefixCmd -** {Settings.Default.PrefixCmd.ToString()}"
							+ $"\n**BotCmd -** {Settings.Default.BotCmd}"
							+ $"\n**ServerName -** {Settings.Default.ServerName}"
							+ $"\n**AllowedRoles -** {Utils.CollectionToList(Settings.Default.AllowedRoles, "`")}"
							+ $"\n**AudioPath -** {Settings.Default.AudioPath}"
							+ "\n**GiveawayPrize -** *hidden*"
							+ "\n**GiveawayCode -** *hidden*"
							+ $"\n**MasterAdminID -** {Settings.Default.MaseterAdminID.ToString()}"
							+ $"\n**DataPath -** {Settings.Default.DataPath}"
							+ $"\n**DataCaching -** {Settings.Default.DataCaching.ToString()}"
							+ $"\n**LeaderboardCmd -** {Settings.Default.LeaderboardCmd}"
							+ $"\n**ApplicationPath -** {Settings.Default.ApplicationPath}"
							+ "\n**[Scope - *User*]**"
							+ $"\n**UpdateChannelName -** {Settings.Default.UpdateChannelName}"
							+ $"\n**RefreshTime -** {Settings.Default.RefreshTime.ToString()}"
							+ $"\n**BoardParameter -** {Settings.Default.BoardParameter}"
							+ $"\n**AutoUpdate -** {Settings.Default.AutoUpdate.ToString()}"
							+ $"\n**EntryCache -** {Settings.Default.EntryCache}"
							+ $"\n**UnlockedIndex -** {Settings.Default.UnlockedIndex.ToString()}"
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
		public static string msgAll = funMsg + lbMsg + adminLbMsg + vcMsg + gameMsg + adminGameMsg + masterAdminGameMsg + rpiMsg + botMsg + adminBotMsg + masterAdminBotMsg + msgEnd;
		public static string rolesMsg = $"You are not allowed to do that.\nAllowed roles: {Utils.CollectionToList(Settings.Default.AllowedRoles, "`")}";
		#endregion
	}
}