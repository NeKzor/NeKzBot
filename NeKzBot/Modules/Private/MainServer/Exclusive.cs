using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Tasks;

namespace NeKzBot.Modules.Private.MainServer
{
	public class Exclusive : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Exclusive Module", LogColor.Init);
			await ExclusiveCommands(Configuration.Default.BotCmd);
		}

		private static Task ExclusiveCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("connect")
						.Alias("vc")
						.Description("Connects the bot to a voice channel. It will follow you automatically if you connected to one already.")
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
						.Description("Disconnects the bot from a voice channel.")
						.AddCheck(Permissions.MainServerOnly)
						.Do(async e => await VoiceChannel.DisconnectAsync(e.Server.Id));

				GBuilder.CreateCommand("stop")
						.Description("Stops a currently running audio stream.")
						.AddCheck(Permissions.MainServerOnly)
						.Do(async _ => await VoiceChannel.StopAudio());
			});
			return Task.FromResult(0);
		}
	}
}