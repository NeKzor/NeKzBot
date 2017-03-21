using System.Threading.Tasks;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public.Others
{
	public class Builder : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Builder Module", LogColor.Init);
			await Utils.CommandBuilder(async() => await CreateToolsAsync(Utils.CBuilderIndex), 0, (await Data.Get<Complex>("tools")).Cast());
			await Utils.CommandBuilder(async () => await CreateMemesAsync(Utils.CBuilderIndex), 0, (await Data.Get<Complex>("memes")).Cast());
			await Utils.CommandBuilder(async () => await CreateLinksAsync(Utils.CBuilderIndex), 0, (await Data.Get<Complex>("links")).Cast());
			await Utils.CommandBuilder(async () => await CreateQuotesAsync(Utils.CBuilderIndex), 0, (await Data.Get<Complex>("quotes")).Cast());
		}

		public static async Task CreateMemesAsync(int i)
		{
			var meme = (await Data.Get<Complex>("memes")).Values[i].Value;
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
		}

		public static async Task CreateToolsAsync(int i)
		{
			var tool = (await Data.Get<Complex>("tools")).Values[i].Value;
			CService.CreateCommand(tool[0])
					.Description(tool[1])
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"**{tool[3]}**\n{tool[2]}");
					});
		}

		public static async Task CreateLinksAsync(int i)
		{
			var link = (await Data.Get<Complex>("links")).Values[i].Value;
			CService.CreateCommand(link[0])
					.Description(link[1])
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{link[3]}\n{link[2]}");
					});
		}

		public static async Task CreateQuotesAsync(int i)
		{
			var text = (await Data.Get<Complex>("quotes")).Values[i].Value;
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
		}
	}
}