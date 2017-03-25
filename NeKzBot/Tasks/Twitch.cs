using System;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;
using NeKzBot.Webhooks;

namespace NeKzBot.Tasks
{
	public static class Twitch
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
			await Task.Delay(serverdelay);
			await Logger.SendAsync("Twitch Started", LogColor.Twitch);
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

					foreach (var streamer in (await Data.Get<Simple>("streamers")).Value)
					{
						// Giving this a scanning rate because why not
						await Task.Delay((int)_delayFactor * (await Data.Get<Simple>("streamers")).Value.Count);

						dynamic api = await TwitchTv.GetStreamAsync(streamer);

						// Check if streamer is offline
						if (api?.stream != null)
						{
							// Ignore when already streaming
							if (cache.Contains(streamer))
								continue;

							await Logger.SendAsync($"{streamer} IS LIVE", LogColor.Twitch);
							await Logger.SendAsync($"Twitch.StartAsync Caching -> {await Utils.StringInBytes(streamer)} bytes", LogColor.Caching);
							cache.Add(streamer);

							var gamename = api?.stream?.channel?.game?.ToString();
							var stream = new TwitchStream()
							{
								ChannelName = api?.stream?.channel?.display_name?.ToString() ?? "ERROR",
								Game = new TwitchGame
								{
									Name = (gamename != null) ? $"Playing {gamename}" : "Streaming",
									BoxArt = (await TwitchTv.GetGameAsync(gamename))?.games[0]?.box?.medium?.ToString() ?? string.Empty
								},
								StreamTitle = api?.stream?.channel?.status?.ToString() ?? "ERROR",
								StreamLink = api?.stream?.channel?.url?.ToString() ?? "ERROR",
								PreviewLink = api?.stream?.preview?.large?.ToString(),
								ChannelViewers = (api?.stream?.viewers != null) ? uint.Parse(api.stream.viewers.ToString()) : 0,
								AvatarLink = api?.stream?.channel?.logo?.ToString() ?? "https://static-cdn.jtvnw.net/jtv_user_pictures/xarth/404_user_70x70.png"
							};

							// Save preview image, upload to dropbox and create a link
							var filename = $"{stream.ChannelName}.jpg";
							var path = await Utils.GetAppPath() + $"/Resources/Cache/{filename}";
							await _fetchClient.GetFileAsync(stream.PreviewLink, path);

							// Not sure if this is actually a good idea, it delays everything :c
							await DropboxCom.DeleteFileAsync("TwitchCache", filename);
							await DropboxCom.UploadAsync("TwitchCache", filename, path);

							// Overwrite new preview link
							var link = await DropboxCom.CreateLinkAsync($"TwitchCache/{filename}");
							stream.PreviewLink = (link != null)
													   ? $"{link}&raw=1"
													   : stream.PreviewLink;

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
						else // Remove from cache when not streaming
							if (cache.Contains(streamer))
								cache.Remove(streamer);
						api = null;
					}
					// Save cache
					await Caching.CFile.SaveCacheAsync(_cacheKey, await Utils.CollectionToList(cache, delimiter: "|"));
					cache = null;

					var delay = (int)(_refreshTime) - await Watch.GetElapsedTime(debugmsg: "Twitch.StartAsync Delay Took -> ");
					await Task.Delay((delay > 0 ) ? delay : 0);
					await Watch.RestartAsync();
				}
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Twitch.StartAsync Error", e);
			}
			IsRunning = false;
			await Logger.SendToChannelAsync("Twitch.StartAsync Ended", LogColor.Twitch);
		}

		public static async Task<string> GetPreviewAsync(string channel)
		{
			var streamer = await TwitchTv.GetStreamAsync(channel);
			return (streamer == null)
							 ? TwitchError.Generic
							 : (streamer?.stream == null)
												 ? TwitchError.Offline
												 : streamer?.stream?.preview?.large?.ToString();
		}

		private static async Task<Embed> CreateEmbedAsync(TwitchStream stream)
		{
			var people = default(string);
			switch (stream.ChannelViewers)
			{
				case 0:
					people = string.Empty;
					break;
				case 1:
					people = " for 1 viewer";
					break;
				default:
					people = $" for {stream.ChannelViewers} viewers";
					break;
			}

			return new Embed
			{
				Author = new EmbedAuthor(stream.ChannelName, stream.StreamLink, stream.AvatarLink),
				Title = "Twitch Livestream",
				Description = $"{await Utils.AsRawText(stream.Game.Name)}{people}!\n\n_{await Utils.AsRawText($"[{stream.StreamTitle}]({stream.StreamLink})")}_",
				Url = stream.StreamLink,
				Color = Data.TwitchColor.RawValue,
				Thumbnail = new EmbedThumbnail(stream.Game.BoxArt),
				Image = new EmbedImage(stream.PreviewLink),
				Timestamp = DateTime.UtcNow.ToString("s"),
				Footer = new EmbedFooter("twitch.tv", Data.TwitchTvIconUrl)
			};
		}
	}
}