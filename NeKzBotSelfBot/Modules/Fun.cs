using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.EmojiTools;
using NeKzBot.Resources;

namespace NeKzBot.Modules
{
	public class Fun : ModuleBase
	{
		[Command("ping")]
		public async Task EverybodyStartedHere()
			=> await Message.EditAsync(Context.Message, "pong");

		[Command("ris")]
		public async Task PrintRis([Remainder]string text)
			=> await Message.EditAsync(Context.Message, await Utils.ToRISAsync(text));

		[Command("react")]
		public async Task PrintRisAsReaction(ulong id, string text)
		{
			await Context.Message.DeleteAsync();
			var msg = await Utils.GetMessageAsync(Context, id) as IUserMessage;
			if (msg == null)
				return;

			var emojis = await Utils.GetEmojisAsRISAsync(text);
			if (emojis.Count > 20)
				return;

			foreach (var item in emojis)
				await msg.AddReactionAsync(UnicodeEmoji.FromText(item));
		}
	}
}