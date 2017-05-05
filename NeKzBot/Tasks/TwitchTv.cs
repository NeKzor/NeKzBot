using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;
using NeKzBot.Webhooks;

namespace NeKzBot.Tasks
{
	public static class TwitchTv
	{
		public static bool IsRunning { get; set; } = false;
		public static InternalWatch Watch { get; } = new InternalWatch();
		private static string _cacheKey;
		private static uint _refreshTime;
		private static uint _delayFactor;
		private static Fetcher _fetchClient;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Twitch", LogColor.Init);
			_fetchClient = new Fetcher();
			_cacheKey = "twitchtv";
			_refreshTime = 5 * 60 * 1000;   // 5 minutes
			_delayFactor = 10;
		}

		public static async Task StartAsync(int serverdelay = 8000)
		{
			//await Task.Delay(serverdelay);
			await Logger.SendAsync("TwitchTv Started", LogColor.Twitch);
			IsRunning = true;
			try
			{
				// Reserve cache memory
				await Caching.CFile.AddKeyAsync(_cacheKey);

				for (;;)
				{
					// Get cache
					var cache = (await Caching.CFile.GetFileAsync(_cacheKey))?.Split('|').ToList();
					if (cache == null)
					{
						await Caching.CFile.SaveCacheAsync(_cacheKey, string.Empty);
						continue;
					}
#if DEBUG
					await Logger.SendAsync(await Utils.CollectionToList(cache, delimiter: "|"));
#endif
					var streamers = (await Data.Get<Simple>("streamers")).Value;
					foreach (var streamer in streamers.ToArray())
					{
						// Giving this a scanning rate because why not
						await Task.Delay((int)_delayFactor * streamers.Count);

						var obj = await TwitchApi.GetStreamAsync(streamer);
						if (obj == null)
							continue;

						// Check if streamer is offline
						var stream = obj.Stream;
						if (stream != null)
						{
							// Ignore when already streaming
							if (cache.Contains(streamer))
								continue;

							await Logger.SendAsync($"{streamer} IS LIVE", LogColor.Twitch);
							await Logger.SendAsync($"TwitchTv.StartAsync Caching -> {await Utils.StringInBytes(streamer)} bytes", LogColor.Caching);
							cache.Add(streamer);

							// Save preview image, upload to dropbox and create a link
							var filename = $"{stream.Channel?.DisplayName ?? "error"}.jpg";
							var path = await Utils.GetAppPath() + $"/Resources/Cache/{filename}";
							await _fetchClient.GetFileAsync(stream.Preview.Large, path);

							// Not sure if this is actually a good idea, it delays everything :c
							await DropboxCom.DeleteFileAsync("TwitchCache", filename);
							await DropboxCom.UploadAsync("TwitchCache", filename, path);

							// Overwrite new preview link
							var link = await DropboxCom.CreateLinkAsync($"TwitchCache/{filename}");
							stream.Preview.Large = (link != null)
														 ? $"{link}&raw=1"
														 : stream.Preview.Large;

							foreach (var item in (await Data.Get<Subscription>("twtvhook")).Subscribers)
							{
								await WebhookService.ExecuteWebhookAsync(item, new Webhook
								{
									UserName = "TwitchTv",
									AvatarUrl = "https://s3-us-west-2.amazonaws.com/web-design-ext-production/p/Glitch_474x356.png",
									Embeds = new Embed[] { await CreateEmbedAsync(stream) }
								});
							}
						}
						// Remove from cache when not streaming
						else if (cache.Contains(streamer))
							cache.Remove(streamer);
						obj = null;
					}
					// Clean up cache
					var newcache = new List<string>() { string.Empty };
					foreach (var item in cache.Skip(1))
						if (streamers.Contains(item))
							newcache.Add(item);
					// Save cache
#if DEBUG
					await Logger.SendAsync(await Utils.CollectionToList(newcache, delimiter: "|"));
#endif
					await Caching.CFile.SaveCacheAsync(_cacheKey, await Utils.CollectionToList(newcache, delimiter: "|"));
					cache = newcache = null;

					var delay = (int)(_refreshTime) - await Watch.GetElapsedTime(debugmsg: "TwitchTv.StartAsync Delay Took -> ");
					await Task.Delay((delay > 0 ) ? delay : 0);
					await Watch.RestartAsync();
				}
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("TwitchTv.StartAsync Error", e);
			}
			IsRunning = false;
			await Logger.SendToChannelAsync("TwitchTv.StartAsync Ended", LogColor.Twitch);
		}

		public static async Task<string> GetPreviewAsync(string channel)
		{
			var streamer = await TwitchApi.GetStreamAsync(channel);
			return (streamer == null)
							 ? TwitchError.Generic
							 : (streamer.Stream == null)
												? TwitchError.Offline
												: streamer.Stream.Preview?.Large;
		}

		private static async Task<Embed> CreateEmbedAsync(TwitchStream stream)
		{
			var people = default(string);
			switch (stream.Viewers)
			{
				case 0:
					people = string.Empty;
					break;
				case 1:
					people = " for 1 viewer";
					break;
				default:
					people = $" for {stream.Viewers} viewers";
					break;
			}

			// Get box art of game for the embed thumbnail
			var obj = await TwitchApi.GetGamesAsync(stream.Game);
			var game = obj.Games.FirstOrDefault(g => string.Equals(g.Name, stream.Game, StringComparison.CurrentCultureIgnoreCase));

			return new Embed
			{
				Author = new EmbedAuthor(stream.Channel.DisplayName, stream.Channel.Url, stream.Channel.Logo),
				Title = "Twitch Livestream",
				Description = $"{await Utils.AsRawText(string.IsNullOrEmpty(stream.Game) ? "Streaming" : $"Playing {stream.Game}")}{people}!\n\n_{await Utils.AsRawText($"[{stream.Channel.Status}]({stream.Channel.Url})")}_",
				Url = stream.Channel.Url,
				Color = Data.TwitchColor.RawValue,
				Thumbnail = new EmbedThumbnail(game?.Box?.Medium),
				Image = new EmbedImage(stream.Preview.Large),
				Timestamp = DateTime.UtcNow.ToString("s"),
				Footer = new EmbedFooter("twitch.tv", Data.TwitchTvIconUrl)
			};
		}
	}
}