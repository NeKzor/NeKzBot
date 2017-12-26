using System.Collections.Generic;
using Portal2Boards.Net.Entities;

namespace NeKzBot.Data
{
	public class Portal2CacheData
	{
		public string Identifier { get; set; }
		public IEnumerable<EntryData> Entries { get; set; }
	}
}