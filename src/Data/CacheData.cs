using System;

namespace NeKzBot.Data
{
	public class CacheData
	{
		public string Identifier { get; set; }
		public DateTime CreatedAt { get; set; }
		public object Value { get; set; }

		public CacheData() => CreatedAt = DateTime.UtcNow;
	}
}