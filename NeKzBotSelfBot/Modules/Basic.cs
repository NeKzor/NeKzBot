using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.CodeAnalysis;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules
{
	public class Basic : ModuleBase
	{
		[Command("quote")]
		public async Task GetQuote(ulong id)
			=> await GetQuote(Context.Message, id);

		[Command("quote")]
		public async Task GetQuote(ulong id, [Remainder]string content)
			=> await GetQuote(Context.Message, id, content);

		[Command("post")]
		public async Task PrintPost(string title, [Remainder]string text)
		{
			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = title,
				Description = text
			});
			await Message.AddEmojisAsync(Context.Message, ":thumbsup:", ":thumbsdown:");
		}

		[Command("poll")]
		public async Task PrintPoll(string title, params string[] options)
		{
			if ((options.Length < 2)
			||(options.Length > 11))
				return;

			var emojis = new string[options.Length];
			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = "Poll"
			}
			.AddField(async field =>
			{
				field.Name = title;
				var output = string.Empty;
				for (int i = 1; i < options.Length; i++)
				{
					var emoji = (i == 10)
								   ? ":keycap_ten:"
								   : await Utils.ToRISAsync(i.ToString());
					output += $"\n{emoji} {options[i]}";
					emojis[i - 1] = emoji;
				}
				field.Value = output;
			}));
			await Message.AddEmojisAsync(Context.Message, emojis);
		}

		[Command("play")]
		public async Task ChangeGame([Remainder]string name)
			=> await Bot.Client.SetGameAsync(name);

		[Command("play")]
		public async Task ChangeGame(bool stream, [Remainder]string name)
			=> await Bot.Client.SetGameAsync(name, stream ? Configuration.Default.TwitchChannelLink : null, stream ? StreamType.Twitch : StreamType.NotStreaming);

		[Command("play")]
		public async Task ChangeGame(Uri link, [Remainder]string name)
		{
			await Message.EditAsync(Context.Message, Context.Message.Content.Replace($"{link.AbsoluteUri}", $"<{link.AbsoluteUri}>"));
			await Bot.Client.SetGameAsync(name, link.AbsoluteUri, StreamType.Twitch);
		}

		private async Task GetQuote(IUserMessage message, ulong id, string content = "")
		{
			var msg = await Utils.GetMessageAsync(Context, id);
			if (msg == null)
				return;

			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = msg.Author.Username,
					IconUrl = msg.Author.GetAvatarUrl()
				},
				Color = await Utils.GetUserColor(await Context.Guild.GetUserAsync(msg.Author.Id), Context.Guild),
				Timestamp = msg.Timestamp
			};

			if (msg.Content != string.Empty)
				embed.WithDescription(msg.Content);
			if (msg.Attachments.Any())
			{
				// Not sure how and why there can be more than one attachment in one message...???
				foreach (var attachement in msg.Attachments)
				{
					if (await Utils.IsImage(attachement.Filename))
						embed.WithImageUrl(attachement.Url);
					else
					{
						embed.AddField(field =>
						{
							field.Name = (msg.Attachments.Count == 1)
																? "Attachment"
																: "Attachments";
							var output = string.Empty;
							foreach (var item in msg.Attachments)
								output += $"\n[{item.Filename}]({item.Url})";
							field.Value = output;
						});
						break;
					}
				}
			}
			await Message.EditAsync(message, embed, content);
		}
	}
}