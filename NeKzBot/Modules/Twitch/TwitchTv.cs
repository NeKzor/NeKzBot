using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules.Twitch
{
	public class TwitchTv
	{
		public static bool isRunning = false;

		public static async Task Start(int serverdelay = 8000)
		{
			await Task.Delay(serverdelay);
			await Logging.CON("TwitchTv auto started", ConsoleColor.DarkMagenta);
			isRunning = true;
			try
			{
				// Find channel to send to
				var dChannel = await Utils.GetChannel(Settings.Default.TwitchChannelName);

				// Reserve cache memory
				var cachekey = "twitchtv";
				await Caching.CApplication.Save(cachekey, new List<string>());

				for (;;)
				{
					// Get cache
					var cache = (List<string>)((await Caching.CApplication.Get(cachekey))[0]);

					//await Logging.CON("TwitchTv auto checking", ConsoleColor.DarkMagenta);
					foreach (var channel in Data.twitchStreamers)
					{
						// Giving this a scanning rate because why not
						await Task.Delay(500);

						dynamic streamer = await GetStreamer(channel);

						// Check if streamer is offline
						if (streamer?.stream != null)
						{
							// Ignore when already streaming
							if (cache.Contains(channel))
								continue;

							await Logging.CON($"{channel} IS LIVE", ConsoleColor.DarkMagenta);
							await Logging.CON($"TwitchTv data cache -> {Utils.StringInBytes(channel)} bytes", ConsoleColor.Red);
							cache.Add(channel);

							var name = streamer?.stream?.channel?.display_name?.ToString() ?? "**ERROR**";
							var game = "playing " + streamer?.stream?.channel?.game?.ToString() ?? "streaming";
							var viewers = streamer?.stream?.viewers?.ToString() ?? "**ERROR**";
							var title = streamer?.stream?.channel?.status?.ToString() ?? "**ERROR**";
							var link = streamer?.stream?.channel?.url?.ToString() ?? "**ERROR**";
							var preview = streamer?.stream?.preview?.large.ToString() ?? "**ERROR";
							await dChannel?.SendMessage($"{preview}\n**{name}** is now {game} for {viewers} viewers!\n*{title}*\n{link}");
						}
						else
						{
							// Remove from cache when not streaming
							if (cache.Contains(channel))
								cache.Remove(channel);
						}
					}

					// Save cache
					await Caching.CApplication.Save(cachekey, cache);
					cache = null;

					// Check in 3 minutes again
					await Task.Delay(3 * 60000);
				}
			}
			catch (Exception ex)
			{
				await Logging.CON("TwitchTv error",ex);
			}
			isRunning = false;
		}

		public static async Task<string> GetPreview(string channel)
		{
			var streamer = await GetStreamer(channel);
			return streamer == null ? "**Error**" :
				streamer?.stream == null ?
				"Streamer is offline." : streamer?.stream?.preview?.large?.ToString();
		}

		private static async Task<dynamic> GetStreamer(string channel)
		{
			// Twitch API
			var json = string.Empty;
			try
			{
				// Download
				json = await Fetching.GetString($"https://api.twitch.tv/kraken/streams/{channel}?client_id={Credentials.Default.TwitchToken}");
			}
			catch (Exception ex)
			{
				await Logging.CHA("Fetching error", ex);
				return null;
			}

			// Read
			if (string.IsNullOrEmpty(json))
			{
				await Logging.CON("TwitchTv web client error", ConsoleColor.Red);
				return null;
			}

			// Read json string
			dynamic streamer = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
			if (string.IsNullOrEmpty(streamer?.ToString()))
			{
				await Logging.CON("TwitchTv json error", ConsoleColor.Red);
				return null;
			}
			return streamer;
		}
	}
}