using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Tasks;
using NeKzBot.Server;
using NeKzBot.Resources;
using NeKzBot.Modules.Games;

namespace NeKzBot.Modules
{
	public class DataManager
	{
		public static object[,] dataCommands;
		public static List<string> rwCommands;

		public static async Task Init()
		{
			await Logging.CON("Initializing data manager", ConsoleColor.DarkYellow);
			dataCommands = new object[,]
			{
				// 0,		1,					2,			3			4					5
				// Command,	allow overwrite,	filepath,	variable,	command action,		initialize action
				{ "cc", true, Data.fileNameConsoleCommands, Data.consoleCommands, new Action(() => OtherCmds.GetRandomCheat(Data.cheatCmd)), new Action(async() => await Data.InitConsoleCommands()) },
				{ "credits", true, Data.fileNameSpecialThanks, Data.specialThanks, new Action(() => OtherCmds.GetCredits(Data.creditCmd)), new Action(async() => await Data.InitSpecialThanks()) },
				{ "exploits", true, Data.fileNameP2Exploits, Data.p2Exploits, new Action(() => OtherCmds.GetRandomExploit(Data.exploitCmd)), new Action(async() => await Data.InitP2Exploits()) },
				{ "killbota", true, Data.fileNameKillBotAliases, Data.killBotAliases, new Action(() => BotCmds.CreateKillBotCommands(Settings.Default.BotCmd, Data.killCmd)), new Action(async() => await Data.InitkillBotAliases()) },
				{ "links", true, Data.fileNameLinkCommands, Data.linkCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Links(Utils.index), 0, Data.linkCommands)), new Action(async() => await Data.InitLinkCommands()) },
				{ "memes", true, Data.fileNameMemeCommands, Data.memeCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Memes(Utils.index), 0, Data.memeCommands)), new Action(async() => await Data.InitMemeCommands()) },
				{ "playingstatus", true, Data.fileNameRandomGames, Data.randomGames, null, new Action(async() => await Data.InitRandomGames()) },
				{ "quotes", true, Data.fileNameQuoteNames, Data.quoteNames, new Action(() => Utils.CommandCreator(() => OtherCmds.Text(Utils.index), 0, Data.quoteNames)), new Action(async() => await Data.InitQuoteNames()) },
				{ "runs", true, Data.fileNameProjectNames, Data.projectNames, new Action(() => OtherCmds.GetSegmentedRun(Data.srunCmd)), new Action(async() => await Data.InitProjectNames()) },
				{ "tools", true, Data.fileNameToolCommands, Data.toolCommands, new Action(() => Utils.CommandCreator(() => OtherCmds.Tools(Utils.index), 0, Data.toolCommands)), new Action(async() => await Data.InitToolCommands()) },
				{ "aa", false, Data.fileNameAudioAliases, Data.audioAliases, null, new Action(async() => await Data.InitAudioAliases()) },
				{ "p2maps", false, Data.fileNamePortal2Maps, Data.portal2Maps, null, new Action(async() => await Data.InitPortal2Maps()) },
				{ "scripts", false, Data.fileNameScriptFiles, Data.scriptFiles, null, new Action(async() => await Data.InitScriptFiles()) },
				{ "sounds", false, Data.fileNameSoundNames, Data.soundNames, null, new Action(async() => await Data.InitSoundNames()) },
				{ "twitch", true, Data.fileNameTwitchStreamers, Data.twitchStreamers, null, new Action(async() => await Data.InitTwitchStreamers()) }
			};
			rwCommands = new List<string>();
			for (int i = 0; i < dataCommands.GetLength(0); i++)
				if ((bool)dataCommands[i, 1])
					rwCommands.Add((string)dataCommands[i, 0]);
		}

		public static async Task LoadModules()
		{
			await Logging.CON("Loading modules", ConsoleColor.DarkYellow);
			await BotCmds.Load();
			await LeaderboardCmds.Load();
			await VoiceChannelCmds.Load();
			await HelpCmds.Load();
			await OtherCmds.Load();
			await GiveawayGame.Load();
			await SpeedrunCmds.Load();
		}

		public static async Task<bool> Reload(int index)
		{
			try
			{
				// Reload new data
				((Action)dataCommands[index, 5]).Invoke();
				// Reload command
				((Action)dataCommands[index, 4])?.Invoke();
				// Reload manager
				await Init();
				return true;
			}
			catch (Exception ex)
			{
				await Logging.CON("DataManager reload error", ex);
				return false;
			}
		}
	}
}