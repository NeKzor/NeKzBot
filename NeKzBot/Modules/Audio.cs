using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Audio;
//using NAudio.Wave;
using NeKzBot.Server;

namespace NeKzBot.Modules
{
	public class Audio : VoiceChannel
	{
		public static async Task Init()
		{
			await Logging.CON("Initializing audio", System.ConsoleColor.DarkYellow);

			Bot.dClient.UsingAudio(x =>
			{
				x.Mode = AudioMode.Outgoing;
			});
		}

		public static async Task PlayWithFFmpeg(ulong serverID, Discord.Channel vChannel, string filePath)
		{
			await Logging.CON("Trying to play FFmpeg", System.ConsoleColor.DarkGreen);
			if (isplaying)
				return;
			isplaying = true;

			IAudioClient aClient = null;
			Process process = null;
			try
			{
				aClient = Bot.dClient.GetServer(serverID).GetAudioClient();
				process = Process.Start(new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = $"-i {Settings.Default.ApplicationPath + Settings.Default.AudioPath + filePath} -f s16le -ar 48000 -ac 2 pipe:1",
					UseShellExecute = false,
					RedirectStandardOutput = true
				});
				await Task.Delay(1000);

				byte[] buffer = new byte[3840];
				int byteCount;
				while (!process.HasExited && !shouldstop)
				{
					byteCount = process.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
					if (byteCount == 0)
						break;
					aClient.Send(buffer, 0, byteCount);
				}
			}
			catch
			{
				await Logging.CON("FFmpeg error", System.ConsoleColor.Red);
			}
			finally
			{
				if (!process.HasExited)
					process.Close();
				isplaying = false;
				shouldstop = false;
			}
		}

		#region UNUSED CODE FOR WINDOWS SYSETEMS
		//public static async Task PlayWithNAudio(ulong serverID, Discord.Channel vChannel, string filePath)
		//{
		//	await Logging.CON("Trying to play naudio", System.ConsoleColor.DarkGreen);
		//	if (isplaying)
		//		return;
		//	isplaying = true;

		//	IAudioClient aClient;
		//	try
		//	{
		//		aClient = Bot.dClient.GetServer(serverID).GetAudioClient();
		//		var OutFormat = new WaveFormat(48000, 16, Bot.dClient.GetService<AudioService>().Config.Channels);
		//		using (var MP3Reader = new AudioFileReader(Resources.Utils.GetPath() + Settings.Default.AudioPath + filePath))
		//		using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat))
		//		{
		//			resampler.ResamplerQuality = 30;
		//			var blockSize = OutFormat.AverageBytesPerSecond / 50;
		//			var buffer = new byte[blockSize];
		//			int byteCount;
		//			while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && !shouldstop)
		//			{
		//				if (byteCount < blockSize)
		//					for (int i = byteCount; i < blockSize; i++)
		//						buffer[i] = 0;
		//				aClient.Send(buffer, 0, blockSize);
		//			}
		//		}
		//		await Task.Delay(100);
		//	}
		//	catch
		//	{
		//		await Logging.CON("NAudio error", System.ConsoleColor.Red);
		//	}
		//	isplaying = false;
		//	shouldstop = false;
		//}
		#endregion
	}
}