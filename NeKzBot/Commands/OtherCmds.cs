using System.Linq;
using Discord.Commands;
using NeKzBot.Properties;

namespace NeKzBot
{
	public class OtherCmds : Commands
	{
		public static void Load()
		{
			Logging.CON("Loading other commands", System.ConsoleColor.DarkYellow);

			GetRandomCheat(Data.cheatCmd);          // !cheat
			GetRandomExploit(Data.exploitCmd);      // !exploit
			GetRandomFact("funfact");               // !funfact, !fact
			GetScript("script");                    // !script <name>
			GetElevatorTiming("dialogue");          // !dialogue
			GetSegmentedRun(Data.srunCmd);          // !segmented <name>
			GetServerInfo("rpi");                   // !rpi
			GetCredits(Data.creditCmd);				// !credits
			OtherCommands();

			Utils.CommandCreator(() => Tools(Utils.index), 0, Data.toolCommands);
			Utils.CommandCreator(() => Memes(Utils.index), 0, Data.memeCommands);
			Utils.CommandCreator(() => Links(Utils.index), 0, Data.linkCommands);
			Utils.CommandCreator(() => Text(Utils.index), 0, Data.quoteNames);
		}

		#region RANDOM
		public static void GetRandomCheat(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` shows you a random console command.\n**-** You can use it in challenge mode.")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(Data.consoleCommands[Utils.RNG(Data.consoleCommands.Count())]);
			});
		}

		public static void GetRandomExploit(string c)
		{
			cmd.CreateCommand(c)
			.Alias("glitch")
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` prints out a random exploit or glitch name.\n**-** You can use it for routing.")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int rand = Utils.RNG(Data.p2Exploits.GetLength(0));
				await e.Channel.SendMessage($"**{Data.p2Exploits[rand, 0]}**\n{Data.p2Exploits[rand, 1]}");
			});
		}

		public static void GetRandomFact(string c)
		{
			cmd.CreateCommand(c)
			.Alias("fact")
			.Description($"**-** `{Settings.Default.PrefixCmd + c} gives you a random text about a random topic.")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"*{Data.quoteNames[Utils.RNG(Data.quoteNames.GetLength(0)), 1]}*");
			});
		}
		#endregion

		#region USEFUL RESOURCES
		public static void GetScript(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <name>` gives you a specific AutoHotkey script.\n**-** Available scripts: {Utils.ArrayToList(Data.scriptFiles, 0, "`")}")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int index;
				if (Utils.SearchArray(Data.scriptFiles, 0, e.Args[0], out index))
					await e.Channel.SendFile($"{Settings.Default.ApplicationPath}Resources/scripts/{Data.scriptFiles[index, 1]}");
				else
					await e.Channel.SendMessage($"Unknown script. Try one of these:\n{Utils.ArrayToList(Data.scriptFiles, 0, "`")}");
			});
		}

		public static void GetElevatorTiming(string c)
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
		}

		public static void GetSegmentedRun(string c)
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
		}
		#endregion

		#region OTHERS
		public static void OtherCommands()
		{
			// It all started here
			cmd.CreateCommand("hello")
			.Description($"**-** `{Settings.Default.PrefixCmd}hello` will greet you back.")
			.Alias("hi", "helloworld", "hey", "yo")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage("Hi! :smile:");
			});

			// Convert text to symbols
			cmd.CreateCommand("ris")
			.Description($"**-** `{Settings.Default.PrefixCmd}ris <text>` returns your message in regional indicator symbols.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] == string.Empty)
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd}ris <text>`");
				else
					await e.Channel.SendMessage(Utils.RIS(e.Args[0]));
			});

			// Memes
			cmd.CreateCommand("routecredit")
			.Description($"**-** `{Settings.Default.PrefixCmd}routecredit` gives somebody route credit for no reason.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage("Route credit goes to...");
				await e.Channel.SendIsTyping();
				await System.Threading.Tasks.Task.Delay(5000);

				Discord.User rand;
				do
					rand = e.Server.Users.ToList()[Utils.RNG(e.Server.UserCount)];
				while (rand.IsBot);

				await e.Channel.SendMessage($"**{rand.Name}**");
			});

			cmd.CreateCommand("??")
			.Alias("?bot", "q", "?q")
			.Description($"**-** `{Settings.Default.PrefixCmd}?? <question>` responses to a question.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				var question = e.Args[0];
				if (string.IsNullOrEmpty(question))
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd}?? <question>`");
				else if (question[question.Length - 1] == '?')
					await e.Channel.SendMessage(Utils.RNGString("**Yes.**", "**No.**", "**Maybe.**", "**NO.**", "**YEEE!**", ":ok_hand:", ":thumbsup:", ":thumbsdown:"));
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

			// Get invite link
			cmd.CreateCommand("invite")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"https://discord.gg/{Settings.Default.ServerInviteID}");
			});

			// Dropbox stuff
			cmd.CreateCommand("cloud")
			.Description($"**-** `{Settings.Default.PrefixCmd}cloud` returns the link for the public demo folder\n**-** Just attach your demo and it'll automatically upload it to Dropbox.")
			.Alias("folder")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"https://www.dropbox.com/sh{Settings.Default.DropboxFolderLink}?dl=0");
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
				if (e.Args[0].Contains('|') && e.Channel.Id == Settings.Default.MaseterAdminID)
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
		}

		public static void GetCredits(string c)
		{
			// The credits
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd}credits` shows you a list of people who helped to develope me.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"**Special Thanks To**\n{Utils.ArrayToList(Data.specialThanks, string.Empty, "\n", "**-** ")}");
			});
		}
		#endregion

		#region MULTIPLE
		public static void Memes(int i)
		{
			cmd.CreateCommand(Data.memeCommands[i, 0])
			.Description(Data.memeCommands[i, 1])
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (Data.memeCommands[i, 2] == string.Empty)
					await e.Channel.SendMessage(Data.memeCommands[i, 3]); // Text only
				else if (Data.memeCommands[i, 3] == string.Empty)
					await e.Channel.SendFile($"{Settings.Default.ApplicationPath}Resources/pics/{Data.memeCommands[i, 2]}"); // File only
				else
				{
					await e.Channel.SendMessage($"**{Data.memeCommands[i, 3]}**"); // File and text
					await System.Threading.Tasks.Task.Delay(333);
					await e.Channel.SendFile($"{Settings.Default.ApplicationPath}Resources/pics/{Data.memeCommands[i, 2]}");
				}
			});
		}

		public static void Tools(int i)
		{
			cmd.CreateCommand(Data.toolCommands[i, 0])
			.Description(Data.toolCommands[i, 1])
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"**{Data.toolCommands[i, 3]}**\n{Data.toolCommands[i, 2]}");
			});
		}

		public static void Links(int i)
		{
			cmd.CreateCommand(Data.linkCommands[i, 0])
			.Description(Data.linkCommands[i, 1])
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage($"{Data.linkCommands[i, 3]}\n{Data.linkCommands[i, 2]}");
			});
		}

		public static void Text(int i)
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
		}
		#endregion

		#region SERVER INFORMATION
		private static void GetServerInfo(string s)
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
					await e.Channel.SendMessage(Utils.GetCommandOutput("date"));
				});

				g.CreateCommand("uptime")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} uptime shows how long the server is running for.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(Utils.GetCommandOutput("uptime").Split(',')[0]);
				});

				g.CreateCommand("temperature")
				.Alias("temp", "howhot?")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} temperature shows the current temperature of the gup the local time of the cpu.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					var t = Utils.GetCommandOutput("vcgencmd", "measure_temp");
					await e.Channel.SendMessage($"SoC Temperature = **{t.Substring(5, t.Length - 5).Replace("'", "°")}**");
				});

				g.CreateCommand("os")
				.Description($"**-** `{Settings.Default.PrefixCmd + s} os gives you more information about the server's operating system.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					var os = Utils.GetCommandOutput("cat", "/etc/os-release");
					var temp = os.Split('\n');
					os = string.Empty;
					foreach (var item in temp)
						os += Utils.GetRestAfter(item, '=') + "\n";
					await e.Channel.SendMessage(os.Substring(0, os.Length - 1).Replace("\"", string.Empty));
				});
			});
		}
		#endregion
	}
}