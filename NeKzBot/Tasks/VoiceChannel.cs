using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using NeKzBot.Classes;
using NeKzBot.Internals;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Tasks
{
	public static class VoiceChannel
	{
		private static readonly ConcurrentDictionary<ulong, Internals.InternalAudio> _audioClients = new ConcurrentDictionary<ulong, Internals.InternalAudio>();

		public static async Task<string> ConnectAsync(Discord.Server guild, Channel vchannel)
		{
			if (_audioClients.TryGetValue(guild.Id, out var audio))
			{
				if (!(audio.Connected))
				{
					await Bot.Client.GetService<AudioService>().Join(vchannel);
					audio.Connected = true;
					if (_audioClients.TryUpdate(guild.Id, audio, audio))
						return AudioError.None;

					await Logger.SendAsync($"Joined Voice Chanel ({guild.Name})(ID {guild.Id})", LogColor.Audio);
					return AudioError.Generic;
				}
				return AudioError.AlreadyConnected;
			}
			if (_audioClients.TryAdd(guild.Id, new Internals.InternalAudio(await Bot.Client.GetService<AudioService>().Join(vchannel)) { Connected = true }))
				return AudioError.None;
			return AudioError.Generic;
		}

		public static async Task<bool> DisconnectAsync(Discord.Server guild)
		{
			await Logger.SendAsync("Leaving Voice Channel", LogColor.Audio);
			if (_audioClients.TryGetValue(guild.Id, out var audio))
			{
				await audio.AudioClient.Disconnect();
				await Logger.SendAsync($"Left Voice Chanel ({guild.Name})(ID {guild.Id})", LogColor.Audio);
				return _audioClients.TryRemove(guild.Id, out var _);
			}
			return false;
		}

		public static async Task<string> PlayAsync(ulong guildid, string file)
		{
			// Get audio client if there's one (only possible if bot connected to a channel)
			var client = default(IAudioClient);
			if (!(_audioClients.TryGetValue(guildid, out var audio)))
				return AudioError.BotNotConneted;
			client = audio.AudioClient;

			// Change status into playing
			if (audio.IsPlaying)
				return AudioError.AlreadyPlaying;
			audio.IsPlaying = true;
			if (!(_audioClients.TryUpdate(guildid, audio, audio)))
				return AudioError.Generic;

			var process = default(Process);
			var path = (await Utils.IsLinux())
								   ? Path.Combine(await Utils.GetAppPath(), Configuration.Default.AudioPath, file)
								   : Path.Combine(Configuration.Default.AudioPath, file);
			if (!(File.Exists(path)))
				return AudioError.FileMissing;

			try
			{
				process = Process.Start(new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 -ac 2 pipe:1 -hide_banner -loglevel panic",
					UseShellExecute = false,
					RedirectStandardOutput = true
				});
				await Task.Delay(1000);

				var buffer = new byte[3840];
				var byteCount = default(int);
				while (!(process.HasExited) && !(audio.ShouldStop))
				{
					byteCount = process.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
					if (byteCount == 0)
						break;
					client.Send(buffer, 0, byteCount);

					// Check if user wants to stop audio stream, update the audio status
					if (_audioClients.TryGetValue(guildid, out var temp))
						audio = temp;
					else
						return AudioError.Generic;
				}
			}
			catch
			{
				await Logger.SendAsync("VoiceChannel.PlayAsync Error", LogColor.Error);
			}

			if (!(process.HasExited))
				process.Close();

			// Save
			audio.IsPlaying = false;
			audio.ShouldStop = false;
			if (!(_audioClients.TryUpdate(guildid, audio, audio)))
				return AudioError.Generic;
			return AudioError.None;
		}

		public static Task<bool> StopAudio(ulong guildid)
		{
			if (_audioClients.TryGetValue(guildid, out var audio))
			{
				audio.ShouldStop = (audio.IsPlaying)
										 ? true
										 : audio.ShouldStop;
				return Task.FromResult(_audioClients.TryUpdate(guildid, audio, audio));
			}
			return Task.FromResult(false);
		}

		public static Task<string> ConnectionCheck(Discord.Server guild, User requester)
		{
			var botchannel = guild.Users.FirstOrDefault(user => user.Id == Bot.Client.CurrentUser.Id)?.VoiceChannel;
			var userchannel = requester.VoiceChannel;

			if (botchannel == null)
				return Task.FromResult(AudioError.BotNotConneted);
			if (requester.Id == Credentials.Default.DiscordBotOwnerId)	// So I can play without connecting *evil laugh* (might be a bad idea)
				return Task.FromResult(AudioError.None);
			if (userchannel == null)
				return Task.FromResult(AudioError.InvalidRequest);
			if (botchannel != userchannel)
				return Task.FromResult(AudioError.WrongChannel);
			return Task.FromResult(AudioError.None);
		}
	}
}