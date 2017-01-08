using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Audio;
//using NAudio.Wave;

namespace NeKzBot
{
	public class Audio : VoiceChannel
	{
		public static void Init()
		{
			Logging.CON("Initializing audio", System.ConsoleColor.DarkYellow);

			dClient.UsingAudio(x =>
			{
				x.Mode = AudioMode.Outgoing;
			});
		}

		public static async Task PlayWithFFmpeg(ulong serverID, Discord.Channel vChannel, string filePath)
		{
			Logging.CON("Trying to play FFmpeg", System.ConsoleColor.DarkGreen);
			if (isplaying)
				return;
			isplaying = true;

			IAudioClient aClient = null;
			Process process = null;
			try
			{
				aClient = dClient.GetServer(serverID).GetAudioClient();
				process = Process.Start(new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = $"-i {Properties.Settings.Default.ApplicationPath + Properties.Settings.Default.AudioPath + filePath} -f s16le -ar 48000 -ac 2 pipe:1",
					UseShellExecute = false,
					RedirectStandardOutput = true
				});
				await Task.Delay(2000);

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
				//dClient.Log.Info($"VC {vChannel.Name}", "FFmpeg ERROR", null);
				Logging.CON("FFmpeg error");
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
		//	Logging.CON("Trying to play naudio");
		//	if (isplaying)
		//		return;
		//	isplaying = true;

		//	IAudioClient aClient;
		//	try
		//	{
		//		aClient = dClient.GetServer(serverID).GetAudioClient();
		//		var OutFormat = new WaveFormat(48000, 16, dClient.GetService<AudioService>().Config.Channels);
		//		using (var MP3Reader = new AudioFileReader(Properties.Settings.Default.ApplicationPath + Properties.Settings.Default.AudioPath + filePath))
		//		using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat))
		//		{
		//			resampler.ResamplerQuality = 30;
		//			int blockSize = OutFormat.AverageBytesPerSecond / 50;
		//			byte[] buffer = new byte[blockSize];
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
		//		//dClient.Log.Info($"VC {vChannel.Name}", "NAudio ERROR", null);
		//		//Logging.CON("NAudio error");
		//	}
		//	isplaying = false;
		//	shouldstop = false;
		//}
		#endregion
	}
}