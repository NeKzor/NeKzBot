using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
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
			private static string _cacheKey;
			private static readonly string _webhookavatar = "https://pbs.twimg.com/profile_images/500500884757831682/L0qajD-Q_400x400.png";	// Should make a static link instead because this might break once...

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing SpeedrunCom AutoNotification", LogColor.Init);
				_notificationCount = 10;
				_refreshTime = 1 * 60 * 1000;	// 1 minute
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
							if (nfstosend?.Count > 0)
							{
								nfstosend.Reverse();
								foreach (var notification in nfstosend)
								{
									var hook = new Webhook
									{
										UserName = "SpeedrunCom",
										AvatarUrl = _webhookavatar,
										Embeds = new Embed[] { await CreateEmbed(notification) }
									};

									// I really hope API v2 is better :s
									if ((notification.Game.Name.Contains("Portal 2"))       // Should include Aperture Tag too
									|| (notification.Game.Name.Contains("Portal Stories"))) // Rip other game mods
										foreach (var subscriber in (await Data.Get<Subscribers>("srcomportal2hook")).Subs)
											await WebhookService.ExecuteWebhookAsync(subscriber, hook);

									foreach (var subscriber in (await Data.Get<Subscribers>("srcomsourcehook")).Subs)
										await WebhookService.ExecuteWebhookAsync(subscriber, hook);
								}
								// Save cache
								var newcache = nfstosend[nfstosend.Count - 1].Cache;
								await Logger.SendAsync($"SpeedrunCom.AutoNotification.StartAsync Caching -> {await Utils.StringInBytes(newcache)} bytes", LogColor.Caching);
								await Caching.CFile.SaveCacheAsync(_cacheKey, newcache);
								newcache = null;
								cache = null;
							}
						}
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

			private static Task<Embed> CreateEmbed(SpeedrunNotification nf)
			{
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
				return Task.FromResult(embed);
			}
		}
	}
}