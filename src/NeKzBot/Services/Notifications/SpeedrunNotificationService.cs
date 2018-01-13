using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.API;
using NeKzBot.Data;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications
{
	public class SpeedrunNotificationService : NotificationService
	{
		private SpeedrunComApiClient _client;

		public SpeedrunNotificationService(IConfiguration config, LiteDatabase dataBase)
			: base (config, dataBase)
		{
		}

		public override Task Initialize()
		{
			base.Initialize();

			_userName = "SpeedrunCom";
			_userAvatar = "https://github.com/NeKzor/NeKzBot/blob/master/public/resources/avatars/speedruncom_avatar.png";
			_sleepTime = 1 * 60 * 1000;

			_client = new SpeedrunComApiClient(_config["user_agent"], _config["speedrun_token"]);

			var data = _dataBase.GetCollection<SpeedrunCacheData>();
			var cache = data.FindOne(d => d.Id == _globalId);
			if (cache == null)
			{
				cache = new SpeedrunCacheData()
				{
					Id = _globalId,
					Notifications = new List<SpeedrunNotification>()
				};
				data.Insert(cache);
			}

			return Task.CompletedTask;
		}

		public override async Task StartAsync()
		{
			try
			{
				while (!_isRunning)
				{
					var watch = Stopwatch.StartNew();

					var db = _dataBase.GetCollection<SpeedrunCacheData>();
					var data = db.FindOne(d => d.Id == _globalId);
					if (data == null)
						throw new Exception("Data cache not found!");

					var cache = data.Notifications;
					var notifications = await _client.GetNotificationsAsync(100);
					var sending = new List<SpeedrunNotification>();

					// Will skip for the very first time
					if (cache.Any())
					{
						// Check cached notification
						foreach (var old in cache)
						{
							foreach (var notification in notifications)
							{
								if (old.Id == notification.Id)
									goto send;
								sending.Add(notification);
							}
						}
						throw new Exception("Could not find the last notification entry!");
					}
send:
					var subscribers = _dataBase.GetCollection<SubscriptionData>(_globalId);
					if (subscribers.Count() > 0)
					{
						if (sending.Count <= 10)
						{
							sending.Reverse();
							foreach (var tosend in sending)
							{
								var embed = await CreateEmbedAsync(tosend);

								// There might be a problem if we have lots of subscribers (retry then?)
								foreach (var subscriber in subscribers.FindAll())
								{
									using (var vc = new DiscordWebhookClient(subscriber.WebhookId, subscriber.WebhookToken))
									{
										await vc.SendMessageAsync("", embeds: new Embed[] { embed });
									}
								}
							}
						}
						else
							throw new Exception("Webhook rate limit exceeded!");
					}

					// Cache
					data.Notifications = notifications.Take(11);
					if (!db.Update(data))
						throw new Exception("Failed to update cache!");

					// Sleep
					var delay = (int)(_sleepTime - watch.ElapsedMilliseconds);
					if (delay < 0)
						throw new Exception($"Task took too long ({delay}ms)");
					await Task.Delay(delay);
				}
			}
			catch (Exception ex)
			{
				await LogException(ex);
			}
		}

		public async Task<Embed> CreateEmbedAsync(SpeedrunNotification nf)
		{
			var author = nf.Text.Split(' ')[0];
			var title = "Latest Notification";
			var category = string.Empty;
			var description = nf.Text;

			// Old code here, I hope this won't throw an exception :>
			switch (nf.Item.Rel)
			{
				case "post":
					title = "Thread Response";
					category = nf.Text.Substring(nf.Text.IndexOf(" in the ") + " in the ".Length, nf.Text.IndexOf(" forum.") - nf.Text.IndexOf(" in the ") - " in the ".Length);
					description = $"*[{nf.Text.Substring(nf.Text.IndexOf("'") + 1, nf.Text.LastIndexOf("'") - nf.Text.IndexOf("'") - 1)}]({nf.Item.Uri.ToRawText()})* ({category.ToRawText()})";
					break;
				case "run":
					title = "Run Submission";
					category = nf.Text.Substring(nf.Text.IndexOf("beat the WR in ") + "beat the WR in ".Length, nf.Text.IndexOf(". The new WR is") - nf.Text.IndexOf("beat the WR in ") - "beat the WR in ".Length);
					description = $"New {(nf.Text.Contains(" beat the WR in ") ? "**World Record**" : "Personal Best")} in [{category.ToRawText()}]({nf.Item.Uri})\nwith a time of {nf.Text.Substring(nf.Text.LastIndexOf(". The new WR is ") + ". The new WR is ".Length)}.";
					break;
				case "game":
					// ???
					break;
				case "guide":
					title = "New Guide";
					// ???
					break;
				case "thread":		// Undocumented API
					title = "New Thread Post";
					category = nf.Text.Substring(nf.Text.LastIndexOf(" in the ") + " in the ".Length, nf.Text.IndexOf(" forum:") - nf.Text.IndexOf(" in the ") - "in the ".Length);
					description = $"*[{nf.Text.Substring(nf.Text.LastIndexOf(" forum: ") + " forum: ".Length)}]({nf.Item.Uri.ToRawText()})* ({category.ToRawText()})";
					break;
				case "moderator":   // Undocumented API
					title = "New Moderator";
					category = nf.Text.Substring(nf.Text.IndexOf("has been added to ") + "has been added to ".Length, nf.Text.IndexOf(" as a moderator.") - nf.Text.IndexOf("has been added to ") - "has been added to ".Length);
					description = $"{author.ToRawText()} is now a moderator for {category.ToRawText()}! :heart:";
					break;
				case "resource":    // Undocumented API
					category = nf.Text.Substring(nf.Text.IndexOf(" for ") + " for ".Length, nf.Text.LastIndexOf(" has") - nf.Text.IndexOf(" for ") - "for".Length);
					if (nf.Text.EndsWith("updated."))
					{
						title = "Updated Resource";
						description = $"The resource *{nf.Text.Substring(nf.Text.IndexOf("The tool resource '") + "The tool resource '".Length + 1, nf.Text.LastIndexOf("' for ") - nf.Text.IndexOf("The tool resource '") - "The tool resource '".Length - 1)}* has been updated for {category.ToRawText()}.";
					}
					else if (nf.Text.EndsWith("added."))
					{
						title = "New Resource";
						description = $"The resource *{nf.Text.Substring(nf.Text.IndexOf("A new tool resource, ") + "A new tool resource, ".Length + 1, nf.Text.LastIndexOf(", has been added to ") - nf.Text.IndexOf("A new tool resource, ") - "A new tool resource, ".Length - 1)}* has been added for {category.ToRawText()}.";
					}
					break;
			}

			var thumbnail = default(string);
			if (!string.IsNullOrEmpty(category))
			{
				var games = await _client.GetGamesAsync(category);
				var game = games?.FirstOrDefault();
				thumbnail = game.Assets.CoverMedium.Uri;
			}

			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = author,
					Url = $"https://www.speedrun.com/{author}",
					IconUrl = $"https://www.speedrun.com/themes/user/{author}/image.png"
				},
				Title = title,
				Url = nf.Item.Uri,
				Description = description,
				Color = new Color(229, 227, 87),
				Timestamp = DateTime.UtcNow,
				Footer = new EmbedFooterBuilder
				{
					Text = "speedrun.com",
					IconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/icons/speedruncom_icon.png"
				}
			};

			if (!string.IsNullOrEmpty(thumbnail))
				embed.WithThumbnailUrl(thumbnail);

			return embed.Build();
		}
	}
}