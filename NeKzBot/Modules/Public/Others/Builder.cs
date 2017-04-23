using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public.Others
{
	public class Builder : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Builder Module", LogColor.Init);
			await Utils.CommandBuilder(CreateTool, (await Data.Get<Complex>("tools")).Values);
			await Utils.CommandBuilder(CreateMeme, (await Data.Get<Complex>("memes")).Values);
			await Utils.CommandBuilder(CreateLink, (await Data.Get<Complex>("links")).Values);
			await Utils.CommandBuilder(CreateQuote, (await Data.Get<Complex>("quotes")).Values);
		}

		public static Action<IEnumerable<string>> CreateMeme = collection =>
		{
			var meme = collection.ToList();
			CService.CreateCommand(meme[0])
					.Description(meme[1])
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						// 0 command, 1 description, 2 picture, 3 text
						await e.Channel.SendIsTyping();
						// Text only
						if (meme[2].Trim() == string.Empty)
							await e.Channel.SendMessage(meme[3]);
						// File only
						else if (meme[3].Trim() == string.Empty)
							await e.Channel.SendFile($"{await Utils.GetAppPath()}/Resources/Private/pics/{meme[2]}");
						// File and text
						else
						{
							await e.Channel.SendMessage($"**{meme[3]}**");
							await Task.Delay(333);
							await e.Channel.SendFile($"{await Utils.GetAppPath()}/Resources/Private/pics/{meme[2]}");
						}
					});
		};

		public static Action<IEnumerable<string>> CreateTool = collection =>
		{
			var tool = collection.ToList();
			CService.CreateCommand(tool[0])
					.Description(tool[1])
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"**{tool[3]}**\n{tool[2]}");
					});
		};

		public static Action<IEnumerable<string>> CreateLink = collection =>
		{
			var link = collection.ToList();
			CService.CreateCommand(link[0])
					.Description(link[1])
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{link[3]}\n{link[2]}");
					});
		};

		public static Action<IEnumerable<string>> CreateQuote = collection =>
		{
			var text = collection.ToList();
			CService.CreateGroup("quote", GBuilder =>
			{
				GBuilder.CreateCommand(text[0])
						.Description("No memes. Note: This might not be exact.")
						.AddCheck(Permissions.VipGuildsOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"*{text[1]}*");
						});
			});
		};
	}
}