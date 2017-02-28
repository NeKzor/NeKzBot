using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Resources;
using NeKzBot.Tasks.Leaderboard;
using NeKzBot.Tasks.NonModules;
using NeKzBot.Tasks.Speedrun;

namespace NeKzBot.Server
{
	public static class Events
	{
		private const int _ratelimit = 333;

		public static async Task OnReceiveAsync(MessageEventArgs msg)
		{
			if (!(msg.User.IsBot))
			{
				try
				{
					if (!(await Steam.CheckWorkshopAsync(msg)))
						if (!(await DemoTools.CheckTickCalculatorAsync(msg)))
							if (!(await DemoTools.CheckStartdemosGeneratorAsync(msg)))
								// Server exclusive
								if (msg.Server?.Id == Credentials.Default.DiscordMainServerId)
									await AutoDownloader.CheckDropboxAsync(msg);
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("MessageReceived Error", e);
				}
			}
		}

		public static async Task OnJoinedServerAsync(ServerEventArgs e)
		{
			await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{Bot.Client.CurrentUser.Name}#{Bot.Client.CurrentUser.Discriminator.ToString("D4")} joined a server.\nName • {e.Server.Name} (ID {e.Server.Id})\nOwner • {(e.Server.Owner?.Name != null ? $"*{e.Server.Owner.Name}#{e.Server.Owner.Discriminator.ToString("D4")}*" : "*not found*")} (ID {e.Server.Owner.Id})");
			await Task.Delay(_ratelimit);
		}

		public static async Task OnLeftServerAsync(ServerEventArgs e)
		{
			await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{Bot.Client.CurrentUser.Name}#{Bot.Client.CurrentUser.Discriminator.ToString("D4")} left a server.\nName • {e.Server.Name} (ID {e.Server.Id})\nOwner • {(e.Server.Owner?.Name != null ? $"*{e.Server.Owner.Name}#{e.Server.Owner.Discriminator.ToString("D4")}*" : "*not found*")} (ID {e.Server.Owner.Id})");
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserBannedAsync(UserEventArgs e)
		{
			await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User.Name}#{e.User.Discriminator.ToString("D4")} (ID {e.User.Id}) has been banned from the server.");
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserUnbannedAsync(UserEventArgs e)
		{
			await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User.Name}#{e.User.Discriminator.ToString("D4")} (ID {e.User.Id}) has been unbanned from the server.");
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserJoinedAsync(UserEventArgs e)
		{
			await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User.Name}#{e.User.Discriminator.ToString("D4")} (ID {e.User.Id}) joined the server.");

			// Give new user a world record role if he has one on speedrun.com
			if (!(e.User.IsBot))
			{
				if (!(e.Server.FindRoles(Configuration.Default.WorldRecordRoleName).Any()))
					await e.Server.CreateRole(Configuration.Default.WorldRecordRoleName, color: new Color(34, 126, 230));
				var role = e.Server.FindRoles(Configuration.Default.WorldRecordRoleName)?.FirstOrDefault();
				if (role != null)
				{
					// Not sure how that can happen since the user joins for the first time but whatever
					if (e.User.HasRole(role))
						return;

					if (await SpeedrunCom.PlayerHasWorldRecord(e.User.Name) == "Yes.")
						await e.User.AddRoles(role);
					else if (await Portal2.CheckIfUserHasWorldRecordAsync($"https://board.iverb.me/changelog?boardName={e.User.Name}&wr=1"))
						await e.User.AddRoles(role);
					// Nickname should also not exist, what am I doing...
					else if ((e.User.Nickname != null)
					&& (e.User.Name != e.User.Nickname))
					{
						if (await SpeedrunCom.PlayerHasWorldRecord(e.User.Nickname) == "Yes.")
							await e.User.AddRoles(role);
						else if (await Portal2.CheckIfUserHasWorldRecordAsync($"https://board.iverb.me/changelog?boardName={e.User.Nickname}&wr=1"))
							await e.User.AddRoles(role);
						else
							return;
					}
					else
						return;
					await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User.Name}#{e.User.Discriminator.ToString("D4")} (ID {e.User.Id}) has earned the _{Configuration.Default.WorldRecordRoleName}_ role.");
				}
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserLeftAsync(UserEventArgs e)
		{
			await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.User.Name}#{e.User.Discriminator.ToString("D4")} (ID {e.User.Id}) left or was kicked from the server.");
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserUpdatedAsync(UserUpdatedEventArgs e)
		{
			if (!(e.After.IsBot))
			{
				if (e.After.CurrentGame == null)
					return;

				var gameafter = e.After.CurrentGame.Value;
				if (gameafter.Type == GameType.Twitch)
				{
					var channel = gameafter.Url.Substring(gameafter.Url.LastIndexOf('/') + 1, gameafter.Url.Length - gameafter.Url.LastIndexOf('/') - 1);
					// Add streamer
					if ((await Data.DataExists("twitch", out var index))
					&& (Data.Manager[index].ReadingAllowed)
					&& (Data.Manager[index].WrittingAllowed)
					&& !(await Utils.SearchArray(Data.Manager[index].Data as string[], channel)))
					{
						if (!(e.Server.FindRoles(Configuration.Default.StreamingRoleName).Any()))
							await e.Server.CreateRole(Configuration.Default.StreamingRoleName, color: Data.TwitchColor);
						var role = e.Server.FindRoles(Configuration.Default.StreamingRoleName)?.FirstOrDefault();
						if (role == null)
							return;
						if (e.After.HasRole(role))
							return;
						await e.After.AddRoles(role);
						// Not adding this to the streaming list, somebody could have abused this by changing his game url multiple times
						//var result = await Utils.AddDataAsync(index, channel);
						await (await Utils.FindTextChannelByName(Configuration.Default.LogChannelName, e.Server))?.SendMessage($"**{await Utils.GetLocalTime()}**\n{e.After.Name}#{e.After.Discriminator.ToString("D4")} (ID {e.After.Id}) has  earned the _{Configuration.Default.StreamingRoleName}_ role.");
					}
				}
			}
			await Task.Delay(_ratelimit);
		}
	}
}