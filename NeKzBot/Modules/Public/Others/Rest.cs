using System;
using System.Collections.Generic;
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
			// It all started here
			CService.CreateCommand("hello")
					.Alias("hi", "helloworld", "hey", "yo")
					.Description("Will greet you back.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{await Utils.RNGStringAsync(Data.BotGreetings)} {await Utils.RNGStringAsync(Data.BotFeelings)}");
					});

			// Convert text to symbols
			CService.CreateCommand("ris")
					.Description("Returns your message in regional indicator symbols.")
					.Parameter("text", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
							await e.Channel.SendMessage(await Utils.RISAsync(e.Args[0]));
						else
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
					});

			// Memes
			CService.CreateCommand("meme")
					.Description("Hints you a meme command.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RNGAsync(Data.MemeCommands, 0) as string}`.");
					});

			CService.CreateCommand("routecredit")
					.Description("Gives somebody route credit for no reason.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage("Route credit goes to...");
						await e.Channel.SendIsTyping();
						await Task.Delay(5000);

						var rand = default(Discord.User);
						do
							rand = e.Server.Users.ElementAt(await Utils.RNGAsync(e.Server.UserCount));
						while (rand.IsBot);

						await e.Channel.SendMessage($"**{rand.Name}**");
					});

			CService.CreateCommand("question")
					.Alias("q", "??")
					.Description("Responses to a question.")
					.Parameter("question", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (string.IsNullOrEmpty(e.Args[0]))
							await e.Channel.SendMessage(await Utils.GetDescriptionAsync(e.Command));
						else if (e.Args[0][e.Args[0].Length - 1] == '?')
							await e.Channel.SendMessage(await Utils.RNGStringAsync(Data.BotAnswers));
						else
							await e.Channel.SendMessage(await Utils.RNGStringAsync("Is this a question?", "This isn't a question.", "Please..."));
					});

			// Small user information
			CService.CreateCommand("when")
					.Description("Shows you when you joined the server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{e.User.Name} joined this server on **{e.User.JoinedAt}**.");
					});

			CService.CreateCommand("idinfo")
					.Description("Returns some user id stats")
					.Do(async e =>
					{
						var users = e.Server.Users.ToArray();
						var lowestid = users[0];
						var highestid = users[0];
						var lowestids = new List<Discord.User>();
						var highestids = new List<Discord.User>();
						var sumids = 0;
						foreach (var item in users)
						{
							// Sometimes the id is zero? What???
							if (item.Id == 0)
								continue;

							// Search for lowest id
							if (lowestid.Id > item.Id)
							{
								lowestid = item;
								lowestids = new List<Discord.User> { item };
							}
							else if (lowestid.Id == item.Id)
								lowestids.Add(item);

							// Search for highest id
							if (highestid.Id < item.Id)
							{
								highestid = item;
								highestids = new List<Discord.User> { item };
							}
							else if (highestid.Id == item.Id)
								highestids.Add(item);

							sumids += item.Discriminator;
						}

						var output1 = string.Empty;
						foreach (var item in lowestids)
							output1 += $"• {item.Nickname}#{item.Discriminator.ToString("D4")}\n";

						var output2 = string.Empty;
						foreach (var item in highestids)
							output2 += $"• {item.Nickname}#{item.Discriminator.ToString("D4")}\n";

						await e.Channel.SendMessage($"{(lowestids.Count > 1 ? $"Lowest IDs\n{output1}" : $"Lowest ID • {lowestid.Name}#{lowestid.Discriminator.ToString("D4")}\n")}"
												  + $"{(highestids.Count > 1 ? $"Highest IDs\n{output2}" : $"Highest ID • {highestid.Name}#{highestid.Discriminator.ToString("D4")}\n")}"
												  + $"Average ID • #{((ulong)Math.Round((decimal)sumids / users.Length, 0)).ToString("D4")}"
						);
					});

			// Create server invite link
			CService.CreateCommand("invite")
					.Description("Creates a new temporary invite link of this server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discord.gg/{(await e.Server.CreateInvite()).Code}");
					});

			CService.CreateCommand("staticinvite")
					.Description("Returns the static invite link of the main server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discord.gg/{Credentials.Default.DiscordMainServerLinkId}");
					});

			// Get bot invite link
			CService.CreateCommand("join")
					.Description("Returns the bot invite link. Use this to invite the bot to your server.")
					.Do(async e =>
					{
						// Useful tool https://discordapi.com/permissions.html
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discordapp.com/api/oauth2/authorize?client_id={Bot.Client.CurrentUser.Id}&scope=bot&permissions={Configuration.Default.BotPermissions}");
					});

			// Dropbox stuff
			CService.CreateCommand("cloud")
					.Alias("folder")
					.Description("Returns the link for the public demo folder. Just attach your demo and it'll be automatically uploaded to Dropbox.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Credentials.Default.DropboxFolderQuery != string.Empty ? $"<https://www.dropbox.com/sh/{Credentials.Default.DropboxFolderQuery}?dl=0>" : "Not available.");
					});

			CService.CreateCommand("dbfolder")
					.Alias("myfolder")
					.Description("Returns the list of files you've stored on Dropbox.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await DropboxCom.ListFilesAsync($"{Configuration.Default.DropboxFolderName}/{e.User.Id}"));
					});

			CService.CreateCommand("dbdelete")
					.Alias("myfolder")
					.Description($"Deletes the file from your own Dropbox folder. For master server-admin only try `{Configuration.Default.PrefixCmd}dbdelete <folder> <file>` to delete files from other users. The folder name is the id of a user.")
					.Parameter("file", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if ((e.Args[0].Contains('|'))
						&& (e.User.Id == Credentials.Default.DiscordBotOwnerId))
						{
							var values = e.Args[0].Split('|');
							if (values.Length == 2)
								await e.Channel.SendMessage(await DropboxCom.DeleteFileAsync($"{Configuration.Default.DropboxFolderName}/{values[0]}", values[1]));
							else
								await e.Channel.SendMessage("Invalid parameters.");
						}
						else
							await e.Channel.SendMessage(await DropboxCom.DeleteFileAsync($"{Configuration.Default.DropboxFolderName}/{e.User.Id}", e.Args[0]));
					});

			// Twitch
			CService.CreateCommand("stream")
					.Description("Shows the preview of a streamer from Twitch.")
					.Alias("preview")
					.Parameter("channel", ParameterType.Required)
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
							var path = await Utils.GetPath() + $"/Resources/Cache/{e.Args[0]}-twitch.jpg";
							await Fetching.GetFileAsync(preview, path);
							await e.Channel.SendFile(path);
						}
					});
			return Task.FromResult(0);
		}

		public static Task GetCredits(string c)
		{
			// The credits
			CService.CreateCommand(c)
					.Description("Shows you a list of people who helped to develope me.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"**Special Thanks To**\n{await Utils.ArrayToList(Data.SpecialThanks, string.Empty, "\n", "• ")}");
					});
			return Task.FromResult(0);
		}

		public static Task GetMapImage(string c)
		{
			CService.CreateCommand(c)
					.Alias("image", "overview")
					.Description($"Returns a picture of a random Portal 2 map. Try `{Configuration.Default.PrefixCmd + c} <mapname>` to show a specific image of a level.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] == string.Empty)
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/maps/{await Utils.RNGAsync(Data.Portal2Maps, 0) as string}.jpg");
						else if (await Utils.SearchArray(Data.Portal2Maps, 2, e.Args[0], out var index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/RResources/Private/pics/maps/{Data.Portal2Maps[index, 0]}.jpg");
						else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.Args[0], out index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/RResources/Private/pics/maps/{Data.Portal2Maps[index, 0]}.jpg");
						else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.Args[0], out index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/RResources/Private/pics/maps/{Data.Portal2Maps[index, 0]}.jpg");
						else
							await e.Channel.SendMessage($"Couldn't find that map. Try `{Configuration.Default.PrefixCmd + c}` with one of these:\n{await Utils.ArrayToList(Data.Portal2Maps, 5)}");
					});
			return Task.FromResult(0);
		}
	}
}