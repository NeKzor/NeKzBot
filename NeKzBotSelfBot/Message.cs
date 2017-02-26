using System.Threading.Tasks;
using Discord;
using Discord.Addons.EmojiTools;

namespace NeKzBot
{
	public static class Message
	{
		public static async Task EditAsync(IUserMessage message, string content)
			=> await message.ModifyAsync(msg => msg.Content = content);

		public static async Task EditAsync(IUserMessage message, EmbedBuilder embed, string content = "")
		{
			await message.ModifyAsync(msg =>
			{
				msg.Content = content;
				msg.Embed = embed.Build();
			});
		}

		public static async Task AddEmojisAsync(IUserMessage message, params string[] emojis)
		{
			foreach (var emoji in emojis)
				await message.AddReactionAsync(UnicodeEmoji.FromText(emoji));
		}
	}
}