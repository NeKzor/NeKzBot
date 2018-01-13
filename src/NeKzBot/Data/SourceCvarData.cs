using System.Collections.Generic;
using LiteDB;
using NeKzBot.Services;

namespace NeKzBot.Data
{
	public class SourceCvarData
	{
		[BsonId(true)]
		public int Id { get; set; }
		public string Name { get; set; }
		public string DefaultValue { get;set; }
		public IEnumerable<string> Flags { get; set; }
		public string HelpText { get; set; }
		public CvarGameType Type { get; set; }
	}
}