using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Modules.Speedrun;

namespace NeKzBot.Tasks
{
	public class SpeedrunCmds : Commands
	{
		public static async Task Load()
		{
			await Logging.CON("Loading speedrun commands", System.ConsoleColor.DarkYellow);
			await FindGameWR($"{Settings.Default.PrefixCmd}wr");
			await FindPlayerPBs($"{Settings.Default.PrefixCmd}pbs");
			await FindGame($"{Settings.Default.PrefixCmd}game");
			await FindPlayer($"{Settings.Default.PrefixCmd}player");
			await FindModerators($"{Settings.Default.PrefixCmd}moderators");
			await GetTopTen($"{Settings.Default.PrefixCmd}top");
			await GetWorldRecordStatus($"{Settings.Default.PrefixCmd}haswr");
			await GetAllGameWorldRecords($"{Settings.Default.PrefixCmd}wrs");
			await GetGameRules($"{Settings.Default.PrefixCmd}rules");
			await GetILGameRules($"{Settings.Default.PrefixCmd}ilrules");
			await GetNotification($"{Settings.Default.PrefixCmd}notification");
		}

		private static Task FindGameWR(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` lists you every category world record of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetGameWorldRecord(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task FindPlayerPBs(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}pb")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <player>` shows you the personal bests of a player.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetPersonalBestOfPlayer(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task FindGame(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns some info about the game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetGameInfo(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task FindPlayer(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <player>` returns some info about a player.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetPlayerInfo(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task GetAllGameWorldRecords(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns all world record of each category.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetGameWorldRecords(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task GetTopTen(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}top10", $"{Settings.Default.PrefixCmd}topten", $"{Settings.Default.PrefixCmd}10")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns the top ten ranking of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetTopTen(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task FindModerators(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}mods")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` returns the moderator list of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetModerators(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task GetWorldRecordStatus(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}haswr?", $"{Settings.Default.PrefixCmd}wr?", $"{Settings.Default.PrefixCmd}isfast?")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <player>` ask the bot if the player has a world record.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"**{SpeedrunCom.PlayerHasWorldRecord(e.Args[0])}**");
			});
			return Task.FromResult(0);
		}

		private static Task GetNotification(string c)
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
			return Task.FromResult(0);
		}

		private static Task GetGameRules(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}rules?")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` return the main rules of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetGameRules(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		private static Task GetILGameRules(string c)
		{
			cmd.CreateCommand(c)
			.Alias($"{Settings.Default.PrefixCmd}ilrules?")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <game>` return the main il rules of a game.\n**-** Data provided by speedrun.com.")
			.Parameter("p", Discord.Commands.ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(SpeedrunCom.GetGameRules(e.Args[0], true));
			});
			return Task.FromResult(0);
		}
	}
}