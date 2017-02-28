using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks.Speedrun;

namespace NeKzBot.Modules.Public
{
	public class Speedrun : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Speedrun Module", LogColor.Init);
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
					.Description("Returns the latest world record of the fastest category.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameWorldRecordAsync(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindPlayerPersonalBests(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}pb")
					.Description("Shows you the personal bests of a player.")
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
					.Description("Returns some info about the game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameInfo(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindPlayer(string c)
		{
			CService.CreateCommand(c)
					.Description("Returns some info about a player.")
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
					.Description("Returns all world record of each category.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameWorldRecordsAsync(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task GetTopTen(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}top10", $"{Configuration.Default.PrefixCmd}topten", $"{Configuration.Default.PrefixCmd}10")
					.Description("Returns the top ten ranking of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetTopTenAsync(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindModerators(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}mods")
					.Description("Returns the moderator list of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						await e.Channel.SendMessage(await SpeedrunCom.GetModerators(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task GetWorldRecordStatus(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}haswr?", $"{Configuration.Default.PrefixCmd}wr?", $"{Configuration.Default.PrefixCmd}isfast?")
					.Description("Checks if a player has a world record.")
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
					.Description("Returns latest notifications. Enter the keyword _x_ to skip the count parameter.")
					.Parameter("count", ParameterType.Required)
					.Parameter("type", ParameterType.Optional)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.GetArg("type"))))
							await e.Channel.SendMessage(await SpeedrunCom.GetLastNotificationAsync(e.GetArg("count"), e.GetArg("type")));
						else
							await e.Channel.SendMessage(await SpeedrunCom.GetLastNotificationAsync(e.GetArg("count")));
					});
			return Task.FromResult(0);
		}

		private static Task GetFullGameRules(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}rules?")
					.Description("Returns the main rules of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameRules(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task GetIndividualLevelRules(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}ilrules?")
					.Description("Returns the main IL rules of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await SpeedrunCom.GetGameRules(e.Args[0], true));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});
			return Task.FromResult(0);
		}
	}
}