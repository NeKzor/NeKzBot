using System.Net.Http;

namespace NeKzBot.Extensions
{
	public static class CustomRequest
	{
		public static Request SendMessage(ulong id)
			=> new Request(HttpMethod.Post, Endpoint.CreateMessage(id));
		public static Request AddReaction(ulong channel, ulong message, string emoji)
			=> new Request(HttpMethod.Put, Endpoint.CreateReaction(channel, message, emoji));
		public static Request PinMessage(ulong channel, ulong message)
			=> new Request(HttpMethod.Put, Endpoint.AddPinnedChannelMessage(channel, message));
	}
}