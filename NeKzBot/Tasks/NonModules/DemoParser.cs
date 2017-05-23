using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using SourceDemoParser.Net;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Tasks.NonModules
{
	public static class DemoParser
	{
		private static readonly Fetcher _fetchClient = new Fetcher();
		private static readonly string _cacheKey = "demo";

		private const uint _maxfilesize = 5000 * 1024;   // 5MB

		// Parse demo files
		public static async Task<bool> CheckForDemoFileAsync(MessageEventArgs args)
		{
			try
			{
				// Could be useful
				if (args.Message.Text.EndsWith($"{Configuration.Default.PrefixCmd}donotparse"))
					return true;

				// I've never seen a message that had more than one attachment
				if (args.Message.Attachments.Length == 1)
				{
					var file = args.Message.Attachments.First();

					// Check file extension
					var filename = file.Filename;
					var extension = Path.GetExtension(filename) ?? string.Empty;

					// Only allow Source Engine demos
					if (extension == ".dem")
					{
						// Maximum 5000KB
						if ((file.Size < _maxfilesize)
						&& (file.Size != 0))
						{
							// Should give every user his own cache too
							var cachekey = $"{_cacheKey}{args.User.Id}";

							// Download data
							try
							{
								await _fetchClient.GetFileAndCacheAsync(file.Url, cachekey);
							}
							catch (Exception e)
							{
								await Logger.SendAsync("Fetching.GetFileAndCacheAsync Error (DemoParser.CheckForDemoFileAsync)", e);
								return true;
							}

							// Get file
							var cachefile = await Caching.CFile.GetPathAndSaveAsync(cachekey);
							if (string.IsNullOrEmpty(cachefile))
								await Logger.SendAsync("Caching.CFile.GetPathAndSaveAsync Error (DemoParser.CheckForDemoFileAsync)", LogColor.Error);
							else
							{
								// Parser throws exception if something failed
								var demo = await SourceDemo.Parse(cachefile);
								await Bot.SendAsync(CustomRequest.SendMessage(args.Channel.Id), new CustomMessage(await GenerateEmbed(demo)));
							}
						}
					}
				}
				else
					return false;
			}
			catch (Exception e)
			{
				await Logger.SendAsync("DemoParser.CheckForDemoFileAsync Error", e);
			}
			return true;
		}

		private static Task<Embed> GenerateEmbed(SourceDemo demo)
		{
			var game = demo.GameInfo.Name;
			var mode = demo.GameInfo.Mode;
			var player = demo.Client;
			var mapname = demo.MapName;
			var alias = demo.GameInfo.GetMapAlias(mapname);
			var adjusted = demo.AdjustedTicks;
			var ticks = demo.PlaybackTicks;
			var time = demo.PlaybackTime;
			var tickrate = demo.Tickrate;
			if (ticks != adjusted)
			{
				ticks = adjusted;
				time = demo.AdjustedTime;
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