using System;
using System.Collections.Generic;

namespace NeKzBot
{
	public class CmdManager
	{
		public static object[,] dataCommands;
		public static List<string> rwCommands;

		public static void Init()
		{
			Logging.CON("Initializing cmd manager", ConsoleColor.DarkYellow);
			dataCommands = new object[,]
			{
				// 0,		1,					2,			3			4					5
				// Command,	allow overwrite,	filepath,	variable,	command action,		initialize action
				{ "cc", true, Data.fileNameConsoleCommands, Data.consoleCommands, new Action(() => OtherCmds.GetRandomCheat(Data.cheatCmd)), new Action(() => Data.InitConsoleCommands()) },
				{ "credits", true, Data.fileNameSpecialThanks, Data.specialThanks, new Action(() => OtherCmds.GetCredits(Data.creditCmd)), new Action(() => Data.InitSpecialThanks()) },
				{ "exploits", true, Data.fileNameP2Exploits, Data.p2Exploits, new Action(() => OtherCmds.GetRandomExploit(Data.exploitCmd)), new Action(() => Data.InitP2Exploits()) },
				{ "killbota", true, Data.fileNameKillBotAliases, Data.killBotAliases, new Action(() => BotCmds.CreateKillBotCommands(Properties.Settings.Default.BotCmd, Data.killCmd)), new Action(() => Data.InitkillBotAliases()) },
				{ "links", true, Data.fileNameLinkCommands, Data.linkCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Links(Utils.index), 0, Data.linkCommands)), new Action(() => Data.InitLinkCommands()) },
				{ "memes", true, Data.fileNameMemeCommands, Data.memeCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Memes(Utils.index), 0, Data.memeCommands)), new Action(() => Data.InitMemeCommands()) },
				{ "playingstatus", true, Data.fileNameRandomGames, Data.randomGames, null, new Action(() => Data.InitRandomGames()) },
				{ "quotes", true, Data.fileNameQuoteNames, Data.quoteNames, new Action(() => Utils.CommandCreator(() => OtherCmds.Text(Utils.index), 0, Data.quoteNames)), new Action(() => Data.InitQuoteNames()) },
				{ "runs", true, Data.fileNameProjectNames, Data.projectNames, new Action(() => OtherCmds.GetSegmentedRun(Data.srunCmd)), new Action(() => Data.InitProjectNames()) },
				{ "tools", true, Data.fileNameToolCommands, Data.toolCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Tools(Utils.index), 0, Data.toolCommands)), new Action(() => Data.InitToolCommands()) },
				{ "aa", false, Data.fileNameAudioAliases, Data.audioAliases, null, new Action(() => Data.InitAudioAliases()) },
				{ "p2maps", false, Data.fileNamePortal2Maps, Data.portal2Maps, null, new Action(() => Data.InitPortal2Maps()) },
				{ "scripts", false, Data.fileNameScriptFiles, Data.scriptFiles, null, new Action(() => Data.InitScriptFiles()) },
				{ "sounds", false, Data.fileNameSoundNames, Data.soundNames, null, new Action(() => Data.InitSoundNames()) },
				{ "twitch", true, Data.fileNameTwitchStreamers, Data.twitchStreamers, null, new Action(() => Data.InitTwitchStreamers()) }
			};
			rwCommands = new List<string>();
			for (int i = 0; i < dataCommands.GetLength(0); i++)
				if ((bool)dataCommands[i, 1])
					rwCommands.Add((string)dataCommands[i, 0]);
		}

		public static void LoadModules()
		{
			Logging.CON("Loading modules", ConsoleColor.DarkYellow);

			BotCmds.Load();                         // !bot <command>
			LeaderboardCmds.Load();                 // !rank, !player, !latestwr
			VoiceChannelCmds.Load();                // !<meme>, !yanni, !p2
			HelpCmds.Load();                        // !fun, !games, !lb, !sounds, !bot?
			OtherCmds.Load();						// !cheat, !exploit etc.
			GiveawayGame.Load();                    // !giveaway
			SpeedrunCmds.Load();
		}
	}
}