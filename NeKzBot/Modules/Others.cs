using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
using NeKzBot.Tasks;
using NeKzBot.Server;
using NeKzBot.Classes;
using NeKzBot.Resources;

namespace NeKzBot.Modules
{
	public class Others : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Other Commands", LogColor.Init);
			await GetRandomCheat(Data.cheatCmd);
			await GetRandomExploit(Data.exploitCmd);
			await GetRandomFact("funfact");
			await GetScriptAsync("scripts");
			await GetElevatorTiming("dialogue");
			await GetSegmentedRunAsync(Data.srunCmd);
			await GetServerInfo("rpi");
			await GetCredits(Data.creditCmd);
			await GetMapImage("view");
			await OtherCommands();
			await Utils.CommandCreator(() => Tools(Utils.CCIndex), 0, Data.ToolCommands);
			await Utils.CommandCreator(() => Memes(Utils.CCIndex), 0, Data.MemeCommands);
			await Utils.CommandCreator(() => Links(Utils.CCIndex), 0, Data.LinkCommands);
			await Utils.CommandCreator(() => Text(Utils.CCIndex), 0, Data.QuoteNames);
		}

		#region RANDOM
		public static Task GetRandomCheat(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c}` shows you a random console command.\n• You can use it in challenge mode.")
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
					.Description($"• `{Configuration.Default.PrefixCmd + c}` prints out a random exploit or glitch name.\n• You can use it for routing.")
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
					.Description($"• `{Configuration.Default.PrefixCmd + c}` gives you a random text about a random topic.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"*{await Utils.RNGAsync(Data.QuoteNames, 1) as string}*");
					});
			return Task.FromResult(0);
		}
		#endregion

		#region USEFUL RESOURCES
		public static async Task GetScriptAsync(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c} <name>` gives you a specific AutoHotkey script.\n• Available scripts: {await Utils.ArrayToList(Data.ScriptFiles, 0, "`")}")
					.Parameter("name", ParameterType.Required)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (await Utils.SearchArray(Data.ScriptFiles, 0, e.Args[0], out var index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/scripts/{Data.ScriptFiles[index, 1]}");
						else
							await e.Channel.SendMessage($"Unknown script. Try one of these:\n{await Utils.ArrayToList(Data.ScriptFiles, 0, "`")}");
					});
		}

		public static Task GetElevatorTiming(string c)
		{
			CService.CreateCommand(c)
					.Alias("elevator", "dialog", "timing")
					.Description($"• `{Configuration.Default.PrefixCmd + c} <mapname>` gives you a hint when to enter the elevator of a map.\n• You can type the map name, challenge mode name or the 3-letter map name code if you want.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (await Utils.SearchArray(Data.Portal2Maps, 2, e.Args[0], out var index))
							await e.Channel.SendMessage(Data.Portal2Maps[index, 4]);
						else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.Args[0], out index))
							await e.Channel.SendMessage(Data.Portal2Maps[index, 4]);
						else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.Args[0], out index))
							await e.Channel.SendMessage(Data.Portal2Maps[index, 4]);
						else
							await e.Channel.SendMessage($"Unknown map name. Try `{Configuration.Default.PrefixCmd + c}` with one of these:\n{await Utils.ArrayToList(Data.Portal2Maps, 5)}");
					});
			return Task.FromResult(0);
		}

		public async static Task GetSegmentedRunAsync(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c} <name>` shows you a completed (or in progress) segmented run.\n• `{Configuration.Default.PrefixCmd + c}` gets a random one.\n• Available projects: {await Utils.ArrayToList(Data.ProjectNames, 0, "`")}")
					.Parameter("name", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var rand = await Utils.RNGAsync(Data.ProjectNames.GetLength(0));
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage($"**{Data.ProjectNames[rand, 1]}**\n{Data.ProjectNames[rand, 2]}");
						else if (await Utils.SearchArray(Data.ProjectNames, 0, e.Args[0], out var index))
							await e.Channel.SendMessage($"**{Data.ProjectNames[index, 1]}**\n{Data.ProjectNames[index, 2]}");
						else if (await Utils.SearchArray(Data.ProjectNames, 1, e.Args[0], out index))
							await e.Channel.SendMessage($"**{Data.ProjectNames[index, 1]}**\n{Data.ProjectNames[index, 2]}");
						else
							await e.Channel.SendMessage($"Unknown run. Try of one these:\n{await Utils.ArrayToList(Data.ProjectNames, 0, "`")}");
					});
		}
		#endregion

		#region OTHERS
		public static Task OtherCommands()
		{
			// It all started here
			CService.CreateCommand("hello")
					.Alias("hi", "helloworld", "hey", "yo")
					.Description($"• `{Configuration.Default.PrefixCmd}hello` will greet you back.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{await Utils.RNGStringAsync(Data.botGreetings)} {await Utils.RNGStringAsync(Data.botFeelings)}");
					});

			// Convert text to symbols
			CService.CreateCommand("ris")
					.Description($"• `{Configuration.Default.PrefixCmd}ris <text>` returns your message in regional indicator symbols.")
					.Parameter("text", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync("ris"));
						else
							await e.Channel.SendMessage(await Utils.RISAsync(e.Args[0]));
					});

			// Memes
			CService.CreateCommand("meme")
					.Description($"• `{Configuration.Default.PrefixCmd}meme` hints you a meme command.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RNGAsync(Data.MemeCommands, 0) as string}`.");
					});

			CService.CreateCommand("routecredit")
					.Description($"• `{Configuration.Default.PrefixCmd}routecredit` gives somebody route credit for no reason.")
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
					.Description($"• `{Configuration.Default.PrefixCmd}question <question>` responses to a question.")
					.Parameter("question", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (string.IsNullOrEmpty(e.Args[0]))
							await e.Channel.SendMessage(await Utils.FindDescriptionAsync("question"));
						else if (e.Args[0][e.Args[0].Length - 1] == '?')
							await e.Channel.SendMessage(await Utils.RNGStringAsync(Data.botAnswers));
						else
							await e.Channel.SendMessage(await Utils.RNGStringAsync("Is this a question?", "This isn't a question.", "Please..."));
					});

			// Small user information
			CService.CreateCommand("when")
					.Description($"• `{Configuration.Default.PrefixCmd}when` shows you when you joined the server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{e.User.Name} joined this server on **{e.User.JoinedAt}**.");
					});

			CService.CreateCommand("idinfo")
					.Description($"• `{Configuration.Default.PrefixCmd}idinfo` returns some user id stats")
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
							output1 += $"• {item.Nickname}#{await Utils.FormatId(item.Discriminator)}\n";

						var output2 = string.Empty;
						foreach (var item in highestids)
							output2 += $"• {item.Nickname}#{await Utils.FormatId(item.Discriminator)}\n";

						await e.Channel.SendMessage($"{(lowestids.Count > 1 ? $"Lowest IDs\n{output1}" : $"Lowest ID • {lowestid.Name}#{await Utils.FormatId(lowestid.Discriminator)}\n")}"
												  + $"{(highestids.Count > 1 ? $"Highest IDs\n{output2}" : $"Highest ID • {highestid.Name}#{await Utils.FormatId(highestid.Discriminator)}\n")}"
												  + $"Average ID • #{await Utils.FormatId((ulong)Math.Round((decimal)sumids / users.Length, 0))}"
						);
					});

			// Create server invite link
			CService.CreateCommand("invite")
					.Description($"• `{Configuration.Default.PrefixCmd}invite` creates a new temporary invite link of this server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discord.gg/{(await e.Server.CreateInvite()).Code}");
					});

			CService.CreateCommand("staticinvite")
					.Description($"• `{Configuration.Default.PrefixCmd}staticinvite` returns the static invite link of the main server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discord.gg/{Credentials.Default.DiscordMainServerLinkId}");
					});

			// Get bot invite link
			CService.CreateCommand("join")
					.Description($"• `{Configuration.Default.PrefixCmd}join` returns the bot invite link.\n• Use this to invite the bot to your server.")
					.Do(async e =>
					{
						// Useful tool https://discordapi.com/permissions.html
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"https://discordapp.com/api/oauth2/authorize?client_id={Bot.Client.CurrentUser.Id}&scope=bot&permissions=271764481");
					});

			// Dropbox stuff
			CService.CreateCommand("cloud")
					.Alias("folder")
					.Description($"• `{Configuration.Default.PrefixCmd}cloud` returns the link for the public demo folder\n• Just attach your demo and it'll automatically upload it to Dropbox.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Credentials.Default.DropboxFolderQuery != string.Empty ? $"<https://www.dropbox.com/sh/{Credentials.Default.DropboxFolderQuery}?dl=0>" : "Not available.");
					});

			CService.CreateCommand("dbfolder")
					.Alias("myfolder")
					.Description($"• `{Configuration.Default.PrefixCmd}dbfolder` returns the list of files you've stored on Dropbox.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await DropboxCom.ListFilesAsync($"{Configuration.Default.DropboxFolderName}/{e.User.Id}"));
					});

			CService.CreateCommand("dbdelete")
					.Alias("myfolder")
					.Description($"• `{Configuration.Default.PrefixCmd}dbdelete <file>` deletes the file from your own Dropbox folder.• For master server-admin only `{Configuration.Default.PrefixCmd}dbdelete <folder> <file>`")
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
					.Description($"• `{Configuration.Default.PrefixCmd}stream <channel>` shows the preview of a streamer from Twitch.")
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
					.Description($"• `{Configuration.Default.PrefixCmd + c}` shows you a list of people who helped to develope me.")
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
					.Description($"• `{Configuration.Default.PrefixCmd + c}` returns a picture of a random Portal 2 map. Use `{Configuration.Default.PrefixCmd}{c} <mapname>` to show a specific image of a level.")
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
		#endregion

		#region MULTIPLE
		public static Task Memes(int i)
		{
			CService.CreateCommand(Data.MemeCommands[i, 0])
					.Description(Data.MemeCommands[i, 1])
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						// Text only
						if (Data.MemeCommands[i, 2] == string.Empty)
							await e.Channel.SendMessage(Data.MemeCommands[i, 3]);
						// File only
						else if (Data.MemeCommands[i, 3] == string.Empty)
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/{Data.MemeCommands[i, 2]}");
						// File and text
						else
						{
							await e.Channel.SendMessage($"**{Data.MemeCommands[i, 1]}**");
							await Task.Delay(333);
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/{Data.MemeCommands[i, 2]}");
						}
					});
			return Task.FromResult(0);
		}

		public static Task Tools(int i)
		{
			CService.CreateCommand(Data.ToolCommands[i, 0])
					.Description(Data.ToolCommands[i, 1])
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"**{Data.ToolCommands[i, 3]}**\n{Data.ToolCommands[i, 2]}");
					});
			return Task.FromResult(0);
		}

		public static Task Links(int i)
		{
			CService.CreateCommand(Data.LinkCommands[i, 0])
					.Description(Data.LinkCommands[i, 1])
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{Data.LinkCommands[i, 3]}\n{Data.LinkCommands[i, 2]}");
					});
			return Task.FromResult(0);
		}

		public static Task Text(int i)
		{
			CService.CreateGroup("quote", GBuilder =>
			{
				GBuilder.CreateCommand(Data.QuoteNames[i, 0])
						.Description("Could be true or not.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"*{Data.QuoteNames[i, 1]}*");
						});
			});
			return Task.FromResult(0);
		}
		#endregion

		#region SERVER INFORMATION
		private static Task GetServerInfo(string s)
		{
			CService.CreateGroup(s, GBuilder =>
			{
				GBuilder.CreateCommand("specs")
						.Description($"• `{Configuration.Default.PrefixCmd + s} specs shows some hardware information about the server.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(Data.serverSpecs);
						});

				GBuilder.CreateCommand("date")
						.Description($"• `{Configuration.Default.PrefixCmd + s} date shows time and date of the server.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Utils.GetCommandOutputAsync("date"));
						});

				GBuilder.CreateCommand("uptime")
						.Description($"• `{Configuration.Default.PrefixCmd + s} uptime shows how long the server is running for.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage((await Utils.GetCommandOutputAsync("uptime")).Split(',')[0]);
						});

				GBuilder.CreateCommand("temperature")
						.Alias("temp", "howhot?")
						.Description($"• `{Configuration.Default.PrefixCmd + s} temperature shows the current temperature of the gup the local time of the cpu.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var temp = await Utils.GetCommandOutputAsync("vcgencmd", "measure_temp");
							await e.Channel.SendMessage($"SoC Temperature = **{temp.Substring(5, temp.Length - 5).Replace("'", "°")}**");
						});

				GBuilder.CreateCommand("os")
						.Description($"• `{Configuration.Default.PrefixCmd + s} os gives you more information about the server's operating system.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var os = await Utils.GetCommandOutputAsync("cat", "/etc/os-release");
							var output = string.Empty;
							foreach (var item in os.Split('\n'))
								output += $"{await Utils.GetRestAfter(item, '=')}\n";
							await e.Channel.SendMessage(output.Substring(0, output.Length - 1).Replace("\"", string.Empty));
						});
			});
			return Task.FromResult(0);
		}
		#endregion
	}
}