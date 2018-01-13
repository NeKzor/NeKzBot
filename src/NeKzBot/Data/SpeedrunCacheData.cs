using System.Collections.Generic;
using LiteDB;
using NeKzBot.API;

namespace NeKzBot.Data
{
	public class SpeedrunCacheData
	{
		[BsonId]
		public string Id { get; set; }
		public IEnumerable<SpeedrunNotification> Notifications { get; set; }
	}
}