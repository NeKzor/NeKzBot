using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Private.Owner
{
	public class Admin : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Admin Module", LogColor.Init);
			await AdminCommands(Configuration.Default.BotCmd);
		}

		private static Task AdminCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("newgame")
						.Alias("playnext", "nextgame", "ng")
						.Description("Sets a random playing game status.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e => Bot.Client.SetGame(await Utils.RngAsync(Data.RandomGames) as string));

				GBuilder.CreateCommand("setgame")
						.Alias("play", "sg")
						.Description("Sets a new playing game status.")
						.Parameter("name", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (!(string.IsNullOrEmpty(e.Args[0])))
							{
								if (e.Args[0] != Bot.Client.CurrentGame.Name)
								{
									Bot.Client.SetGame(e.Args[0]);
									await e.Channel.SendMessage($"Bot is now playing **{e.Args[0]}**.");
								}
								else
									await e.Channel.SendMessage("Bot is already playing that game.");
							}
							else
								await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
						});

				GBuilder.CreateCommand("echo")
						.Alias("say")
						.Description("Returns the given message.")
						.Parameter("message", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Utils.CutMessage(e.GetArg("message")));
						});

				GBuilder.CreateCommand("send")
						.Description("Sends the given message to a specific channel of a guild. Use the keyword _this_ to skip the guild id parameter.")
						.Parameter("guild_id", ParameterType.Required)
						.Parameter("channel", ParameterType.Required)
						.Parameter("message", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							if (string.IsNullOrEmpty(e.GetArg("message")))
							{
								await e.Channel.SendMessage("Message text can't be empty.");
								return;
							}

							var server = default(Discord.Server);
							if (ulong.TryParse(e.GetArg("guild_id"), out var guiid))
								server = await Utils.FindServerById(guiid);
							else
								server = e.Server;

							var channel = default(Channel);
							if (server != null)
							{
								channel = await Utils.FindTextChannelByName(e.GetArg("channel"), server);
								var permissions = e.Channel.Users.FirstOrDefault(x => x.Id == Bot.Client.CurrentUser.Id).GetPermissions(channel);
								if (!(permissions.ReadMessages))
								{
									await e.Channel.SendMessage("Cannot send a message to a hidden channel.");
									return;
								}
								if (!(permissions.SendMessages))
								{
									await e.Channel.SendMessage("Cannot send messages without a permission.");
									return;
								}

								if (channel == null)
								{
									await e.Channel.SendMessage("Could not find the channel.");
									return;
								}
							}
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage(await Utils.CutMessage(e.GetArg("message")));
								return;
							}

							await channel.SendIsTyping();
							await channel.SendMessage(await Utils.CutMessage(e.GetArg("message")));
						});
			});
			return Task.FromResult(0);
		}
	}
}