namespace NeKzBot
{
	internal static class Program
	{
		private static void Main()
			=> new Bot().RunAsync().GetAwaiter().GetResult();
	}
}