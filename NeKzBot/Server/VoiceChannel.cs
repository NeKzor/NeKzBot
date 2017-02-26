using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
//using NAudio.Wave;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Tasks
{
	public static class VoiceChannel
	{
		public static bool Connected { get; private set; } = false;		// Only play when in VC
		public static bool IsPlaying { get; private set; } = false;		// Prevent double playing
		public static bool ShouldStop { get; private set; } = false;	// Stop music, needed when playing long audio tracks

		public static async Task ConnectAsync(Channel channel)
		{
			await Logger.SendAsync("Joining Voice Channel", LogColor.Audio);
			if (!(Connected))
			{
				await Bot.Client.GetService<AudioService>().Join(channel);
				Connected = true;
			}
		}

		public static async Task DisconnectAsync(ulong id)
		{
			await Logger.SendAsync("Leaving Voice Channel", LogColor.Audio);
			if (Connected)
			{
				await Bot.Client.GetServer(id).GetAudioClient().Disconnect();
				Connected = false;
			}
		}

		public static async Task PlayWithFFmpegAsync(ulong id, Channel channel, string file)
		{
			await Logger.SendAsync("Playing Audio With FFmpeg", LogColor.Audio);
			if (IsPlaying)
				return;
			IsPlaying = true;

			var aClient = default(IAudioClient);
			var process = default(Process);
			try
			{
				aClient = Bot.Client.GetServer(id).GetAudioClient();
				process = Process.Start(new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = $"-i {Path.Combine(await Utils.GetPath(), Configuration.Default.AudioPath, file)} -f s16le -ar 48000 -ac 2 pipe:1",
					UseShellExecute = false,
					RedirectStandardOutput = true
				});
				await Task.Delay(1000);

				var buffer = new byte[3840];
				var byteCount = default(int);
				while (!(process.HasExited) && !(ShouldStop))
				{
					byteCount = process.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
					if (byteCount == 0)
						break;
					aClient.Send(buffer, 0, byteCount);
				}
			}
			catch
			{
				await Logger.SendAsync("Audio.PlayWithFFmpegAsync Error", LogColor.Error);
			}
			finally
			{
				if (!(process.HasExited))
					process.Close();
				IsPlaying = false;
				ShouldStop = false;
			}
		}

		public static Task StopAudio()
		{
			ShouldStop = (IsPlaying)
							? true
							: ShouldStop;
			return Task.FromResult(0);
		}

		#region UNUSED CODE
		//public static async Task PlayWithNAudioAsync(ulong id, Channel channel, string file)
		//{
		//	await Logger.SendAsync("Playing Audio With NAudio", LogColor.Audio);
		//	if (IsPlaying)
		//		return;
		//	IsPlaying = true;

		//	var aClient = default(IAudioClient);
		//	try
		//	{
		//		aClient = Bot.Client.GetServer(id).GetAudioClient();
		//		var OutFormat = new WaveFormat(48000, 16, Bot.Client.GetService<AudioService>().Config.Channels);
		//		using (var MP3Reader = new AudioFileReader(Path.Combine(await Utils.GetPath(), Configuration.Default.AudioPath, file)))
		//		using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat))
		//		{
		//			resampler.ResamplerQuality = 30;
		//			var blockSize = OutFormat.AverageBytesPerSecond / 50;
		//			var buffer = new byte[blockSize];
		//			var byteCount = default(int);
		//			while (((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) && !(ShouldStop))
		//			{
		//				if (byteCount < blockSize)
		//					for (int i = byteCount; i < blockSize; i++)
		//						buffer[i] = 0;
		//				aClient.Send(buffer, 0, blockSize);
		//			}
		//		}
		//	}
		//	catch
		//	{
		//		await Logger.SendAsync("Audio.PlayWithNAudioAsync Error", LogColor.Error);
		//	}
		//	IsPlaying = false;
		//	ShouldStop = false;
		//}
		#endregion
	}
}