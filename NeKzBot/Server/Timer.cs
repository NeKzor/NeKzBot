using System.Diagnostics;
using System.Threading.Tasks;
using NeKzBot.Internals;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Utilities;

namespace NeKzBot.Server
{
	public static class Timer
	{
		public static bool IsRunning { get; private set; } = false;
		public static InternalWatch Watch { get; } = new InternalWatch();
		private static Stopwatch _cacheWatch;
		private const uint _globalDelay = 10;

		// Timer
		public static async Task RunAsync()
		{
			await Logger.SendAsync("Timer Started", LogColor.Leaderboard);
			_cacheWatch = new Stopwatch();
			_cacheWatch.Start();
			IsRunning = true;
			try
			{
				for (;;)
				{
					var delay = (int)((_globalDelay * 60 * 1000) - await Watch.GetElapsedTime(debugmsg: "Timer.RunAsync Delay Took -> "));
					await Task.Delay((delay > 0) ? delay : 0);
					await Watch.RestartAsync();
					await Task.Factory.StartNew(async () => Bot.Client.SetGame(await Utils.RngAsync((await Data.Get<Simple>("games")).Value)));
					await Task.Factory.StartNew(async () => Bot.Client.SetStatus(await Utils.RngAsync(Data.BotStatus)));
				}
			}
			catch
			{
				await Logger.SendAsync("Timer.RunAsync Error", LogColor.Error);
			}
			IsRunning = false;
		}
	}
}