using Newtonsoft.Json;

namespace NeKzBot.Extensions
{
	public sealed class CustomMessage
	{
		[JsonProperty("content")]
		public string Content { get; set; }
		[JsonProperty("nonce")]
		public ulong Nonce { get; set; }
		[JsonProperty("tts")]
		public bool AsTts { get; set; }
		[JsonProperty("file")]
		public string File { get; set; }
		[JsonProperty("embed")]
		public Embed EmbedObject { get; set; }

		public CustomMessage(Embed embed)
		{
			Content = string.Empty;
			EmbedObject = embed;
		}

		public CustomMessage(string content, Embed embed)
		{
			Content = content;
			EmbedObject = embed;
		}
	}
}
