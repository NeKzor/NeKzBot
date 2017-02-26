using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Tasks;
using NeKzBot.Server;

namespace NeKzBot.Modules
{
	public class Exclusive : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Bot Exclusive Commands", LogColor.Init);
			await ExclusiveCommands(Configuration.Default.BotCmd);
		}

		private static Task ExclusiveCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("connect")
						.Alias("vc")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} connect` connects the bot to a voice channel.\n• It will follow you automatically if you connected to one already.")
						.AddCheck(Permissions.MainServerOnly)
						.Do(async e =>
						{
							if (e.User.VoiceChannel == null)
								await VoiceChannel.ConnectAsync(e.Server.VoiceChannels.FirstOrDefault());
							else
								await VoiceChannel.ConnectAsync(e.User.VoiceChannel);
						});

				GBuilder.CreateCommand("disconnect")
						.Alias("dc")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} disconnect` disconnects the bot when it's in a voice channel.")
						.AddCheck(Permissions.MainServerOnly)
						.Do(async e => await VoiceChannel.DisconnectAsync(e.Server.Id));

				GBuilder.CreateCommand("stop")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} stop` stops a currently running audio stream.")
						.AddCheck(Permissions.MainServerOnly)
						.Do(async _ => await VoiceChannel.StopAudio());
			});
			return Task.FromResult(0);
		}
	}
}