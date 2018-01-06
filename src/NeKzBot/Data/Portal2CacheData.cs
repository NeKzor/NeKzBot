using System.Collections.Generic;
using LiteDB;
using Portal2Boards.Net.Entities;

namespace NeKzBot.Data
{
	public class Portal2CacheData
	{
		[BsonId]
		public string Identifier { get; set; }
		public IEnumerable<EntryData> Entries { get; set; }
	}
}