using System;
using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules.Speedrun
{
	public partial class SpeedrunCom
	{
		internal class AutoNotification
		{
			public static bool isRunning = false;

			public static async Task Start(int clientdelay = 8000)
			{
				// Wait some time till speedrun client connected to the API
				await Task.Delay(clientdelay);
				await Logging.CON("AutoNotification started", ConsoleColor.DarkRed);
				isRunning = true;
				try
				{
					// Find channel to send to
					var channel = await Utils.GetChannel(Settings.Default.NotificationChannelName);

					// Reserve cache memory
					var cachekey = "autonf";
					await Caching.CFile.AddKey(cachekey);

					for (;;)
					{
						// Get cache
						var cache = await Caching.CFile.GetFile(cachekey);

						// Download data
						var notification = await GetNotificationUpdate();

						// Ignore data we don't want and errors
						if (!string.IsNullOrEmpty(notification))
						{
							// Only update if new
							if (notification != cache)
							{
								// Save cache
								await Logging.CON($"AutoNotification data cache -> {Utils.StringInBytes(notification)} bytes", ConsoleColor.Red);
								await Caching.CFile.Save(cachekey, notification);

								// Send update
								await channel?.SendMessage(notification);
							}
						}
						await Task.Delay(60000);   // Check every minutes (max speed request is 100 per min tho)
					}
				}
				catch
				{
					await Logging.CON("AutoNotification error", ConsoleColor.Red);
				}
				isRunning = false;
			}
		}
	}
}