using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;
using System.Collections.Generic;
using NeKzBot.Internals;
using NeKzBot.Classes;

namespace NeKzBot.Modules.Public.Others
{
	public class Resource : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Resource Module", LogColor.Init);
			await GetScript("scripts");
			await GetElevatorTiming("dialogue");
			await GetPortalCvar("cvar");
			await GetSegmentedRun(Data.SegmentedRunCommand);
		}

		public static Task GetScript(string c)
		{
			CService.CreateCommand(c)
					.Alias("script")
					.Description("Gives you a specific AutoHotkey script.")
					.Parameter("name", ParameterType.Unparsed)
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.GetArg("name"))))
						{
							var scripts = await Data.Get<Complex>("scripts");
							var script = scripts.Get(e.GetArg("name"));
							if (script != null)
								await e.Channel.SendFile($"{await Utils.GetAppPath()}/Resources/Private/scripts/{script.Value[1]}");
							else
								await e.Channel.SendMessage($"Unknown script. Try one of these: {await Utils.CollectionToList(scripts.Cast(0), "`")}");
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		public static Task GetElevatorTiming(string c)
		{
			CService.CreateCommand(c)
					.Alias("elevator", "dialog", "timing")
					.Description("Gives you a hint when to enter the elevator of a map. You can enter the map name, challenge mode name or the 3-letter map name code if you want.")
					.Parameter("mapname", ParameterType.Unparsed)
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.GetArg("mapname") != string.Empty)
						{
							var result = await (await Data.Get<Portal2Maps>("p2maps")).Search(e.GetArg("mapname"));
							if (result == null)
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}
							await e.Channel.SendMessage(result.ElevatorTiming);
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		public static Task GetSegmentedRun(string c)
		{
			CService.CreateCommand(c)
					.Description("Shows you a completed (or in progress) segmented run. Try the command without a parameter to get a random run.")
					.Parameter("project", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var projects = await Data.Get<Complex>("projects");
						var rand = projects.Values[await Utils.RngAsync(projects.Values.Count)].Value;
						if (string.IsNullOrEmpty(e.GetArg("project")))
							await e.Channel.SendMessage($"**{rand[1]}**\n{rand[2]}");
						else
						{
							var result = default(Simple);
							if ((result = projects.Get(e.GetArg("project"))) == null)
								await e.Channel.SendMessage($"Unknown project. Try of one these: {await Utils.CollectionToList(projects.Cast(), "`")}");
							else
								await e.Channel.SendMessage($"**{result.Value[1]}**\n{result.Value[2]}");
						}
					});
			return Task.FromResult(0);
		}

		public static Task GetPortalCvar(string c)
		{
			CService.CreateCommand(c)
					.Alias("cvars")
					.Description("Returns the description of a Portal 2 console variable.")
					.Parameter("cvar", ParameterType.Unparsed)
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var cvars = await Data.Get<Complex>("p2cvars");
						var rand = cvars.Values[await Utils.RngAsync(cvars.Values.Count)].Value;
						if (string.IsNullOrEmpty(e.GetArg("cvar")))
							await e.Channel.SendMessage($"**{rand[0]}**{(rand[3] != string.Empty ? $"\n{rand[3]}" : string.Empty)}{(rand[1] != string.Empty ? $"\n• Default Value: {rand[1]}" : string.Empty)}{(rand[2] != string.Empty ? $"\n• Flags: {rand[2]}" : string.Empty)}");	// Sry
						else
						{
							var result = default(Simple);
							if ((result = cvars.Get(e.GetArg("cvar"))) == null)
								await e.Channel.SendMessage("Unknown console variable.");
							else
								await e.Channel.SendMessage($"**{result.Value[0]}**{(result.Value[3] != string.Empty ? $"\n{result.Value[3]}" : string.Empty)}{(result.Value[1] != string.Empty ? $"\n• Default Value: {result.Value[1]}" : string.Empty)}{(result.Value[2] != string.Empty ? $"\n• Flags: {result.Value[2]}" : string.Empty)}");
						}
					});

			CService.CreateCommand("find")
					.Parameter("cvar", ParameterType.Unparsed)
					.AddCheck(Permissions.VipGuildsOnly)
					.Hide()
					.Do(async e =>
					{
						if (e.Args[0] == string.Empty)
							return;

						var temp = new List<string>();
						var cvars = (await Data.Get<Complex>("p2cvars")).Values;
						for (int i = 0; i < cvars.Count; i++)
							temp.Add(cvars[i].Value[0]);

						var output = temp.Where(cvar => cvar.Length >= e.Args[0].Length);
						if (temp?.Count() < 1)
							return;
						output = output.Where(cvar => cvar.Substring(0, e.Args[0].Length) == e.Args[0]);
						if (temp?.Count() < 1)
							return;
						await (await e.User.CreatePMChannel())?.SendMessage(await Utils.CutMessageAsync(await Utils.CollectionToList(output.ToList())));
					});
			return Task.FromResult(0);
		}
	}
}