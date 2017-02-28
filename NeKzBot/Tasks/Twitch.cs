using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Classes.Discord;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Webhooks;

namespace NeKzBot.Tasks
{
	public static class Twitch
	{
		public static bool IsRunning { get; set; } = false;
		public static InternalWatch Watch { get; } = new InternalWatch();
		private static string _cacheKey;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Twitch", LogColor.Init);
			_cacheKey = _cacheKey ?? "twitchtv";
		}

		public static async Task StartAsync(int serverdelay = 8000)
		{
			await Task.Delay(serverdelay);
			await Logger.SendAsync("Twitch Started", LogColor.Twitch);
			IsRunning = true;
			try
			{
				// Reserve cache memory
				await Caching.CApplication.SaveCacheAsync(_cacheKey, new List<string>());

				for (;;)
				{
					// Get cache
					var cache = (await Caching.CApplication.GetCacheAsync(_cacheKey))[0] as List<string>;

					foreach (var streamer in Data.TwitchStreamers)
					{
						// Giving this a scanning rate because why not
						await Task.Delay(1000);

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
								ChannelName = api?.stream?.channel?.display_name?.ToString() ?? "**ERROR**",
								Game = new TwitchGame
								{
									Name = (gamename != null) ? $"Playing {gamename}" : "Streaming",
									BoxArt = (await Classes.TwitchTv.GetGameAsync(gamename))?.games[0]?.box?.medium?.ToString() ?? string.Empty
								},
								StreamTitle = api?.stream?.channel?.status?.ToString() ?? "**ERROR**",
								StreamLink = api?.stream?.channel?.url?.ToString() ?? "**ERROR**",
								PreviewLink = api?.stream?.preview?.large?.ToString(),
								ChannelViewers = (api?.stream?.viewers != null) ? uint.Parse(api.stream.viewers.ToString()) : 0,
								AvatarLink = api?.stream?.channel?.logo?.ToString() ?? "https://static-cdn.jtvnw.net/jtv_user_pictures/xarth/404_user_70x70.png"
							};

							// Save preview image, upload to dropbox and create a link
							var filename = $"{stream.ChannelName}.jpg";
							var path = await Utils.GetPath() + $"/Resources/Cache/{filename}";
							await Fetching.GetFileAsync(stream.PreviewLink, path);
							await DropboxCom.UploadAsync("TwitchCache", filename, path);

							// Overwrite new preview link
							stream.PreviewLink = $"{await DropboxCom.CreateLinkAsync($"TwitchCache/{filename}")}&raw=1";

							foreach (var item in Data.TwitchTvSubscribers)
							{
								 await WebhookService.ExecuteWebhookAsync(item, new Webhook
								{
									UserName = item.UserName,
									AvatarUrl = "https://s3-us-west-2.amazonaws.com/web-design-ext-production/p/Glitch_474x356.png",
									Embeds = new Embed[] { await CreateEmbed(stream) }
								});
							}
						}
						else // Remove from cache when not streaming
							if (cache.Contains(streamer))
								cache.Remove(streamer);
					}
					// Save cache
					await Caching.CApplication.SaveCacheAsync(_cacheKey, cache);
					cache = null;

					// Check in 3 minutes again
					await Task.Delay((3 * 60000) - await Watch.GetElapsedTimeAsync(message: "Twitch.StartAsync Delay Took -> "));
					await Watch.RestartAsync();
				}
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Twitch.StartAsync Error", e);
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

		private static Task<Embed> CreateEmbed(TwitchStream stream)
		{
			return Task.FromResult(new Embed
			{
				Author = new EmbedAuthor(stream.ChannelName, stream.StreamLink, stream.AvatarLink),
				Title = "Twitch Livestream",
				Description = $"{stream.Game.Name} for {stream.ChannelViewers} viewers!\n\n_{stream.StreamTitle}_",
				Url = stream.StreamLink,
				Color = Data.TwitchColor.RawValue,
				Thumbnail = new EmbedThumbnail(stream.Game.BoxArt),
				Image = new EmbedImage(stream.PreviewLink),
				Timestamp = DateTime.UtcNow.ToString("s"),
				Footer = new EmbedFooter("twitch.tv", "https://www.twitch.tv/favicon.ico")
			});
		}
	}
}