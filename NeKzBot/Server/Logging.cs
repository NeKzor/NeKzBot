using System;

namespace NeKzBot
{
	public class Logging
	{
		public static void Init()
		{
			CON("Initializing logging", ConsoleColor.DarkYellow);

			NBot.dClient.MessageReceived += (s, e) => {
				if (!e.Message.IsAuthor)
					NBot.dClient.Log.Info($"USER : {e.User.ToString()}", e.Message.Text, null);
				else
					NBot.dClient.Log.Info($"BOT : {e.User.ToString()}", e.Message.Text, null);
			};
		}

		public static void CON(object sender, Discord.LogMessageEventArgs e)
		{
			Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} @ SOCKET : {e.Severity} : {e.Source} : {e.Message}");
		}

		public static void CON(string s, ConsoleColor c = ConsoleColor.Red, bool toupper = true)
		{
			Console.ForegroundColor = c;
			Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss @ ROOT : ")}{string.Format("{0:N2}s : ", Program.uptimeWatch.Elapsed.TotalSeconds)}{ Utils.UpperString(s, toupper)}");
			Console.ResetColor();
		}

		public static void CON(ConsoleColor c = ConsoleColor.Red, bool toupper = true, params string[] s)
		{
			Console.ForegroundColor = c;
			foreach (var item in s)
				Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss @ ROOT : ")}{string.Format("{0:N2}s : ", Program.uptimeWatch.Elapsed.TotalSeconds)}{Utils.UpperString(item, toupper)}");
			Console.ResetColor();
		}
	}
}