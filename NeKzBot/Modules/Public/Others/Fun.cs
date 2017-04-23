using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public.Others
{
	public class Fun : CommandModule
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
						await e.Channel.SendMessage(await Utils.RngAsync((await Data.Get<Simple>("cc")).Value));
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
						var data = (await Data.Get<Complex>("exploits")).Values;
						var rand = data[await Utils.RngAsync(data.Count)].Value;
						await e.Channel.SendMessage($"**{rand[0]}**\n{rand[1]}");
					});
			return Task.FromResult(0);
		}

		public static Task GetRandomFact(string c)
		{
			CService.CreateCommand(c)
					.Alias("fact")
					.Description("Gives you a random text about a random topic. Note: These quotes might not be exact and can sometimes be false.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var data = (await Data.Get<Complex>("quotes")).Values;
						await e.Channel.SendMessage($"*{data[await Utils.RngAsync(data.Count)].Value[1]}*");
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
						await e.Channel.SendMessage((e.Args[0] != "kys") ? await Utils.RngStringAsync(Data.BotFeelings) : await Utils.RisAsync("hopes deleted") + " :robot:");
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
					.Alias("rootcredit")
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
						await msg.Edit($"{credit} **{await Utils.AsRawText((string.IsNullOrEmpty(rand.Nickname)) ? rand.Name : rand.Nickname)}**");
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
						else if (question[question.Length - 1] == '?'
						&& (question.Any(c => char.IsLetter(c))))
							await e.Channel.SendMessage(await Utils.RngStringAsync(Data.BotAnswers));
						else
							await e.Channel.SendMessage(await Utils.RngStringAsync("Is this a question?", "This isn't a question.", "Please...", "lol"));
					});
			return Task.FromResult(0);
		}
	}
}