using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public.Others
{
	public class Fun : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Fun Module", LogColor.Init);
			await GetRandomCheat(Data.CheatCommand);
			await GetRandomExploit(Data.ExploitCommand);
			await GetRandomFact("funfact");
			await OtherCommands();
		}

		public static Task GetRandomCheat(string c)
		{
			CService.CreateCommand(c)
					.Description("Returns a random console command. You can use it in Portal 2 challenge mode.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await Utils.RngAsync(Data.ConsoleCommands) as string);
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
						var rand = await Utils.RngAsync(Data.Portal2Exploits.GetLength(0));
						await e.Channel.SendMessage($"**{Data.Portal2Exploits[rand, 0]}**\n{Data.Portal2Exploits[rand, 1]}");
					});
			return Task.FromResult(0);
		}

		public static Task GetRandomFact(string c)
		{
			CService.CreateCommand(c)
					.Alias("fact")
					.Description("Gives you a random text about a random topic.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"*{await Utils.RngAsync(Data.QuoteNames, 1) as string}*");
					});
			return Task.FromResult(0);
		}

		public static Task OtherCommands()
		{
			// It all started here
			CService.CreateCommand("hello")
					.Alias("hi", "helloworld", "hey", "yo")
					.Description("Will greet you back.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{await Utils.RngStringAsync(Data.BotGreetings)} {await Utils.RngStringAsync(Data.BotFeelings)}");
					});

			CService.CreateCommand("bot")
					.Parameter("haHA", ParameterType.Unparsed)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await Utils.RngStringAsync(Data.BotFeelings));
					});

			// Convert text to symbols
			CService.CreateCommand("ris")
					.Description("Returns your message in regional indicator symbols.")
					.Parameter("text", ParameterType.Unparsed)
					.Do(async e =>
					{
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							var result = await Utils.RisAsync(e.Args[0]);
							if (result == string.Empty)
								return;
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(result);
						}
						else
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
						}
					});

			CService.CreateCommand("routecredit")
					.Description("Gives somebody route credit for no reason.")
					.AddCheck(Permissions.DisallowBots)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						const string credit = "Route credit goes to...";
						var msg = await e.Channel.SendMessage("Route credit goes to...");
						await e.Channel.SendIsTyping();
						await Task.Delay(5000);

						var rand = default(User);
						do
							rand = e.Server.Users.ElementAt(await Utils.RngAsync(e.Server.UserCount));
						while (rand.IsBot);
						await msg.Edit($"{credit} **{rand.Name}**");
					});

			CService.CreateCommand("question")
					.Alias("q", "??")
					.Description("Responses to a simple yes-no question.")
					.Parameter("question", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var question = e.Args[0];
						if (string.IsNullOrEmpty(question))
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
						else if (question[question.Length - 1] == '?')
							await e.Channel.SendMessage(await Utils.RngStringAsync(Data.BotAnswers));
						else
							await e.Channel.SendMessage(await Utils.RngStringAsync("Is this a question?", "This isn't a question.", "Please..."));
					});
			return Task.FromResult(0);
		}
	}
}