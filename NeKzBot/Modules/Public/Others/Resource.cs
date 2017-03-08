using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;
using System.Collections.Generic;

namespace NeKzBot.Modules.Public.Others
{
	public class Resource : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Resource Module", LogColor.Init);
			await GetScriptAsync("scripts");
			await GetElevatorTiming("dialogue");
			await GetPortalCvar("cvar");
			await GetSegmentedRun(Data.SegmentedRunCommand);
		}

		public static async Task GetScriptAsync(string c)
		{
			CService.CreateCommand(c)
					.Alias("script")
					.Description($"Gives you a specific AutoHotkey script. Available scripts: {await Utils.ArrayToList(Data.ScriptFiles, 0, "`")}")
					.Parameter("name", ParameterType.Unparsed)
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] != string.Empty)
						{
							if (await Utils.SearchArray(Data.ScriptFiles, 0, e.Args[0], out var index))
								await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/scripts/{Data.ScriptFiles[index, 1]}");
							else
								await e.Channel.SendMessage($"Unknown script. Try one of these: {await Utils.ArrayToList(Data.ScriptFiles, 0, "`")}");
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
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
						if (e.Args[0] != string.Empty)
						{
							if (await Utils.SearchArray(Data.Portal2Maps, 2, e.Args[0], out var index))
								await e.Channel.SendMessage(Data.Portal2Maps[index, 4]);
							else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.Args[0], out index))
								await e.Channel.SendMessage(Data.Portal2Maps[index, 4]);
							else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.Args[0], out index))
								await e.Channel.SendMessage(Data.Portal2Maps[index, 4]);
							else
								await e.Channel.SendMessage("Unknown map name.");
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
						var rand = await Utils.RngAsync(Data.ProjectNames.GetLength(0));
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage($"**{Data.ProjectNames[rand, 1]}**\n{Data.ProjectNames[rand, 2]}");
						else if (await Utils.SearchArray(Data.ProjectNames, 0, e.Args[0], out var index))
							await e.Channel.SendMessage($"**{Data.ProjectNames[index, 1]}**\n{Data.ProjectNames[index, 2]}");
						else if (await Utils.SearchArray(Data.ProjectNames, 1, e.Args[0], out index))
							await e.Channel.SendMessage($"**{Data.ProjectNames[index, 1]}**\n{Data.ProjectNames[index, 2]}");
						else
							await e.Channel.SendMessage($"Unknown project. Try of one these: {await Utils.ArrayToList(Data.ProjectNames, 0, "`")}");
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
						var rand = await Utils.RngAsync(Data.Portal2Cvars.GetLength(0));
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage($"**{Data.Portal2Cvars[rand, 0]}**{(Data.Portal2Cvars[rand, 3] != string.Empty ? $"\n{Data.Portal2Cvars[rand, 3]}" : string.Empty)}{(Data.Portal2Cvars[rand, 1] != string.Empty ? $"\n• Default Value: {Data.Portal2Cvars[rand, 1]}" : string.Empty)}{(Data.Portal2Cvars[rand, 2] != string.Empty ? $"\n• Flags: {Data.Portal2Cvars[rand, 2]}" : string.Empty)}");	// Sry
						else if (await Utils.SearchArray(Data.Portal2Cvars, 0, e.Args[0], out var index))
							await e.Channel.SendMessage($"**{Data.Portal2Cvars[index, 0]}**{(Data.Portal2Cvars[index, 3] != string.Empty ? $"\n{Data.Portal2Cvars[index, 3]}" : string.Empty)}{(Data.Portal2Cvars[index, 1] != string.Empty ? $"\n• Default Value: {Data.Portal2Cvars[index, 1]}" : string.Empty)}{(Data.Portal2Cvars[index, 2] != string.Empty ? $"\n• Flags: {Data.Portal2Cvars[index, 2]}" : string.Empty)}");
						else
							await e.Channel.SendMessage("Unknown console variable.");
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
						for (int i = 0; i < Data.Portal2Cvars.GetLength(0); i++)
							temp.Add(Data.Portal2Cvars[i, 0]);

						var cvars = temp.Where(cvar => cvar.Length >= e.Args[0].Length);
						if (temp?.Count() < 1)
							return;
						cvars = cvars.Where(cvar => cvar.Substring(0, e.Args[0].Length) == e.Args[0]);
						if (temp?.Count() < 1)
							return;
						await (await e.User.CreatePMChannel())?.SendMessage(await Utils.CutMessage(await Utils.ListToList(cvars.ToList())));
					});
			return Task.FromResult(0);
		}
	}
}