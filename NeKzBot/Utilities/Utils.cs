using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Internals.Entities;
using NeKzBot.Server;

namespace NeKzBot.Utilities
{
	public static partial class Utils
	{
		// Turn collections to readable string list
		public static Task<string> CollectionToList(IEnumerable<string> collection, string formatting = "", string delimiter = ", ", string list = "", int customsub = 0)
		{
			var output = string.Empty;
			for (int i = 0; i < collection.Count(); i++)
				output += list + formatting + collection.ElementAt(i) + formatting + delimiter;
			return Task.FromResult(output.Substring(0, output.Length - ((customsub == 0) ? delimiter.Length : customsub)));
		}

		// Create multiple commands from Simple class collection
		public static Task CommandBuilder(Action<IEnumerable<string>> task, IEnumerable<Simple> commands)
		{
			var temp = commands.ToList();
			for (int i = 0; i < temp.Count; i++)
				task(temp[i].Value);
			return Task.FromResult(0);
		}

		public static Task CommandBuilder(Action<string, IEnumerable<string>> task, IEnumerable<Simple> commands, IEnumerable<string> aliases)
		{
			var temp = commands.ToList();
			foreach (var alias in aliases)
				for (int i = 0; i < temp.Count; i++)
					task(alias, temp[i].Value);
			return Task.FromResult(0);
		}

		// System Process
		public static async Task<string> GetCommandOutputAsync(string command, string parameters = "")
		{
			try
			{
				return await Process.Start(new ProcessStartInfo
				{
					FileName = command,
					Arguments = parameters,
					UseShellExecute = false,
					RedirectStandardOutput = true
				})
				.StandardOutput.ReadToEndAsync();
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Utils.GetCommandOutputAsync Error", e);
			}
			return "**Error.**";
		}

		public async static Task<string> GetDurationAsync(DateTime? time, bool withseconds = true)
			=> await GetDurationFromTimeSpan((time != default(DateTime?))
									   ? DateTime.UtcNow - time
									   : default(TimeSpan?), withseconds);

		public static Task<string> GetDurationFromTimeSpan(TimeSpan? duration, bool withseconds = true)
		{
			if (duration == default(TimeSpan?))
				return Task.FromResult(default(string));

			var output = (duration?.Days > 0)
										? $"{duration?.Days} Day{(duration?.Days == 1 ? string.Empty : "s")} "
										: string.Empty;
			output += (duration?.Hours > 0)
									  ? $"{duration?.Hours} Hour{(duration?.Hours == 1 ? string.Empty : "s")} "
									  : string.Empty;
			output += (duration?.Minutes > 0)
										? $"{duration?.Minutes} Minute{(duration?.Minutes == 1 ? string.Empty : "s")} "
										: string.Empty;
			output += ((duration?.Seconds > 0) && (withseconds))
										? $"{duration?.Seconds} Second{(duration?.Seconds == 1 ? string.Empty : "s")}"
										: string.Empty;
			output += ((duration?.Days > 0)
			&& (duration?.TotalDays > 365))
								   ? $" (about {Math.Round((decimal)duration?.TotalDays / 365, 1)} Year{(Math.Round((decimal)duration?.TotalDays / 365, 1) == (decimal)1.0 ? string.Empty : "s")})"
								   : string.Empty;
			return Task.FromResult((output != string.Empty)
										   ? output
										   : default(string));
		}

		public static async Task<string> GenerateModuleListAsync(string title, string description = default(string), string footer = default(string), string specialprefix = default(string), params string[] commands)
		{
			var output = string.Empty;
			foreach (var name in commands)
			{
				var command = await FindCommandByName((string.IsNullOrEmpty(specialprefix)) ? name : specialprefix + name);
				if (command == null)
					continue;
				output += $"\n• `{Configuration.Default.PrefixCmd}{command.Text}";
				if (command.Parameters.Any())
				{
					foreach (var parameter in command.Parameters)
					{
						if (parameter.Type == ParameterType.Multiple)
							output += $" <{parameter.Name}> <etc.>";
						else if (parameter.Type == ParameterType.Optional)
							output += $" ({parameter.Name})";
						else
							output += $" <{parameter.Name}>";
					}
				}
				output += "`";
			}
			var desc = (string.IsNullOrEmpty(description))
							  ? string.Empty
							  : $"\n{description}";
			return (string.IsNullOrEmpty(footer))
						  ? $"**[{title}]**{desc}{output}"
						  : $"**[{title}]**{desc}{output}\n{footer}";
		}
	}
}