using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Server;
using NeKzBot.Resources;
using NeKzBot.Modules.Message;

namespace NeKzBot.Tasks
{
	public class OtherCmds : Commands
	{
		public static async Task Load()
		{
			await Logging.CON("Loading other commands", System.ConsoleColor.DarkYellow);
			await GetRandomCheat(Data.cheatCmd);
			await GetRandomExploit(Data.exploitCmd);
			await GetRandomFact("funfact");
			await GetScript("script");
			await GetElevatorTiming("dialogue");
			await GetSegmentedRun(Data.srunCmd);
			await GetServerInfo("rpi");
			await GetCredits(Data.creditCmd);
			await GetMapImage("view");
			await OtherCommands();
			await Utils.CommandCreator(() => Tools(Utils.index), 0, Data.toolCommands);
			await Utils.CommandCreator(() => Memes(Utils.index), 0, Data.memeCommands);
			await Utils.CommandCreator(() => Links(Utils.index), 0, Data.linkCommands);
			await Utils.CommandCreator(() => Text(Utils.index), 0, Data.quoteNames);
		}

		#region RANDOM
		public static Task GetRandomCheat(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` shows you a random console command.\n**-** You can use it in challenge mode.")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(Data.consoleCommands[Utils.RNG(Data.consoleCommands.Count())]);
			});
			return Task.FromResult(0);
		}

		public static Task GetRandomExploit(string c)
		{
			cmd.CreateCommand(c)
			.Alias("glitch")
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` prints out a random exploit or glitch name.\n**-** You can use it for routing.")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				var rand = Utils.RNG(Data.p2Exploits.GetLength(0));
				await e.Channel.SendMessage($"**{Data.p2Exploits[rand, 0]}**\n{Data.p2Exploits[rand, 1]}");
			});
			return Task.FromResult(0);
		}

		public static Task GetRandomFact(string c)
		{
			cmd.CreateCommand(c)
			.Alias("fact")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} gives you a random text about a random topic.")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"*{Data.quoteNames[Utils.RNG(Data.quoteNames.GetLength(0)), 1]}*");
			});
			return Task.FromResult(0);
		}
		#endregion

		#region USEFUL RESOURCES
		public static Task GetScript(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <name>` gives you a specific AutoHotkey script.\n**-** Available scripts: {Utils.ArrayToList(Data.scriptFiles, 0, "`")}")
			.Parameter("p", ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int index;
				if (Utils.SearchArray(Data.scriptFiles, 0, e.Args[0], out index))
					await e.Channel.SendFile($"{Utils.GetPath()}Resources/scripts/{Data.scriptFiles[index, 1]}");
				else
					await e.Channel.SendMessage($"Unknown script. Try one of these:\n{Utils.ArrayToList(Data.scriptFiles, 0, "`")}");
			});
			return Task.FromResult(0);
		}

		public static Task GetElevatorTiming(string c)
		{
			cmd.CreateCommand(c)
			.Alias("elevator")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <mapname>` gives you a hint when to enter the elevator of a map.\n**-** You can type the map name, challenge mode name or the 3-letter map name code if you want.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int index;
				if (Utils.SearchArray(Data.portal2Maps, 2, e.Args[0], out index))
					await e.Channel.SendMessage(Data.portal2Maps[index, 4]);
				else if (Utils.SearchArray(Data.portal2Maps, 3, e.Args[0], out index))
					await e.Channel.SendMessage(Data.portal2Maps[index, 4]);
				else if (Utils.SearchArray(Data.portal2Maps, 5, e.Args[0], out index))
					await e.Channel.SendMessage(Data.portal2Maps[index, 4]);
				else
					await e.Channel.SendMessage($"Unknown map name. Try `{Settings.Default.PrefixCmd + c}` with one of these:\n{Utils.ArrayToList(Data.portal2Maps, 5)}");
			});
			return Task.FromResult(0);
		}

		public static Task GetSegmentedRun(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <name>` shows you a completed (or in progress) segmented run.\n**-** `{Settings.Default.PrefixCmd + c}` gets a random one.\n**-** Available projects: {Utils.ArrayToList(Data.projectNames, 0, "`")}")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int index, rand = Utils.RNG(Data.projectNames.GetLength(0));
				if (e.Args[0] == string.Empty)
					await e.Channel.SendMessage($"**{Data.projectNames[rand, 1]}**\n{Data.projectNames[rand, 2]}");
				else if (Utils.SearchArray(Data.projectNames, 0, e.Args[0], out index))
					await e.Channel.SendMessage($"**{Data.projectNames[index, 1]}**\n{Data.projectNames[index, 2]}");
				else if (Utils.SearchArray(Data.projectNames, 1, e.Args[0], out index))
					await e.Channel.SendMessage($"**{Data.projectNames[index, 1]}**\n{Data.projectNames[index, 2]}");
				else
					await e.Channel.SendMessage($"Unknown run. Try of one these:\n{Utils.ArrayToList(Data.projectNames, 0, "`")}");
			});
			return Task.FromResult(0);
		}
		#endregion

		#region OTHERS
		public static Task OtherCommands()
		{
			// It all started here
			cmd.CreateCommand("hello")
			.Description($"**-** `{Settings.Default.PrefixCmd}hello` will greet you back.")
			.Alias("hi", "helloworld", "hey", "yo")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"{Utils.RNGString(Data.botGreetings)} {Utils.RNGString(Data.botFeelings)}");
			});

			// Convert text to symbols
			cmd.CreateCommand("ris")
			.Description($"**-** `{Settings.Default.PrefixCmd}ris <text>` returns your message in regional indicator symbols.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] == string.Empty)
					await e.Channel.SendMessage(await Utils.FindDescription("ris"));
				else
					await e.Channel.SendMessage(Utils.RIS(e.Args[0]));
			});

			// Memes
			cmd.CreateCommand("meme")
			.Description($"**-** `{Settings.Default.PrefixCmd}meme` hints you a meme command.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"Try `{Settings.Default.PrefixCmd}{Data.memeCommands[Utils.RNG(Data.memeCommands.GetLength(0)), 0]}`.");
			});

			cmd.CreateCommand("routecredit")
			.Description($"**-** `{Settings.Default.PrefixCmd}routecredit` gives somebody route credit for no reason.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage("Route credit goes to...");
				await e.Channel.SendIsTyping();
				await Task.Delay(5000);

				Discord.User rand;
				do
					rand = e.Server.Users.ToList()[Utils.RNG(e.Server.UserCount)];
				while (rand.IsBot);

				await e.Channel.SendMessage($"**{rand.Name}**");
			});

			cmd.CreateCommand("question")
			.Alias("q", "??")
			.Description($"**-** `{Settings.Default.PrefixCmd}question <question>` responses to a question.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (string.IsNullOrEmpty(e.Args[0]))
					await e.Channel.SendMessage(await Utils.FindDescription("question"));
				else if (e.Args[0][e.Args[0].Length - 1] == '?')
					await e.Channel.SendMessage(Utils.RNGString(Data.botAnswers));
				else
					await e.Channel.SendMessage(Utils.RNGString("Is this a question?", "This isn't a question.", "Please..."));
			});
			
			// Small user information
			cmd.CreateCommand("when")
			.Description($"**-** `{Settings.Default.PrefixCmd}when` shows you when you joined the server.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"{e.User.Name} joined this server on **{e.User.JoinedAt.ToString()}**.");
			});

			cmd.CreateCommand("idinfo")
			.Description($"**-** `{Settings.Default.PrefixCmd}idinfo` returns some user ID stats")
			.Do(async e =>
			{
				var users = e.Server.Users.ToArray();
				var lowestid = users[0];
				var highestid = users[0];
				foreach (var item in users)
				{
					lowestid = lowestid.Discriminator > item.Discriminator ? item : lowestid;
					highestid = highestid.Discriminator < item.Discriminator ? item : highestid;
				}
				await e.Channel.SendMessage($"Lowest ID **-** {lowestid.Name}#{lowestid.Discriminator}\nHighest ID **-** {highestid.Name}#{highestid.Discriminator}");
			});

			// Get server invite link
			cmd.CreateCommand("invite")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"https://discord.gg/{Credentials.Default.DiscordMainServerLinkID}");
			});

			// Get invite link of bot
			cmd.CreateCommand("invitebot")
			.Alias("botinvite")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"https://discordapp.com/api/oauth2/authorize?client_id={Bot.dClient.CurrentUser.Id}&scope=bot&permissions=0");
			});

			// Dropbox stuff
			cmd.CreateCommand("cloud")
			.Description($"**-** `{Settings.Default.PrefixCmd}cloud` returns the link for the public demo folder\n**-** Just attach your demo and it'll automatically upload it to Dropbox.")
			.Alias("folder")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"https://www.dropbox.com/sh/{Credentials.Default.DropboxFolderID}?dl=0");
			});

			cmd.CreateCommand("dbfolder")
			.Description($"**-** `{Settings.Default.PrefixCmd}dbfolder` returns the list of files you've stored on Dropbox.")
			.Alias("myfolder")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(await DropboxCom.ListFiles($"{Settings.Default.DropboxFolderName}/{e.User.Id.ToString()}"));
			});

			cmd.CreateCommand("dbdelete")
			.Description($"**-** `{Settings.Default.PrefixCmd}dbdelete <file>` deletes the file from your own Dropbox folder.**-** For master server-admin only `{Settings.Default.PrefixCmd}dbdelete <folder> <file>`")
			.Alias("myfolder")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0].Contains('|') && e.User.Id == Credentials.Default.DiscordBotOwnerID)
				{
					var values = e.Args[0].Split('|');
					if (values.Count() == 2)
						await e.Channel.SendMessage(await DropboxCom.DeleteFile($"{Settings.Default.DropboxFolderName}/{values[0]}", values[1]));
					else
						await e.Channel.SendMessage("Invalid parameters.");
				}
				else
					await e.Channel.SendMessage(await DropboxCom.DeleteFile($"{Settings.Default.DropboxFolderName}/{e.User.Id.ToString()}", e.Args[0]));
			});

			// Twitch
			cmd.CreateCommand("stream")
			.Description($"**-** `{Settings.Default.PrefixCmd}stream <channel>` shows the preview of a streamer from Twitch.")
			.Alias("preview")
			.Parameter("p", ParameterType.Required)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(await Modules.Twitch.TwitchTv.GetPreview(e.Args[0]));
			});
			return Task.FromResult(0);
		}

		public static Task GetCredits(string c)
		{
			// The credits
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` shows you a list of people who helped to develope me.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"**Special Thanks To**\n{Utils.ArrayToList(Data.specialThanks, string.Empty, "\n", "**-** ")}");
			});
			return Task.FromResult(0);
		}

		public static Task GetMapImage(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` returns a picture of a random Portal 2 map. Use `{Settings.Default.PrefixCmd}{c} <mapname>` to show a specific image of a level.")
			.Alias("image", "overview")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				int index;
				if (e.Args[0] == string.Empty)
					await e.Channel.SendFile($"Resources/pics/maps/{Data.portal2Maps[Utils.RNG(Data.portal2Maps.GetLength(0)), 0]}.jpg");
				else if (Utils.SearchArray(Data.portal2Maps, 2, e.Args[0], out index))
					await e.Channel.SendFile($"Resources/pics/maps/{Data.portal2Maps[index, 0]}.jpg");
				else if (Utils.SearchArray(Data.portal2Maps, 3, e.Args[0], out index))
					await e.Channel.SendFile($"Resources/pics/maps/{Data.portal2Maps[index, 0]}.jpg");
				else if (Utils.SearchArray(Data.portal2Maps, 5, e.Args[0], out index))
					await e.Channel.SendFile($"Resources/pics/maps/{Data.portal2Maps[index, 0]}.jpg");
				else
					await e.Channel.SendMessage($"Couldn't find that map. Try `{Settings.Default.PrefixCmd + c}` with one of these:\n{Utils.ArrayToList(Data.portal2Maps, 5)}");
			});
			return Task.FromResult(0);
		}
		#endregion

		#region MULTIPLE
		public static Task Memes(int i)
		{
			cmd.CreateCommand(Data.memeCommands[i, 0])
			.Description(Data.memeCommands[i, 1])
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				// Text only
				if (Data.memeCommands[i, 2] == string.Empty)
					await e.Channel.SendMessage(Data.memeCommands[i, 3]);
				// File only
				else if (Data.memeCommands[i, 3] == string.Empty)
					await e.Channel.SendFile($"{Utils.GetPath()}Resources/pics/{Data.memeCommands[i, 2]}");
				// File and text
				else
				{
					await e.Channel.SendMessage($"**{Data.memeCommands[i, 3]}**");
					await Task.Delay(333);
					await e.Channel.SendFile($"{Utils.GetPath()}Resources/pics/{Data.memeCommands[i, 2]}");
				}
			});
			return Task.FromResult(0);
		}

		public static Task Tools(int i)
		{
			cmd.CreateCommand(Data.toolCommands[i, 0])
			.Description(Data.toolCommands[i, 1])
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"**{Data.toolCommands[i, 3]}**\n{Data.toolCommands[i, 2]}");
			});
			return Task.FromResult(0);
		}

		public static Task Links(int i)
		{
			cmd.CreateCommand(Data.linkCommands[i, 0])
			.Description(Data.linkCommands[i, 1])
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"{Data.linkCommands[i, 3]}\n{Data.linkCommands[i, 2]}");
			});
			return Task.FromResult(0);
		}

		public static Task Text(int i)
		{
			cmd.CreateGroup("quote", g =>
			{
				g.CreateCommand(Data.quoteNames[i, 0])
				.Description("Could be true or not.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage($"*{Data.quoteNames[i, 1]}*");
				});
			});
			return Task.FromResult(0);
		}
		#endregion
		
		#region SERVER INFORMATION
		private static Task GetServerInfo(string s)
		{
			cmd.CreateGroup(s, g =>
			{
				g.CreateCommand("specs")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} specs shows some hardware information about the server.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(Data.serverSpecs);
				});

				g.CreateCommand("date")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} date shows time and date of the server.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(await Utils.GetCommandOutput("date"));
				});

				g.CreateCommand("uptime")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} uptime shows how long the server is running for.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage((await Utils.GetCommandOutput("uptime")).Split(',')[0]);
				});

				g.CreateCommand("temperature")
				.Alias("temp", "howhot?")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} temperature shows the current temperature of the gup the local time of the cpu.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					var t = await Utils.GetCommandOutput("vcgencmd", "measure_temp");
					await e.Channel.SendMessage($"SoC Temperature = **{t.Substring(5, t.Length - 5).Replace("'", "°")}**");
				});

				g.CreateCommand("os")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} os gives you more information about the server's operating system.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					var os = await Utils.GetCommandOutput("cat", "/etc/os-release");
					var temp = os.Split('\n');
					os = string.Empty;
					foreach (var item in temp)
						os += Utils.GetRestAfter(item, '=') + "\n";
					await e.Channel.SendMessage(os.Substring(0, os.Length - 1).Replace("\"", string.Empty));
				});
			});
			return Task.FromResult(0);
		}
		#endregion
	}
}