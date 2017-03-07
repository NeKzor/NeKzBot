namespace NeKzBot
{
	internal static class Program
	{
		private static void Main(string[] args)
			=> new Bot().StartAsync()
						.GetAwaiter()
						.GetResult();
	}
}