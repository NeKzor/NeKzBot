using System.Collections.Generic;
using NeKzBot.Services;

namespace NeKzBot.Data
{
	public class SourceCvarData
	{
		public CvarGameType Type { get; set; }
		public string Cvar { get; set; }
		public string Description { get; set; }
		public IEnumerable<string> Flags { get; set; }
	}
}