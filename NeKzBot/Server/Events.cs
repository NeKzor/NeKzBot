using System;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Tasks.NonModules;
using NeKzBot.Utilities;

namespace NeKzBot.Server
{
	public static class Events
	{
		private const int _ratelimit = 333;

		public static async Task OnReceiveAsync(MessageEventArgs e)
		{
			try
			{
				if ((e.User?.IsBot == false)
				&& ((await Data.Get<Simple>("vips")).Value.Contains(e.Server?.Id.ToString())))
				{
					if (!(await Steam.CheckWorkshopAsync(e)))
						if (!(await DemoTools.CheckTickCalculatorAsync(e)))
							if (!(await DemoTools.CheckStartdemosGeneratorAsync(e)))
								if (!(await DemoParser.CheckForDemoFileAsync(e)))
									return;
				}
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnReceiveAsync Error", ex);
			}
		}

		public static async Task OnServerAvailableAsync(ServerEventArgs e)
			=> await Logger.SendAsync($"Server \"{e.Server.Name}\" is available");
		public static async Task OnServerUnavailableAsync(ServerEventArgs e)
			=> await Logger.SendAsync($"Server \"{e.Server.Name}\" is unavailable");
		public static async Task OnReadyAsync(EventArgs e)
			=> await Logger.SendAsync("Ready");

		public static async Task OnJoinedServerAsync(ServerEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName))?
					.SendMessage($"**{await Utils.GetLocalTimeAsync()}**" +
								 $"\n{Bot.Client.CurrentUser.Name}#{Bot.Client.CurrentUser.Discriminator.ToString("D4")} joined a server." +
								 $"\nName • {await Utils.AsRawText(e.Server?.Name) ?? "*Unavailable*"} (ID {e.Server?.Id ?? 0})" +
								 $"\nOwner • {await Utils.AsRawText(e.Server?.Owner?.Name) ?? "*Unavailable*"}#{e.Server?.Owner?.Discriminator.ToString("D4") ?? "0"}) (ID {e.Server?.Owner?.Id ?? 0})");
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
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName))?
					.SendMessage($"**{await Utils.GetLocalTimeAsync()}**" +
								 $"\n{Bot.Client.CurrentUser.Name}#{Bot.Client.CurrentUser.Discriminator.ToString("D4")} left a server." +
								 $"\nName • {await Utils.AsRawText(e.Server?.Name) ?? "*Unavailable*"} (ID {e.Server?.Id ?? 0})" +
								 $"\nOwner • {await Utils.AsRawText(e.Server?.Owner?.Name) ?? "*Unavailable*"}#{e.Server?.Owner?.Discriminator.ToString("D4") ?? "0"}) (ID {e.Server?.Owner?.Id ?? 0})");
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnLeftServerAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}

		public static async Task OnUserJoinedAsync(UserEventArgs e)
		{
			try
			{
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?
					.SendMessage($"**{await Utils.GetLocalTimeAsync()}**" +
								 $"\n{e.User?.Name ?? "*Unavailable*"}#{e.User?.Discriminator.ToString("D4") ?? "0"} (ID {e.User?.Id ?? 0}) joined the server.");
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
				await (await Utils.FindTextChannel(Configuration.Default.LogChannelName, e.Server))?
					.SendMessage($"**{await Utils.GetLocalTimeAsync()}**" +
								 $"\n{e.User?.Name ?? "*Unavailable*"}#{e.User?.Discriminator.ToString("D4") ?? "0"} (ID {e.User?.Id ?? 0}) left the server.");
			}
			catch (Exception ex)
			{
				await Logger.SendToChannelAsync("Events.OnUserLeftAsync Error", ex);
			}
			await Task.Delay(_ratelimit);
		}
	}
}