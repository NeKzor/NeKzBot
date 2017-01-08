using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Properties;

namespace NeKzBot
{
	public class TwitchTv
	{
		public static async Task Start(int serverdelay = 8000)
		{
			try
			{
				await Task.Delay(serverdelay);
				Logging.CON("TwitchTv auto started", ConsoleColor.DarkMagenta);

				// Find channel to send to
				var dChannel = GetChannelByName();

				// Reserve cache memory
				var cachekey = "twitchtv";
				Caching.CApplication.Save(cachekey, new List<string>());

				for (;;)
				{
					// Get cache
					var cache = (List<string>)Caching.CApplication.Get(cachekey)[0];

					Logging.CON("TwitchTv auto checking", ConsoleColor.DarkMagenta);
					foreach (var channel in Data.twitchStreamers)
					{
						// Giving this a scanning rate because why not
						await Task.Delay(500);

						// Twitch API link
						string url = $"https://api.twitch.tv/kraken/streams/{channel}?client_id={Settings.Default.TwitchClientToken}";
						var json = "";

						try
						{
							// Download
							json = await Fetching.GetString(url);
						}
						catch (Exception ex)
						{
							Logging.CHA($"Fetching error\n{ex.ToString()}");
							continue;
						}

						// Read
						if (string.IsNullOrEmpty(json))
						{
							Logging.CON("TwitchTv web client error");
							continue;
						}

						// Read json string
						dynamic streamer = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
						if (string.IsNullOrEmpty(streamer?.ToString()))
						{
							Logging.CON("TwitchTv json error");
							continue;
						}

						// Check if streamer is offline
						if (streamer?.stream != null)
						{
							// Ignore when already streaming
							if (cache.Contains(channel))
								continue;

							Logging.CON($"{channel} IS LIVE", ConsoleColor.DarkMagenta);
							Logging.CON($"CACHING DATA WITH {Utils.StringInBytes(channel)} bytes");
							cache.Add(channel);

							var name = streamer?.stream?.channel?.display_name ?? "**ERROR**";
							var game = "playing " + streamer?.stream?.channel?.game?.ToString() ?? "streaming";
							var viewers = streamer?.stream?.viewers ?? "**ERROR**";
							var title = streamer?.stream?.channel?.status ?? "**ERROR**";
							var link = streamer?.stream?.channel?.url ?? "**ERROR**";
							await dChannel?.SendMessage($"**{name?.ToString()}** is now {game?.ToString()} for {viewers?.ToString()} viewers!\n*{title?.ToString()}*\n{link?.ToString()}");
						}
						else
						{
							// Remove from cache when not streaming
							if (cache.Contains(channel))
								cache.Remove(channel);
						}
					}

					// Save cache
					Caching.CApplication.Save(cachekey, cache);
					cache = null;

					// Check in 1 minute again
					await Task.Delay(60000);
				}
			}
			catch (Exception ex)
			{
				Logging.CHA($"TwitchTv error\n{ex.ToString()}");
			}
		}

		// Get default twitch channel name
		private static Discord.Channel GetChannelByName(string serverName = null, string channelName = null)
		{
			serverName = serverName ?? Settings.Default.ServerName;
			channelName = channelName ?? Settings.Default.TwitchChannelName;
			return NBot.dClient?.FindServers(serverName)?.First().FindChannels(channelName, Discord.ChannelType.Text, true)?.First() ?? null;
		}
	}
}