using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;

namespace NeKzBot.Modules.Public.Vip
{
	public class Sound : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Sound Module", LogColor.Init);
			await GetRandomYanniSound("yanni");
			await GetRandomPortal2Sound("p2");
			await Utils.CommandBuilder(() => PlaySoundInVoiceChannel(Utils.CBuilderGroup, Utils.CBuilderIndex), 0, Data.SoundNames, true, Data.AudioAliases);
			await BotVipCommands(Configuration.Default.BotCmd);
		}

		public static Task PlaySoundInVoiceChannel(string s, int i)
		{
			CService.CreateGroup(s, GBuilder =>
			{
				GBuilder.CreateCommand(Data.SoundNames[i, 0])
						.Description(Data.SoundNames[i, 1])
						.AddCheck(Permissions.VipGuildsOnly)
						.Do(async e =>
						{
							var check = await VoiceChannel.ConnectionCheck(e.Server, e.User);
							if (check == AudioError.None)
							{
								var result = await VoiceChannel.PlayAsync(e.Server.Id, Data.SoundNames[i, 2]);
								if (result != AudioError.None)
									await e.Channel.SendMessage(result);
							}
							else
								await e.Channel.SendMessage(check);
						});
			});
			return Task.FromResult(0);
		}

		public static Task GetRandomYanniSound(string c)
		{
			CService.CreateCommand(c)
					.Description("Plays a random sound from the living meme. Only works in voice channels.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						var check = await VoiceChannel.ConnectionCheck(e.Server, e.User);
						if (check == AudioError.None)
						{
							var result = await VoiceChannel.PlayAsync(e.Server.Id, Data.SoundNames[await Utils.Rng(24, 31), 2]); // Array range of P2 sounds
							if (result != AudioError.None)
								await e.Channel.SendMessage(result);
						}
						else
							await e.Channel.SendMessage(check);
					});
			return Task.FromResult(0);
		}

		public static Task GetRandomPortal2Sound(string c)
		{
			CService.CreateCommand(c)
					.Description("Plays a random challenge mode sound. Only works in voice channels.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						var check = await VoiceChannel.ConnectionCheck(e.Server, e.User);
						if (check == AudioError.None)
						{
							var result = await VoiceChannel.PlayAsync(e.Server.Id, Data.SoundNames[await Utils.Rng(0, 24), 2]); // Array range of P2 sounds
							if (result != AudioError.None)
								await e.Channel.SendMessage(result);
						}
						else
							await e.Channel.SendMessage(check);
					});
			return Task.FromResult(0);
		}

		// Admin only
		private static Task BotVipCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("connect")
						.Alias("vc")
						.Description("Connects the bot to a voice channel. It will follow you automatically if you have connected to one already.")
						.AddCheck(Permissions.VipGuildsOnly)
						.AddCheck(Permissions.AdminOnly)
						.Do(async e =>
						{
							var result = string.Empty;
							if (e.User.VoiceChannel == null)
								result = await VoiceChannel.ConnectAsync(e.Server, e.Server.VoiceChannels.FirstOrDefault());
							else
								result = await VoiceChannel.ConnectAsync(e.Server, e.User.VoiceChannel);

							if (result != AudioError.None)
								await e.Channel.SendMessage(result);
						});

				GBuilder.CreateCommand("disconnect")
						.Alias("dc")
						.Description("Disconnects the bot from a voice channel.")
						.AddCheck(Permissions.VipGuildsOnly)
						.AddCheck(Permissions.AdminOnly)
						.Do(async e =>
						{
							if (!(await VoiceChannel.DisconnectAsync(e.Server)))
								await e.Channel.SendMessage(AudioError.Generic);
						});

				GBuilder.CreateCommand("stop")
						.Description("Stops a currently running audio stream.")
						.AddCheck(Permissions.VipGuildsOnly)
						.AddCheck(Permissions.AdminOnly)
						.Do(async e =>
						{
							if (!(await VoiceChannel.StopAudio(e.Server.Id)))
								await e.Channel.SendMessage(AudioError.Generic);
						});
			});
			return Task.FromResult(0);
		}
	}
}