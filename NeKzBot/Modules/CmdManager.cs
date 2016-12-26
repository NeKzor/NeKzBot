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
				{ "cc", true, "consolecmds.dat", Data.consoleCommands, new Action(() => OtherCmds.GetRandomCheat(Data.cheatCmd)), new Action(() => Data.InitConsoleCommands()) },
				{ "credits", true, "credits.dat", Data.specialThanks, new Action(() => OtherCmds.GetCredits(Data.creditCmd)), new Action(() => Data.InitSpecialThanks()) },
				{ "exploits", true, "exploits.dat", Data.p2Exploits, new Action(() => OtherCmds.GetRandomExploit(Data.exploitCmd)), new Action(() => Data.InitP2Exploits()) },
				{ "killbota", true, "killbot.dat", Data.killBotAliases, new Action(() => BotCmds.CreateKillBotCommands(Properties.Settings.Default.BotCmd, Data.killCmd)), new Action(() => Data.InitkillBotAliases()) },
				{ "links", true, "links.dat", Data.linkCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Links(Utils.index), 0, Data.linkCommands)), new Action(() => Data.InitLinkCommands()) },
				{ "memes", true, "memes.dat", Data.memeCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Memes(Utils.index), 0, Data.memeCommands)), new Action(() => Data.InitMemeCommands()) },
				{ "playingstatus", true, "playingstatus.dat", Data.randomGames, null, new Action(() => Data.InitRandomGames()) },
				{ "quotes", true, "quotes.dat", Data.quoteNames, new Action(() => Utils.CommandCreator(() => OtherCmds.Text(Utils.index), 0, Data.quoteNames)), new Action(() => Data.InitQuoteNames()) },
				{ "runs", true, "runs.dat", Data.projectNames, new Action(() => OtherCmds.GetSegmentedRun(Data.srunCmd)), new Action(() => Data.InitProjectNames()) },
				{ "tools", true, "tools.dat", Data.toolCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Tools(Utils.index), 0, Data.toolCommands)), new Action(() => Data.InitToolCommands()) },
				{ "aa", false, "audioAliases.dat", Data.audioAliases, null, new Action(() => Data.InitAudioAliases()) },
				{ "p2maps", false, "p2maps.dat", Data.portal2Maps, null, new Action(() => Data.InitPortal2Maps()) },
				{ "scripts", false, "scripts.dat", Data.scriptFiles, null, new Action(() => Data.InitScriptFiles()) },
				{ "sounds", false, "sounds.dat", Data.soundNames, null, new Action(() => Data.InitSoundNames()) }
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
			//GiveawayGame.Load();					// !giveaway
		}
	}
}