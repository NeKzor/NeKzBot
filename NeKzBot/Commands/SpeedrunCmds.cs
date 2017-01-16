using NeKzBot.Server;

namespace NeKzBot
{
	public class SpeedrunCmds : Commands
	{
		public static void Load()
		{
			Logging.CON("Loading speedrun commands", System.ConsoleColor.DarkYellow);
			FindGameWR($"{Settings.Default.PrefixCmd}wr");
			FindPlayerPBs($"{Settings.Default.PrefixCmd}pbs");
			FindGame($"{Settings.Default.PrefixCmd}game");
			FindPlayer($"{Settings.Default.PrefixCmd}player");
			FindModerators($"{Settings.Default.PrefixCmd}moderators");
			GetTopTen($"{Settings.Default.PrefixCmd}top");
			GetWorldRecordStatus($"{Settings.Default.PrefixCmd}haswr");
			GetAllGameWorldRecords($"{Settings.Default.PrefixCmd}wrs");
			GetGameRules($"{Settings.Default.PrefixCmd}rules");
			GetILGameRules($"{Settings.Default.PrefixCmd}ilrules");
			GetNotification($"{Settings.Default.PrefixCmd}notification");
		}

		private static void FindGameWR(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` lists you every category world record of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetGameWorldRecord(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <game>`");
			});
		}

		private static void FindPlayerPBs(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}pb")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <player>` shows you the personal bests of a player.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetPersonalBestOfPlayer(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <playe>`");
			});
		}

		private static void FindGame(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns some info about the game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetGameInfo(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <game>`");
			});
		}

		private static void FindPlayer(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <player>` returns some info about a player.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetPlayerInfo(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <playe>`");
			});
		}

		private static void GetAllGameWorldRecords(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns all world record of each category.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetGameWorldRecords(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <game>`");
			});
		}

		private static void GetTopTen(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}top10", $"{Settings.Default.PrefixCmd}topten", $"{Settings.Default.PrefixCmd}10")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns the top ten ranking of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetTopTen(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <game>`");
			});
		}

		private static void FindModerators(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}mods")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns the moderator list of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetModerators(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <game>`");
			});
		}

		private static void GetWorldRecordStatus(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}haswr?", $"{Settings.Default.PrefixCmd}wr?", $"{Settings.Default.PrefixCmd}isfast?")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <player>` ask the bot if the player has a world record.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage($"**{SpeedrunCom.PlayerHasWorldRecord(e.Args[0])}**");
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <player>`");
			});
		}

		private static void GetNotification(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}nf", $"{Settings.Default.PrefixCmd}news")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <count> <type>` returns latest notifications.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				var values = e.Args[0].Split(' ');
				var pcount = values.GetLength(0);
				if (values[0] == string.Empty)
					await e.Channel.SendMessage(await SpeedrunCom.GetLastNotification());
				else if (pcount == 1)
					await e.Channel.SendMessage(await SpeedrunCom.GetLastNotification(values[0]));
				else if (pcount == 2)
					await e.Channel.SendMessage(await SpeedrunCom.GetLastNotification(values[0], values[1]));
				else
					await e.Channel.SendMessage($"Invalid parameter count. Try `{Settings.Default.PrefixCmd + c} <count> <type>`");
			});
		}

		private static void GetGameRules(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}rules?")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` return the main rules of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetGameRules(e.Args[0]));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <game>`");
			});
		}

		private static void GetILGameRules(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}ilrules?")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` return the main il rules of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] != string.Empty)
					await e.Channel.SendMessage(SpeedrunCom.GetGameRules(e.Args[0], true));
				else
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <game>`");
			});
		}
	}
}