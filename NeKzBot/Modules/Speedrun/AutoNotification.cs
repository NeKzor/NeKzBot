using System;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Properties;

namespace NeKzBot
{
	public partial class SpeedrunCom
	{
		internal class AutoNotification
		{
			public static async Task Start(int clientdelay = 8000)
			{
				try
				{
					// Wait some time till speedrun client connected to the API
					await Task.Delay(clientdelay);

					Logging.CON("AutoNotification started", ConsoleColor.DarkRed);

					// Find channel to send to
					var channel = GetChannelByName();

					// Reserve cache memory
					var cachekey = "autonf";
					Caching.CFile.AddKey(cachekey);

					for (;;)
					{
						Logging.CON("AutoNotification checking", ConsoleColor.DarkRed);
						
						// Get cache
						var cache = Caching.CFile.GetFile(cachekey);

						// Download data
						var notification = GetLastNotification(update: true);

						// Only update if new
						if (notification != cache)
						{
							// Save cache
							Logging.CON($"CACHING NEW DATA {Utils.StringInBytes(notification)} bytes");
							Caching.CFile.Save(cachekey, notification);

							// Send update
							await channel.SendMessage(notification);
						}
						await Task.Delay(60000);   // Check every minutes (max speed request is 100 per min tho)
					}
				}
				catch
				{
					Logging.CON("AutoNotification error");
				}
			}

			// Get default notfications update channel
			private static Discord.Channel GetChannelByName(string serverName = null, string channelName = null)
			{
				serverName = serverName ?? Settings.Default.ServerName;
				channelName = channelName ?? Settings.Default.NotificationChannelName;
				return NBot.dClient?.FindServers(serverName)?.First().FindChannels(channelName, Discord.ChannelType.Text, true)?.First() ?? null;
			}
		}
	}
}