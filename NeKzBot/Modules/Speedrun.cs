using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Resources;
using NeKzBot.Tasks.Speedrun;
using Discord.Commands;

namespace NeKzBot.Modules
{
	public class Speedrun : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Speedrun Commands", LogColor.Init);
			await FindGameWorldRecord($"{Configuration.Default.PrefixCmd}wr");
			await FindPlayerPersonalBests($"{Configuration.Default.PrefixCmd}pbs");
			await FindGame($"{Configuration.Default.PrefixCmd}game");
			await FindPlayer($"{Configuration.Default.PrefixCmd}player");
			await FindModerators($"{Configuration.Default.PrefixCmd}moderators");
			await GetTopTen($"{Configuration.Default.PrefixCmd}top");
			await GetWorldRecordStatus($"{Configuration.Default.PrefixCmd}haswr");
			await GetAllGameWorldRecords($"{Configuration.Default.PrefixCmd}wrs");
			await GetFullGameRules($"{Configuration.Default.PrefixCmd}rules");
			await GetIndividualLevelRules($"{Configuration.Default.PrefixCmd}ilrules");
			await GetNotification($"{Configuration.Default.PrefixCmd}notification");
		}

		private static Task FindGameWorldRecord(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c} <game>` returns the latest world record of the fastet category.\n• Data provided by speedrun.com.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameWorldRecordAsync(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync(c));
					});
			return Task.FromResult(0);
		}

		private static Task FindPlayerPersonalBests(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}pb")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <player>` shows you the personal bests of a player.\n• Data provided by speedrun.com.")
					.Parameter("player", ParameterType.Required)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await SpeedrunCom.GetPersonalBestOfPlayerAsync(e.Args[0]));
					});
			return Task.FromResult(0);
		}

		private static Task FindGame(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c} <game>` returns some info about the game.\n• Data provided by speedrun.com.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameInfo(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync(c));
					});
			return Task.FromResult(0);
		}

		private static Task FindPlayer(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c} <player>` returns some info about a player.\n• Data provided by speedrun.com.")
					.Parameter("player", ParameterType.Required)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await SpeedrunCom.GetPlayerInfo(e.Args[0]));
					});
			return Task.FromResult(0);
		}

		private static Task GetAllGameWorldRecords(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c} <game>` returns all world record of each category.\n• Data provided by speedrun.com.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameWorldRecordsAsync(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync(c));
					});
			return Task.FromResult(0);
		}

		private static Task GetTopTen(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}top10", $"{Configuration.Default.PrefixCmd}topten", $"{Configuration.Default.PrefixCmd}10")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <game>` returns the top ten ranking of a game.\n• Data provided by speedrun.com.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetTopTenAsync(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync(c));
					});
			return Task.FromResult(0);
		}

		private static Task FindModerators(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}mods")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <game>` returns the moderator list of a game.\n• Data provided by speedrun.com.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						await e.Channel.SendMessage(await SpeedrunCom.GetModerators(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync(c));
					});
			return Task.FromResult(0);
		}

		private static Task GetWorldRecordStatus(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}haswr?", $"{Configuration.Default.PrefixCmd}wr?", $"{Configuration.Default.PrefixCmd}isfast?")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <player>` ask the bot if the player has a world record.\n• Data provided by speedrun.com.")
					.Parameter("player", ParameterType.Required)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"**{await SpeedrunCom.PlayerHasWorldRecord(e.Args[0])}**");
					});
			return Task.FromResult(0);
		}

		private static Task GetNotification(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}nf", $"{Configuration.Default.PrefixCmd}news")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <count> <type>` returns latest notifications.\n• Data provided by speedrun.com.")
					.Parameter("count", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var values = e.Args[0].Split(' ');
						var pcount = values.GetLength(0);
						if (values[0] == string.Empty)
							await e.Channel.SendMessage(await SpeedrunCom.GetLastNotificationAsync());
						else if (pcount == 1)
							await e.Channel.SendMessage(await SpeedrunCom.GetLastNotificationAsync(values[0]));
						else if (pcount == 2)
							await e.Channel.SendMessage(await SpeedrunCom.GetLastNotificationAsync(values[0], values[1]));
						else
							await e.Channel.SendMessage($"Invalid parameter count. Try `{Configuration.Default.PrefixCmd + c} <count> <type>`");
					});
			return Task.FromResult(0);
		}

		private static Task GetFullGameRules(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}rules?")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <game>` return the main rules of a game.\n• Data provided by speedrun.com.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameRules(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync(c));
					});
			return Task.FromResult(0);
		}

		private static Task GetIndividualLevelRules(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}ilrules?")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <game>` return the main il rules of a game.\n• Data provided by speedrun.com.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameRules(e.Args[0], true));
						else
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync(c));
					});
			return Task.FromResult(0);
		}
	}
}