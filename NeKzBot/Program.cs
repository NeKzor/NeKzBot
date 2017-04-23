using System;
using System.Threading.Tasks;

namespace NeKzBot
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			for (;;)
			{
				try
				{
					using (var nekzbot = new Bot())
						nekzbot.StartAsync().GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"\nNeKzBot Stopped Working\n{ex}");
				}
				Task.Delay(10 * 1000).GetAwaiter().GetResult();
				Console.WriteLine("RESTART\n");
			}
		}
	}
}