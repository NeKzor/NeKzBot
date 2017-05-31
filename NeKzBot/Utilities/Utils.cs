using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using SourceDemoParser.Net;
using NeKzBot.Extensions;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
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

		public static Task<Embed> GenerateDemoEmbed(SourceDemo demo)
		{
			var game = demo.GameInfo.Name;
			var mode = demo.GameInfo.Mode;
			var player = demo.Client;
			var mapname = demo.MapName;
			var alias = demo.GameInfo.GetMapAlias(mapname);
			var adjusted = demo.AdjustedTicks;
			var ticks = demo.PlaybackTicks;
			var time = demo.PlaybackTime;
			var tickrate = demo.GetTickrate();
			if (ticks != adjusted)
			{
				ticks = adjusted;
				time = demo.GetAdjustedTime();
			}
			// Just filling the last embed field with useless and inaccurate information because I don't know what else I could put in there
			// Calculate jump count (not a real jump but if you could somehow combine this by looking for the FL_ONGROUND flag then it should work)
			var jumps = demo.ConsoleCommands.Where(frame => frame.ConsoleCommand.StartsWith("+jump"));
			var registeredjumps = jumps.Count();
			var command = jumps.FirstOrDefault();
			var actualjumps = 0;
			if (registeredjumps > 1)
			{
				foreach (var frame in jumps.Skip(1))
				{
					if (command.CurrentTick != frame.CurrentTick)
						actualjumps++;
					command = frame;
				}
			}
			else
				actualjumps = registeredjumps;

			return Task.FromResult(new Embed
			{
				Title = "Demo Info",
				Url = "https://github.com/NeKzor/SourceDemoParser.Net",
				Color = Data.BasicColor.RawValue,
				Fields = new EmbedField[]
				{
					// I trust you guys, do not try to break this :s (escaping)
					new EmbedField("Game",  game + $"{((string.IsNullOrEmpty(mode)) ? string.Empty : $"\n{mode}")}", true),
					new EmbedField("Player", player, true),
					new EmbedField("Time", $"{ticks} ticks\n{time.ToString("N3")}s", true),
					new EmbedField("Map",  mapname + $"{((string.IsNullOrEmpty(alias)) ? string.Empty : $"\n{alias}")}", true),
					new EmbedField("Tickrate", $"{tickrate}", true),
					new EmbedField("Stats", $"Jump Inputs: {registeredjumps}\nJump Ticks: {actualjumps}", true)
				},
				Footer = new EmbedFooter("Parsed with SourceDemoParser.Net")
			});
		}
	}
}