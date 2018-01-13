using System.Collections.Generic;
using LiteDB;
using Portal2Boards.Net.Entities;

namespace NeKzBot.Data
{
	public class Portal2CacheData
	{
		[BsonId]
		public string Id { get; set; }
		public IEnumerable<EntryData> Entries { get; set; }
	}
}