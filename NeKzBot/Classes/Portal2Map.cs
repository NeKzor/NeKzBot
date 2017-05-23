using Newtonsoft.Json;

namespace NeKzBot.Classes
{
	[JsonObject("map")]
	public sealed class Portal2Map
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("cm_name")]
		public string ChallengeModeName { get; set; }
		[JsonProperty("best_time_id")]
		public string BestTimeId { get; set; }
		[JsonProperty("best_portals_id")]
		public string BestPortalsId { get; set; }
		[JsonProperty("three_letter_code")]
		public string ThreeLetterCode { get; set; }
		[JsonProperty("elevator_timing")]
		public string ElevatorTiming { get; set; }
		[JsonProperty("map_filter")]
		public Portal2MapFilter Filter { get; set; }
	}

	public enum Portal2MapFilter
	{
		Any,
		SinglePlayer,
		MultiPlayer,
		Workshop        // Workshop has two other modes again but it means custom map for now
	}
}