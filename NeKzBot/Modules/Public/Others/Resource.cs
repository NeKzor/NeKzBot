using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public.Others
{
	public class Resource : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Resource Module", LogColor.Init);
			await GetScript("scripts");
			await GetElevatorTiming("dialogue");
			await GetSegmentedRun(Data.SegmentedRunCommand);
		}

		public static Task GetScript(string c)
		{
			CService.CreateCommand(c)
					.Description("Gives you a specific AutoHotkey script.")
					.Parameter("name", ParameterType.Required)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (await Utils.SearchArray(Data.ScriptFiles, 0, e.Args[0], out var index))
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/scripts/{Data.ScriptFiles[index, 1]}");
						else
							await e.Channel.SendMessage($"Unknown script. Try one of these:\n{await Utils.ArrayToList(Data.ScriptFiles, 0, "`")}");
					});
			return Task.FromResult(0);
		}

		public static Task GetElevatorTiming(string c)
		{
			CService.CreateCommand(c)
					.Alias("elevator", "dialog", "timing")
					.Description("Gives you a hint when to enter the elevator of a map. You can enter the map name, challenge mode name or the 3-letter map name code if you want.")
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

		public static Task GetSegmentedRun(string c)
		{
			CService.CreateCommand(c)
					.Description("Shows you a completed (or in progress) segmented run. Try the command without a parameter to get a random run.")
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
			return Task.FromResult(0);
		}
	}
}