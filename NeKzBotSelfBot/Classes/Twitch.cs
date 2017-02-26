namespace NeKzBot.Classes
{
	public sealed class TwitchStream
	{
		public string ChannelName { get; set; }
		public string GameName { get; set; }
		public string StreamTitle { get; set; }
		public string StreamLink { get; set; }
		public string AvatarLink { get; set; }
		public string PreviewLink { get; set; }
		public uint ChannelViewers { get; set; }
	}
}