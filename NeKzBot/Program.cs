namespace NeKzBot
{
	/// <summary>
	/// Project name:	NeKzBot
	/// Made by:		NeKz
	/// Start date:		03/12/2016
	/// Version:		1.0
	/// Release date:	26/12/2016
	/// GitHub repo:	http://github.com/NeKzor/NeKzBot
	/// Description:	Test the application here - https://discord.gg/rEazbJn
	/// </summary>
	public class Program
	{
		public static System.Diagnostics.Stopwatch uptimeWatch = new System.Diagnostics.Stopwatch();

		private static void Main()
		{
			uptimeWatch.Start();
			System.Console.BackgroundColor = System.ConsoleColor.Black;
			System.Console.ForegroundColor = System.ConsoleColor.DarkGreen;
			System.Console.Title = $"{Properties.Settings.Default.AppName} V{Properties.Settings.Default.AppVersion} - Discord Server Bot";

			try
			{
				NBot bot = new NBot();
			}
			catch (System.Exception ex)
			{
				Logging.CON($"what happened???\n{ex.ToString()}");
			}
		}

		// Uptime =/= connection time
		public static string GetUptime() => uptimeWatch.Elapsed.ToString("h\\h\\ m\\m\\ s\\s");
	}
}