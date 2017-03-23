using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Tasks.Leaderboard;
using NeKzBot.Tasks.NonModules;
using NeKzBot.Tasks.Speedrun;
using NeKzBot.Utilities;

namespace NeKzBot.Server
{
	// TODO: fix or delete
	public static class Events
	{
		private const int _ratelimit = 333;

		public static async Task OnReceiveAsync(MessageEventArgs e)
		{
			try
			{
				if (!((bool)(e.User?.IsBot))
				&& ((await Data.Get<Simple>("vips")).Value.Contains(e.Server?.Id.ToString())))
				{
					if (!(await Steam.CheckWorkshopAsync(e)))
						if (!(await DemoTools.CheckTickCalculatorAsync(e)))
							if (!(await DemoTools.CheckStartdemosGeneratorAsync(e)))
								await AutoDownloader.CheckDropboxAsync(e);
				}
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnReceiveAsync Error", ex);
			}
		}

		public static async Task OnJoinedServerAsync(ServerEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{Bot.Client.CurrentUser?.Name}#{Bot.Client.CurrentUser?.Discriminator.ToString("D4")} joined a server.\nName • {e.Server?.Name} (ID {e.Server?.Id})\nOwner • {(e.Server?.Owner?.Name != null ? $"*{e.Server?.Owner.Name}#{e.Server?.Owner.Discriminator.ToString("D4")}*" : "*not found*")} (ID {e.Server?.Owner.Id})");
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnJoinedServerAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnLeftServerAsync(ServerEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{Bot.Client.CurrentUser?.Name}#{Bot.Client.CurrentUser?.Discriminator.ToString("D4")} left a server.\nName • {e.Server?.Name} (ID {e.Server?.Id})\nOwner • {(e.Server?.Owner?.Name != null ? $"*{e.Server?.Owner.Name}#{e.Server?.Owner.Discriminator.ToString("D4")}*" : "*not found*")} (ID {e.Server?.Owner.Id})");
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnLeftServerAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserBannedAsync(UserEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User?.Name}#{e.User?.Discriminator.ToString("D4")} (ID {e.User?.Id}) has been banned from the server.");
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnUserBannedAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserUnbannedAsync(UserEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User?.Name}#{e.User?.Discriminator.ToString("D4")} (ID {e.User?.Id}) has been unbanned from the server.");
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnUserUnbannedAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserJoinedAsync(UserEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User?.Name}#{e.User?.Discriminator.ToString("D4")} (ID {e.User?.Id}) joined the server.");

				// Give new user a world record role if he has one on speedrun.com or on board.iverb.me
				if (!((bool)(e?.User?.IsBot))
				&& (e?.Server?.Id == Credentials.Default.DiscordMainServerId))
				{
					if (!((bool)(e.Server?.FindRoles(Configuration.Default.WorldRecordRoleName).Any())))
						await e.Server?.CreateRole(Configuration.Default.WorldRecordRoleName, color: new Color(34, 126, 230));
					var role = e.Server?.FindRoles(Configuration.Default.WorldRecordRoleName)?.FirstOrDefault();
					if (role != null)
					{
						// Not sure how that can happen since the user joins for the first time but whatever
						if ((bool)e.User?.HasRole(role))
							return;

						if (await SpeedrunCom.PlayerHasWorldRecord(e.User?.Name) == "Yes.")
							await e.User?.AddRoles(role);
						else if (await Portal2.CheckIfUserHasWorldRecordAsync($"https://board.iverb.me/changelog?boardName={e.User?.Name}&wr=1"))
							await e.User?.AddRoles(role);
						// Nickname should also not exist, what am I doing...
						else if ((e.User?.Nickname != null)
						&& (e.User?.Name != e.User?.Nickname))
						{
							if (await SpeedrunCom.PlayerHasWorldRecord(e.User?.Nickname) == "Yes.")
								await e.User?.AddRoles(role);
							else if (await Portal2.CheckIfUserHasWorldRecordAsync($"https://board.iverb.me/changelog?boardName={e.User?.Nickname}&wr=1"))
								await e.User?.AddRoles(role);
							else
								return;
						}
						else
							return;
						await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User?.Name}#{e.User?.Discriminator.ToString("D4")} (ID {e.User?.Id}) has earned the _{Configuration.Default.WorldRecordRoleName}_ role.");
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnUserJoinedAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserLeftAsync(UserEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User?.Name}#{e.User?.Discriminator.ToString("D4")} (ID {e.User?.Id}) left or was kicked from the server.");
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnUserLeftAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserUpdatedAsync(UserUpdatedEventArgs e)
		{
			try
			{
				if (!(e.After.IsBot)
				&& (e.Server?.Id == Credentials.Default.DiscordMainServerId))
				{
					if (e.After.CurrentGame == null)
						return;

					var gameafter = e.After.CurrentGame.Value;
					if (gameafter.Type == GameType.Twitch)
					{
						var channel = gameafter.Url.Substring(gameafter.Url.LastIndexOf('/') + 1);
						// Add streamer
						if ((await Data.Get("streamers") is IData data)
						&& (data.ReadWriteAllowed)
						&& !(await Utils.SearchCollection((data.Memory as Simple).Value, channel)))
						{
							if (!((bool)(e.Server?.FindRoles(Configuration.Default.StreamingRoleName).Any())))
								await e.Server?.CreateRole(Configuration.Default.StreamingRoleName, color: Data.TwitchColor);
							var role = e.Server?.FindRoles(Configuration.Default.StreamingRoleName)?.FirstOrDefault();
							if (role == null)
								return;
							if (e.After.HasRole(role))
								return;
							await e.After.AddRoles(role);
							// Not adding this to the streaming list, somebody could have abused this by changing his game url multiple times
							//var result = await Utils.AddDataAsync(index, channel);
							await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.After.Name}#{e.After.Discriminator.ToString("D4")} (ID {e.After.Id}) has  earned the _{Configuration.Default.StreamingRoleName}_ role.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnUserUpdatedAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}
	}
}