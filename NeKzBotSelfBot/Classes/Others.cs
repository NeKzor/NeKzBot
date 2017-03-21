using System;

namespace NeKzBot.Classes
{
	public class WebHeader
	{
		private Tuple<string, string> _header { get; }

		public Tuple<string, string> GetHeader()
			=> _header;

		public WebHeader(string h1, string h2)
			=> _header = new Tuple<string, string>(h1, h2);
	}
}