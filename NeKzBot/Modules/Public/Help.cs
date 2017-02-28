using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public
{
	public class Help : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Help Module", LogColor.Init);
			await HelpCommands();
		}

		private static Task HelpCommands()
		{
			CService.CreateCommand("help")
					.Alias("?")
					.Description("Returns the description of a command.")
					.Parameter("command", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await Utils.FindDescriptionAsync(e.Args[0]));
					});

			CService.CreateCommand("commands")
					.Alias("cmds")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							// Thanks for the 2k character limit, Discord
							await e.Channel.SendMessage(Data.FunMessage + Data.LeaderboardMessage + Data.VoiceChannelMessage + Data.MainServerMessage + Data.SpeedrunComMessage + Data.RaspberryPiMessage + Data.DropboxMessage + Data.BotMessage + Data.BotOwnerBotMessage + Data.MessagEnding);
						else if (Permissions.MainServerOnly(e.Server))
							await e.User.SendMessage(Data.FunMessage + Data.LeaderboardMessage + Data.VoiceChannelMessage + Data.MainServerMessage + Data.SpeedrunComMessage + Data.RaspberryPiMessage + Data.DropboxMessage + Data.BotMessage + Data.MessagEnding);
						else
							await e.User.SendMessage(Data.FunMessage + Data.LeaderboardMessage + Data.VoiceChannelMessage + Data.BotMessage + Data.MessagEnding);
					});

			CService.CreateCommand("fun")
					.Alias("fun?")
					.Description("Shows you a list of \"fun\" commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.FunMessage);
					});

			CService.CreateCommand("leaderboard")
					.Alias("lb", "lb?")
					.Description("Shows you a list of leaderboard commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.LeaderboardMessage + Data.BotOwnerLeaderboardMessage);
						else
							await e.Channel.SendMessage(Data.LeaderboardMessage);
					});

			CService.CreateCommand("sounds")
					.Alias("vc?")
					.Description("Shows you a list of sound commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.VoiceChannelMessage);
					});

			CService.CreateCommand("game")
					.Alias("game?")
					.Description("Shows you a list of sound commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.BotOwnerGameMessage);
						else if (Permissions.MainServerOnly(e.Server))
							await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}giveaway`.");
						else
							await e.Channel.SendMessage("There are no games on this server.");
					});

			CService.CreateCommand("rpi")
					.Alias("rpi?")
					.Description("Shows you a list of commands which give you information about the server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.RaspberryPiMessage);
					});

			CService.CreateCommand("speedrun")
					.Alias("speedruncom", "srcom", "srcom?")
					.Description("Shows you a list of speedruncom commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.SpeedrunComMessage);
					});

			CService.CreateCommand("dropbox")
					.Alias("dropbox", "db?")
					.Description("Shows you a list of Dropbox commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.DropboxMessage + Data.BotOwnerDropboxMessage);
						else
							await e.Channel.SendMessage(Data.DropboxMessage);
					});

			CService.CreateCommand("server")
					.Alias("servercmds", "mainserver")
					.Description("Shows you a list of server exclusive commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.MainServerMessage);
					});

			CService.CreateCommand("botcommands")
					.Alias("botcmds", "bot?")
					.Description("Shows you a list of bot commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.BotOwnerBotMessage);
						else
							await e.Channel.SendMessage(Data.BotMessage);
					});
			return Task.FromResult(0);
		}
	}
}