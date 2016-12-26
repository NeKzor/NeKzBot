using System.Threading.Tasks;
using Discord.Audio;

namespace NeKzBot
{
	public class VoiceChannel
	{
		public static bool
			vcconnected = false,    // Only play when in VC
			isplaying = false,      // Prevent double playing
			shouldstop = false;     // Stop music, needed when playing long audio tracks

		public static async Task ConnectVC(Discord.Channel vChannel)
		{
			Logging.CON("Trying to join vc", System.ConsoleColor.DarkCyan);
			if (!vcconnected)
			{
				await NBot.dClient.GetService<AudioService>().Join(vChannel);
				vcconnected = true;
			}
		}

		public static async Task DisconnectVC(ulong serverID)
		{
			Logging.CON("Trying to leave vc", System.ConsoleColor.DarkCyan);
			if (vcconnected)
			{
				await NBot.dClient.GetServer(serverID).GetAudioClient().Disconnect();
				vcconnected = false;
			}
		}
	}
}