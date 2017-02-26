using Newtonsoft.Json;

namespace NeKzBot.Classes.Discord
{
	[JsonObject("embed")]
	public class Embed
	{
		[JsonProperty("author")]
		public EmbedAuthor Author { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("description")]
		public string Description { get; set; }
		[JsonProperty("thumbnail")]
		public EmbedThumbnail Thumbnail { get; set; }
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("color")]
		public uint Color { get; set; }
		[JsonProperty("fields")]
		public EmbedField[] Fields { get; set; }
		[JsonProperty("image")]
		public EmbedImage Image { get; set; }
		[JsonProperty("timestamp")]
		public string Timestamp { get; set; }
		[JsonProperty("footer")]
		public EmbedFooter Footer { get; set; }
	}

	public class EmbedThumbnail
	{
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("procy_url")]
		public string ProxyUrl { get; set; }
		[JsonProperty("height")]
		public int Height { get; set; }
		[JsonProperty("width")]
		public int Width { get; set; }

		public EmbedThumbnail(string url)
		{
			Url = url;
		}
	}

	public class EmbedAuthor
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("icon_url")]
		public string IconUrl { get; set; }

		public EmbedAuthor(string name, string url = "", string iconurl = "")
		{
			Name = name;
			Url = url;
			IconUrl = iconurl;
		}
	}

	public class EmbedImage
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		public EmbedImage(string url)
		{
			Url = url;
		}
	}

	public class EmbedFooter
	{
		[JsonProperty("icon_url")]
		public string IconUrl { get; set; }
		[JsonProperty("text")]
		public string Text { get; set; }

		public EmbedFooter(string text = "", string iconurl = "")
		{
			IconUrl = iconurl;
			Text = text;
		}
	}

	public class EmbedField
	{
		[JsonProperty("inline")]
		public bool Inline { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("value")]
		public string Value { get; set; }

		public EmbedField(string name, string value, bool inline = false)
		{
			Inline = inline;
			Name = name;
			Value = value;
		}
	}
}
