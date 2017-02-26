namespace NeKzBot
{
	public static class Program
	{
		public static void Main(string[] args)
            => new Bot().StartAsync()
						.GetAwaiter()
						.GetResult();
	}
}