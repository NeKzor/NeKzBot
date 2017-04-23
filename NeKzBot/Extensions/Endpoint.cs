namespace NeKzBot.Extensions
{
	public static class Endpoint
	{
		public static string CreateMessage(ulong id)
			=> $"channels/{id}/messages";
		public static string CreateReaction(ulong channel, ulong message, string emoji)
			=> $"/channels/{channel}/messages/{message}/reactions/{emoji}/@me";
		public static string AddPinnedChannelMessage(ulong channel, ulong message)
			=> $"/channels/{channel}/pins/{message}";
	}
}