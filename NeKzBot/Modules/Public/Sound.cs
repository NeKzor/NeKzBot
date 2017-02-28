using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;

namespace NeKzBot.Modules.Public
{
	public class Sound : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Sound Module", LogColor.Init);
			await GetRandomYanniSound("yanni");
			await GetRandomPortal2Sound("p2");
			await Utils.CommandBuilder(() => PlaySoundInVoiceChannel(Utils.CBuilderGroup, Utils.CBuilderIndex), 0, Data.SoundNames, true, Data.AudioAliases);
		}

		public static Task PlaySoundInVoiceChannel(string s, int i)
		{
			CService.CreateGroup(s, GBuilder =>
			{
				GBuilder.CreateCommand(Data.SoundNames[i, 0])
						.Description(Data.SoundNames[i, 1])
						.AddCheck(Permissions.MainServerOnly)
						.Do(async e =>
						{
							var channel = e.Server.Users.FirstOrDefault(user => user.Id == Bot.Client.CurrentUser.Id)?.VoiceChannel;
							if ((VoiceChannel.Connected)
							&& (channel != null))
							{
								if (e.User.VoiceChannel == null)
									await e.Channel.SendMessage("You have to connect to a voice channel.");
								else if (e.User.VoiceChannel == channel)
								{
									if (!(VoiceChannel.IsPlaying))
										await VoiceChannel.PlayWithFFmpegAsync(e.Server.Id, e.User.VoiceChannel, Data.SoundNames[i, 2]);
									else
										await e.Channel.SendMessage($"Already playing. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} stop`.");
								}
								else
									await e.Channel.SendMessage("You aren't connected to the right voice channel.");
							}
							else
								await e.Channel.SendMessage($"Not VC connected. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} vc`.");
						});
			});
			return Task.FromResult(0);
		}

		public static Task GetRandomYanniSound(string c)
		{
			CService.CreateCommand(c)
					.Description("Plays a random sound from the living meme. Only works in voice channels.")
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						var channel = e.Server.Users.FirstOrDefault(user => user.Id == Bot.Client.CurrentUser.Id)?.VoiceChannel;
						if ((VoiceChannel.Connected)
						&& (channel != null))
						{
							if (e.User.VoiceChannel == null)
								await e.Channel.SendMessage("You have to connect to a voice channel.");
							else if (e.User.VoiceChannel == channel)
							{
								if (!(VoiceChannel.IsPlaying))
									await VoiceChannel.PlayWithFFmpegAsync(e.Server.Id, e.User.VoiceChannel, Data.SoundNames[await Utils.RNG(24, 31), 2]); // Array range of yanni sounds
								else
									await e.Channel.SendMessage($"Already playing. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} stop`.");
							}
							else
								await e.Channel.SendMessage("You aren't connected to the right voice channel.");
						}
						else
							await e.Channel.SendMessage($"Not VC connected. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} vc`.");
					});
			return Task.FromResult(0);
		}

		public static Task GetRandomPortal2Sound(string c)
		{
			CService.CreateCommand(c)
					.Description("Plays a random challenge mode sound. Only works in voice channels.")
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						var channel = e.Server.Users.FirstOrDefault(user => user.Id == Bot.Client.CurrentUser.Id)?.VoiceChannel;
						if ((VoiceChannel.Connected)
						&& (channel != null))
						{
							if (e.User.VoiceChannel == null)
								await e.Channel.SendMessage("You have to connect to a voice channel.");
							else if (e.User.VoiceChannel == channel)
							{
								if (!(VoiceChannel.IsPlaying))
									await VoiceChannel.PlayWithFFmpegAsync(e.Server.Id, e.User.VoiceChannel, Data.SoundNames[await Utils.RNG(0, 24), 2]); // Array range of P2 sounds
								else
									await e.Channel.SendMessage($"Already playing. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} stop`.");
							}
							else
								await e.Channel.SendMessage("You aren't connected to the right voice channel.");
						}
						else
							await e.Channel.SendMessage($"Not VC connected. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} vc`.");
					});
			return Task.FromResult(0);
		}
	}
}