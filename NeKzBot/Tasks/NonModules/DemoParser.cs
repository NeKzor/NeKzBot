using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using SourceDemoParser.Net;
using NeKzBot.Extensions;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Tasks.NonModules
{
	public static class DemoParser
	{
		private static readonly Fetcher _fetchClient = new Fetcher();

		private const string _cacheKey = "demo";
		private const uint _maxFileSize = 5000 * 1024;   // 5MB

		// Parse demo files
		public static async Task<bool> CheckForDemoFileAsync(MessageEventArgs args)
		{
			try
			{
				// Could be useful
				if (args.Message.Text.EndsWith($"{Configuration.Default.PrefixCmd}donotparse"))
					return true;
				if (args.Message.Text.EndsWith($"{Configuration.Default.PrefixCmd}dontparse"))
					return true;
				if (args.Message.Text.StartsWith("+render"))	// Support for GamingAndStuffs's bot
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
						if ((file.Size < _maxFileSize)
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
								var demo = await SourceDemo.ParseFileAsync(cachefile);
								await Bot.SendAsync(CustomRequest.SendMessage(args.Channel.Id), new CustomMessage(await Utils.GenerateDemoEmbed(demo)));
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
	}
}