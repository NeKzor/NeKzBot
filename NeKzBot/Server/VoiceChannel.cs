using System.Threading.Tasks;
using Discord.Audio;

namespace NeKzBot
{
	public class VoiceChannel : NBot
	{
		public static bool vcconnected = false;		// Only play when in VC
		public static bool isplaying = false;		// Prevent double playing
		public static bool shouldstop = false;		// Stop music, needed when playing long audio tracks

		public static async Task ConnectVC(Discord.Channel vChannel)
		{
			Logging.CON("Trying to join vc", System.ConsoleColor.DarkGreen);
			if (!vcconnected)
			{
				await dClient.GetService<AudioService>().Join(vChannel);
				vcconnected = true;
			}
		}

		public static async Task DisconnectVC(ulong serverID)
		{
			Logging.CON("Trying to leave vc", System.ConsoleColor.DarkCyan);
			if (vcconnected)
			{
				await dClient.GetServer(serverID).GetAudioClient().Disconnect();
				vcconnected = false;
			}
		}
	}
}