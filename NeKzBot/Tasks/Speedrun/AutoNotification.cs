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

namespace NeKzBot.Tasks.Speedrun
{
	public static partial class SpeedrunCom
	{
		internal static class AutoNotification
		{
			public static bool IsRunning { get; set; } = false;
			public static InternalWatch Watch { get; } = new InternalWatch();
			private static uint _notificationCount;
			private static uint _refreshTime;
			private static uint _rateLimit;
			private static string _cacheKey;

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing SpeedrunCom AutoNotification", LogColor.Init);
				_notificationCount = 50;
				_refreshTime = 1 * 60 * 1000;   // 1 minute
				_rateLimit = 20;                // ^Max 10 notifications in this time
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

						if (notifications?.Count > 0)
						{
							// Find the last notification
							var nfstosend = new List<SpeedrunNotification>();
							foreach (var notification in notifications)
							{
								if (cache != notification.Id)
									nfstosend.Add(notification);
								else
									break;
							}

							// Rate limit
							if (nfstosend.Count <= _rateLimit)
							{
								nfstosend.Reverse();
								foreach (var notification in nfstosend)
								{
									var hook = new Webhook
									{
										UserName = "SpeedrunCom",
										AvatarUrl = Data.SpeedrunComWebhookAvatar,
										Embeds = new Embed[] { await CreateEmbedAsync(notification) }
									};

									// I really hope API v2 is better :s
									if ((notification.Game.Name.Contains("Portal 2"))       // Should include Aperture Tag too
									|| (notification.Game.Name.Contains("Portal Stories"))) // Rip other game mods
										foreach (var subscriber in (await Data.Get<Subscription>("srcomportal2hook")).Subscribers)
											await WebhookService.ExecuteWebhookAsync(subscriber, hook);

									foreach (var subscriber in (await Data.Get<Subscription>("srcomsourcehook")).Subscribers)
										await WebhookService.ExecuteWebhookAsync(subscriber, hook);
								}

								if (nfstosend.Count >= 1)
								{
									// Save cache
									var newcache = nfstosend.Last().Id;
									await Logger.SendAsync($"SpeedrunCom.AutoNotification.StartAsync Caching -> {await Utils.StringInBytes(newcache)} bytes", LogColor.Caching);
									await Caching.CFile.SaveCacheAsync(_cacheKey, newcache);
									newcache = null;
								}
							}
							else
								await Logger.SendAsync("SpeedrunCom.AutoNotification.StartAsync Rate Limit Exceeded", LogColor.Error);
							nfstosend = null;
						}
						notifications = null;
						cache = null;
						// Check every minute (max speed request is 100 per min tho)
						var delay = (int)(_refreshTime) - await Watch.GetElapsedTime(debugmsg: "Speedrun.AutoNotification.StartAsync Delay Took -> ");
						await Task.Delay((delay > 0) ? delay : 0);
						await Watch.RestartAsync();
					}
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("Speedrun.AutoNotification.StartAsync Error", e);
				}
				IsRunning = false;
				await Logger.SendToChannelAsync("SpeedrunCom.AutoNotification.StartAsync Ended", LogColor.Speedrun);
			}

			private static async Task<Embed> CreateEmbedAsync(SpeedrunNotification nf)
			{
				if (string.IsNullOrEmpty(nf.FormattedText))
					await Logger.SendToChannelAsync($"SpeedrunCom.CreateEmbedAsync Text Is Empty (ID = {nf.Id}", LogColor.Speedrun);

				var embed = new Embed
				{
					Title = "Latest Notification",
					Description = nf.FormattedText,
					Url = nf.ContentLink,
					Color = Data.SpeedruncomColor.RawValue,
					Thumbnail = new EmbedThumbnail(nf.Game.CoverLink),
					Timestamp = DateTime.UtcNow.ToString("s"),
					Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl)
				};
				if (nf.Author != null)
					embed.WithAuthor(new EmbedAuthor(nf.Author.Name, $"https://www.speedrun.com/{nf.Author.Name}", $"https://www.speedrun.com/themes/user/{nf.Author.Name}/image.png"));
				return embed;
			}
		}
	}
}