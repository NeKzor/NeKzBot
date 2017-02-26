using System.Threading.Tasks;
using NeKzBot.Tasks;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules
{
	public class Sound : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Sound Commands", LogColor.Init);
			await GetRandomYanniSound("yanni");
			await GetRandomPortal2Sound("p2");
			await Utils.CommandCreator(() => PlaySoundInVC(Utils.CCGroup, Utils.CCIndex), 0, Data.SoundNames, true, Data.AudioAliases);
		}

		public static Task PlaySoundInVC(string s, int i)
		{
			CService.CreateGroup(s, GBuilder =>
			{
				GBuilder.CreateCommand(Data.SoundNames[i, 0])
						.Description(Data.SoundNames[i, 1])
						.AddCheck(Permissions.MainServerOnly)
						.Do(async e =>
						{
							if (VoiceChannel.Connected)
							{
								if (!(VoiceChannel.IsPlaying))
									await VoiceChannel.PlayWithFFmpegAsync(e.Server.Id, e.User.VoiceChannel, Data.SoundNames[i, 2]);
								else
									await e.Channel.SendMessage($"Bot is already playing sound. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} {Data.SoundNames[i, 0]}`");
							}
							else
								await e.Channel.SendMessage($"Bot isn't VC connected. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} vc`");
						});
			});
			return Task.FromResult(0);
		}

		public static Task GetRandomYanniSound(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c}` plays a random sound from the living meme.\n• Only works when in VC.")
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						if (VoiceChannel.Connected)
						{
							if (!(VoiceChannel.IsPlaying))
								await VoiceChannel.PlayWithFFmpegAsync(e.Server.Id, e.User.VoiceChannel, Data.SoundNames[await Utils.RNG(24, 31), 2]); // Array range of yanni sounds
							else
								await e.Channel.SendMessage($"Bot is already playing sound. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} {Data.SoundNames[await Utils.RNG(24, 31), 0]}`");
						}
						else
							await e.Channel.SendMessage($"Bot isn't VC connected. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} vc`");
					});
			return Task.FromResult(0);
		}

		public static Task GetRandomPortal2Sound(string c)
		{
			CService.CreateCommand(c)
					.Description($"• `{Configuration.Default.PrefixCmd + c}` plays a random challenge mode sound.\n• Only works when in VC.")
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						if (VoiceChannel.Connected)
						{
							if (!(VoiceChannel.IsPlaying))
								await VoiceChannel.PlayWithFFmpegAsync(e.Server.Id, e.User.VoiceChannel, Data.SoundNames[await Utils.RNG(0, 24), 2]); // Array range of P2 sounds
							else
								await e.Channel.SendMessage($"Bot is already playing sound. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} {Data.SoundNames[await Utils.RNG(0, 24), 0]}`");
						}
						else
							await e.Channel.SendMessage($"Bot isn't VC connected. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} vc`");
					});
			return Task.FromResult(0);
		}
	}
}