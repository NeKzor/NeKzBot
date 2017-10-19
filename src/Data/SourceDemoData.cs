using SourceDemoParser;

namespace NeKzBot.Data
{
	public class SourceDemoData
	{
		public ulong Id { get; set; }
		public string DownloadUrl { get; set; }
		public SourceDemo Demo { get; set; }
	}
}