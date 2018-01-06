using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeKzBot.API
{
	[JsonObject]
	public class SpeedrunData<T>
	{
		[JsonProperty("data")]
		public IEnumerable<T> Data { get; set; }
		[JsonProperty("pagination")]
		public SpeedrunPagination Pagination { get; set; }
	}

	[JsonObject]
	public class SpeedrunLink
	{
		[JsonProperty("rel")]
		public string Rel { get; set; }
		[JsonProperty("uri")]
		public string Uri { get; set; }
	}

	[JsonObject]
	public class SpeedrunPagination
	{
		[JsonProperty("offset")]
		public uint Offset { get; set; }
		[JsonProperty("max")]
		public uint Max { get; set; }
		[JsonProperty("size")]
		public uint Size { get; set; }
		[JsonProperty("links")]
		public IEnumerable<SpeedrunLink> Links { get; set; }
	}

	[JsonObject]
	public class SpeedrunNotification
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("created")]
		public string Created { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; }
		[JsonProperty("text")]
		public string Text { get; set; }
		[JsonProperty("item")]
		public SpeedrunLink Item { get; set; }
		[JsonProperty("links")]
		public IEnumerable<SpeedrunLink> Links { get; set; }
	}
}