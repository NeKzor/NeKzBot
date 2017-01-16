using System;
using System.Threading.Tasks;
using NeKzBot.Server;

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
					var channel = Utils.GetChannel(Settings.Default.NotificationChannelName);

					// Reserve cache memory
					var cachekey = "autonf";
					Caching.CFile.AddKey(cachekey);

					for (;;)
					{
						Logging.CON("AutoNotification checking", ConsoleColor.DarkRed);
						
						// Get cache
						var cache = Caching.CFile.GetFile(cachekey);

						// Download data
						var notification = await GetNotificationUpdate();

						// Ignore data we don't want and errors
						if (!string.IsNullOrEmpty(notification))
						{
							// Only update if new
							if (notification != cache)
							{
								// Save cache
								Logging.CON($"CACHING NEW DATA {Utils.StringInBytes(notification)} bytes");
								Caching.CFile.Save(cachekey, notification);

								// Send update
								await channel?.SendMessage(notification);
							}
						}
						await Task.Delay(60000);   // Check every minutes (max speed request is 100 per min tho)
					}
				}
				catch
				{
					Logging.CON("AutoNotification error");
				}
			}
		}
	}
}