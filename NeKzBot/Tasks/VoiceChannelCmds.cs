using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Modules;
using NeKzBot.Resources;

namespace NeKzBot.Tasks
{
	public class VoiceChannelCmds : Commands
	{
		public static async Task Load()
		{
			await Logging.CON("Loading voice channel commands", System.ConsoleColor.DarkYellow);
			await GetRandomYanniSound("yanni");
			await GetRandomPortal2Sound("p2");
			await Utils.CommandCreator(() => PlaySoundInVC(Utils.group, Utils.index), 0, Data.soundNames, true, Data.audioAliases);
		}

		public static Task PlaySoundInVC(string s, int i)
		{
			cmd.CreateGroup(s, g =>
			{
				g.CreateCommand(Data.soundNames[i, 0])
				.Description(Data.soundNames[i, 1])
				.Do(async (e) =>
				{
					if (VoiceChannel.vcconnected)
					{
						if (!VoiceChannel.isplaying)
							await Audio.PlayWithFFmpeg(e.Server.Id, e.User.VoiceChannel, Data.soundNames[i, 2]);
						else
							await e.Channel.SendMessage($"Bot is already playing sound. Try `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} {Data.soundNames[i, 0] + "`"}");
					}
					else
						await e.Channel.SendMessage($"Bot isn't VC connected. Try `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} vc`");
				});
			});
			return Task.FromResult(0);
		}

		public static Task GetRandomYanniSound(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` plays a random sound from the living meme.\n**-** Only works when in VC.")
			.Do(async (e) =>
			{
				if (VoiceChannel.vcconnected)
				{
					if (!VoiceChannel.isplaying)
						await Audio.PlayWithFFmpeg(e.Server.Id, e.User.VoiceChannel, Data.soundNames[Utils.RNG(24, 31), 2]); // Array range of yanni sounds
					else
						await e.Channel.SendMessage($"Bot is already playing sound. Try `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} " + Data.soundNames[Utils.RNG(24, 31), 0] + "`");
				}
				else
					await e.Channel.SendMessage($"Bot isn't VC connected. Try `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} vc`");
			});
			return Task.FromResult(0);
		}

		public static Task GetRandomPortal2Sound(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` plays a random challenge mode sound.\n**-** Only works when in VC.")
			.Do(async (e) =>
			{
				if (VoiceChannel.vcconnected)
				{
					if (!VoiceChannel.isplaying)
						await Audio.PlayWithFFmpeg(e.Server.Id, e.User.VoiceChannel, Data.soundNames[Utils.RNG(0, 24), 2]); // Array range of P2 sounds
					else
						await e.Channel.SendMessage($"Bot is already playing sound. Try `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} " + Data.soundNames[Utils.RNG(0, 24), 0] + "`");
				}
				else
					await e.Channel.SendMessage($"Bot isn't VC connected. Try `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} vc`");
			});
			return Task.FromResult(0);
		}
	}
}