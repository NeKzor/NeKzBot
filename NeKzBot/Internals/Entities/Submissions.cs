using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeKzBot.Internals.Entities
{
	[JsonObject("submissions")]
	public sealed class Submissions : IMemory, IEnumerable<Submission>
	{
		[JsonProperty("players")]
		public List<Submission> Players { get; set; }
		[JsonProperty("is_available")]
		public bool IsAvailable { get; set; }
		[JsonProperty("rules")]
		public string Rules { get; set; }
		[JsonProperty("game_name")]
		public string GameName { get; set; }
		[JsonProperty("map_name")]
		public string MapName { get; set; }

		public Submissions()
			=> Players = new List<Submission>();
		public Submissions(List<Submission> subs)
			=> Players = subs;

		public IEnumerable<object> Values => Players;

		public IEnumerator<Submission> GetEnumerator()
			=> Players.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}

	[JsonObject("submission")]
	public sealed class Submission
	{
		[JsonProperty("user_name")]
		public string UserName { get; set; }
		[JsonProperty("user_disc")]
		public ushort UserDiscriminator { get; set; }
		[JsonProperty("user_id")]
		public ulong UserId { get; set; }
		[JsonProperty("date")]
		public DateTime SubmisionDate { get; set; }

		public Submission()
		{
		}
		public Submission(string name, ushort disc, ulong id, DateTime? date = null)
		{
			UserName = name;
			UserDiscriminator = disc;
			UserId = id;
			SubmisionDate = date ?? DateTime.UtcNow;
		}
	}
}