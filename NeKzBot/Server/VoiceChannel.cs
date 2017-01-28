using System.Threading.Tasks;
using Discord.Audio;

namespace NeKzBot.Server
{
	public class VoiceChannel
	{
		public static bool vcconnected = false;		// Only play when in VC
		public static bool isplaying = false;		// Prevent double playing
		public static bool shouldstop = false;		// Stop music, needed when playing long audio tracks

		public static async Task ConnectVC(Discord.Channel vChannel)
		{
			await Logging.CON("Trying to join vc", System.ConsoleColor.DarkGreen);
			if (!vcconnected)
			{
				await Bot.dClient.GetService<AudioService>().Join(vChannel);
				vcconnected = true;
			}
		}

		public static async Task DisconnectVC(ulong serverID)
		{
			await Logging.CON("Trying to leave vc", System.ConsoleColor.DarkCyan);
			if (vcconnected)
			{
				await Bot.dClient.GetServer(serverID).GetAudioClient().Disconnect();
				vcconnected = false;
			}
		}
	}
}