using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;

namespace NeKzBot.Modules.Public.Others
{
	public class Rest : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Rest Module", LogColor.Init);
			await GetCredits(Data.CreditsCommand);
			await GetMapImage("view");
			await OtherCommands();
		}

		public static Task OtherCommands()
		{
			// Create server invite link
			CService.CreateCommand("invite")
					.Description("Creates a new temporary invite link of this server.")
					.Do(async e =>
					{
						if ((bool)e.Server.Users.FirstOrDefault(user => user.Id == Bot.Client.CurrentUser.Id)?.ServerPermissions.CreateInstantInvite)
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"https://discord.gg/{(await e.Server.CreateInvite()).Code}");
						}
					});

			// Get bot invite link
			CService.CreateCommand("join")
					.Description("Returns the bot invite link. Use this to invite the bot to your server.")
					.Do(async e =>
					{
						// Useful tool https://finitereality.github.io/permissions
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discordapp.com/api/oauth2/authorize?client_id={Bot.Client.CurrentUser.Id}&scope=bot&permissions={Configuration.Default.BotPermissions}");
					});

			// Twitch
			CService.CreateCommand("twitch")
					.Alias("stream")
					.Description("Shows the preview of a streamer from Twitch.")
					.Parameter("channel", ParameterType.Required)
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var preview = await Twitch.GetPreviewAsync(e.Args[0]);
						if (preview == null)
							await e.Channel.SendMessage(TwitchError.Generic);
						else if (preview == TwitchError.Offline)
							await e.Channel.SendMessage("Streamer is offline.");
						else
						{
							var path = $"{await Utils.GetPath()}/Resources/Cache/{e.Args[0]}-stream.jpg";
							await Fetching.GetFileAsync(preview, path);
							await e.Channel.SendFile(path);
						}
					});

			// Hidden
			CService.CreateCommand("devserver")
					.Description("Returns the static invite link of the main server.")
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discord.gg/{Credentials.Default.DiscordMainServerLinkId}");
					});
			return Task.FromResult(0);
		}

		public static Task GetCredits(string c)
		{
			// The credits
			CService.CreateCommand(c)
					.Description("Shows you a list of people who deserve some credit. It will be sent as a DM because the list is kinda long.")
					.Hide()
					.Do(async e => await (await e.User.CreatePMChannel())?.SendMessage($"**Special Thanks To**\n{await Utils.ArrayToList(Data.SpecialThanks.OrderBy(name => name).ToArray(), string.Empty, "\n", "• ")}\n\nNote: Names are sorted in alphabetical order."));
			return Task.FromResult(0);
		}

		public static Task GetMapImage(string c)
		{
			CService.CreateCommand(c)
					.Alias("image", "overview", "preview")
					.Description($"Returns a picture of a random Portal 2 map. Try `{Configuration.Default.PrefixCmd + c} <mapname>` to show a specific image of a level.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] == string.Empty)
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/maps/{await Utils.RngAsync(Data.Portal2Maps, 0) as string}.jpg");
						else if (await Utils.SearchArray(Data.Portal2Maps, 2, e.Args[0], out var index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/maps/{Data.Portal2Maps[index, 0]}.jpg");
						else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.Args[0], out index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/maps/{Data.Portal2Maps[index, 0]}.jpg");
						else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.Args[0], out index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/maps/{Data.Portal2Maps[index, 0]}.jpg");
						else
							await e.Channel.SendMessage("Couldn't find that map.");
					});
			return Task.FromResult(0);
		}
	}
}