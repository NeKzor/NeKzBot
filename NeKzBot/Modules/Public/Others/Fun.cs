using System.Threading.Tasks;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public.Others
{
	public class Fun : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Fun Module", LogColor.Init);
			await GetRandomCheat(Data.CheatCommandd);
			await GetRandomExploit(Data.ExploitCommand);
			await GetRandomFact("funfact");
		}

		public static Task GetRandomCheat(string c)
		{
			CService.CreateCommand(c)
					.Description("Shows you a random console command. You can use it in Portal 2 challenge mode.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await Utils.RNGAsync(Data.ConsoleCommands) as string);
					});
			return Task.FromResult(0);
		}

		public static Task GetRandomExploit(string c)
		{
			CService.CreateCommand(c)
					.Alias("glitch")
					.Description("Returns a random Portal 2 exploit. You can use it for routing.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var rand = await Utils.RNGAsync(Data.Portal2Exploits.GetLength(0));
						await e.Channel.SendMessage($"**{Data.Portal2Exploits[rand, 0]}**\n{Data.Portal2Exploits[rand, 1]}");
					});
			return Task.FromResult(0);
		}

		public static Task GetRandomFact(string c)
		{
			CService.CreateCommand(c)
					.Alias("fact")
					.Description("Gives you a random text about a random topic.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"*{await Utils.RNGAsync(Data.QuoteNames, 1) as string}*");
					});
			return Task.FromResult(0);
		}
	}
}