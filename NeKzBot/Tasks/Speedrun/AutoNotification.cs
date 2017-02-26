using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Server;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Internals;
using NeKzBot.Classes.Discord;
using NeKzBot.Webhooks;

namespace NeKzBot.Tasks.Speedrun
{
	public static partial class SpeedrunCom
	{
		internal static class AutoNotification
		{
			public static bool IsRunning { get; set; } = false;
			public static InternalWatch Watch { get; } = new InternalWatch();
			private static uint _notificationCount;
			private static string _cacheKey;

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing SpeedrunCom AutoNotification", LogColor.Init);
				_notificationCount = 50;
				_cacheKey = "autonf";

				// Reserve cache memory
				await Caching.CFile.AddKeyAsync(_cacheKey);
				if (await Caching.CFile.GetFileAsync(_cacheKey) == null)
					await Caching.CFile.SaveCacheAsync(_cacheKey, string.Empty);
			}

			public static async Task StartAsync(int clientdelay = 8000)
			{
				await Task.Delay(clientdelay);
				await Logger.SendAsync("AutoNotification Started", LogColor.Speedrun);
				IsRunning = true;
				try
				{
					for (;;)
					{
						// Get cache
						var cache = await Caching.CFile.GetFileAsync(_cacheKey);

						// Download data
						var notifications = (string.IsNullOrEmpty(cache))
													? await GetNotificationUpdatesAsync(1)
													: await GetNotificationUpdatesAsync(_notificationCount);

						if ((notifications != null)
						&& (notifications?.Count > 0))
						{
							// Find the last notification
							var nfstosend = new List<SpeedrunNotification>();
							foreach (var notification in notifications)
							{
								if (cache != notification.Cache)
									nfstosend.Add(notification);
								else
									break;
							}
							if (nfstosend.Count == 0)
								continue;

							nfstosend.Reverse();
							foreach (var notification in nfstosend)
							{
								foreach (var item in Data.SRComSubscribers)
								{
									await WebhookService.ExecuteWebhookAsync(item, new Webhook
									{
										UserName = item.UserName,
										AvatarUrl = "https://pbs.twimg.com/profile_images/500500884757831682/L0qajD-Q_400x400.png",
										Embeds = new Embed[] { await CreateEmbed(notification) }
									});
								}
							}
							// Save cache
							var newcache = nfstosend[nfstosend.Count - 1].Cache;
							await Logger.SendAsync($"SpeedrunCom.AutoNotification.StartAsync Caching -> {await Utils.StringInBytes(newcache)} bytes", LogColor.Caching);
							await Caching.CFile.SaveCacheAsync(_cacheKey, newcache);
						}
						await Task.Delay((1 * 60000) - await Watch.GetElapsedTimeAsync(message: "Speedrun.AutoNotification.StartAsync Delay Took -> "));   // Check in about every minute (max speed request is 100 per min tho)
						await Watch.RestartAsync();
					}
				}
				catch
				{
					await Logger.SendAsync("Speedrun.AutoNotification.StartAsync Error", LogColor.Error);
				}
				IsRunning = false;
				await Logger.SendAsync("SpeedrunCom.AutoNotification.StartAsync Ended", LogColor.Speedrun);
			}

			private static Task<Embed> CreateEmbed(SpeedrunNotification nf)
			{
				return Task.FromResult(new Embed
				{
					Author = new EmbedAuthor(nf.Author, $"https://www.speedrun.com/{nf.Author}", $"https://www.speedrun.com/themes/user/{nf.Author}/image.png"),
					Title = "Latest Notification",
					Description = nf.FormattedText,
					Url = nf.ContentLink,
					Color = Data.SpeedruncomColor.RawValue,
					Thumbnail = new EmbedThumbnail(nf.Game.CoverLink),
					Timestamp = DateTime.UtcNow.ToString("s"),  // Close enough
					Footer = new EmbedFooter("speedrun.com", "https://www.speedrun.com/themes/default/favicon.png")
				});
			}
		}
	}
}