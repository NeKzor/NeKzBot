using System.Threading.Tasks;
using Discord.Audio;

namespace NeKzBot.Server
{
	public static class Audio
	{
		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Audio", LogColor.Init);
			Bot.Client.UsingAudio(a => a.Mode = AudioMode.Outgoing);
		}
	}
}