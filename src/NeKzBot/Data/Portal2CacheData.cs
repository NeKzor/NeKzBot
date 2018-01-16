using System.Collections.Generic;
using LiteDB;

namespace NeKzBot.Data
{
	public class Portal2CacheData
	{
		[BsonId(true)]
		public int Id { get; set; }
		public IEnumerable<uint> EntryIds { get; set; }

		public Portal2CacheData()
			=> EntryIds = new List<uint>();
	}
}