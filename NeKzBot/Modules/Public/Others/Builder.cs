using System.Threading.Tasks;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public.Others
{
	public class Builder : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Builder Module", LogColor.Init);
			await Utils.CommandBuilder(() => Tools(Utils.CBuilderIndex), 0, Data.ToolCommands);
			await Utils.CommandBuilder(() => Memes(Utils.CBuilderIndex), 0, Data.MemeCommands);
			await Utils.CommandBuilder(() => Links(Utils.CBuilderIndex), 0, Data.LinkCommands);
			await Utils.CommandBuilder(() => Text(Utils.CBuilderIndex), 0, Data.QuoteNames);
		}

		public static Task Memes(int i)
		{
			CService.CreateCommand(Data.MemeCommands[i, 0])
					.Description(Data.MemeCommands[i, 1])
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						// 0 command, 1 description, 2 picture, 3 text
						await e.Channel.SendIsTyping();
						// Text only
						if (Data.MemeCommands[i, 2] == string.Empty)
							await e.Channel.SendMessage(Data.MemeCommands[i, 3]);
						// File only
						else if (Data.MemeCommands[i, 3] == string.Empty)
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/{Data.MemeCommands[i, 2]}");
						// File and text
						else
						{
							await e.Channel.SendMessage($"**{Data.MemeCommands[i, 3]}**");
							await Task.Delay(333);
							await e.Channel.SendFile($"{await Utils.GetPath()}/Resources/Private/pics/{Data.MemeCommands[i, 2]}");
						}
					});
			return Task.FromResult(0);
		}

		public static Task Tools(int i)
		{
			CService.CreateCommand(Data.ToolCommands[i, 0])
					.Description(Data.ToolCommands[i, 1])
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"**{Data.ToolCommands[i, 3]}**\n{Data.ToolCommands[i, 2]}");
					});
			return Task.FromResult(0);
		}

		public static Task Links(int i)
		{
			CService.CreateCommand(Data.LinkCommands[i, 0])
					.Description(Data.LinkCommands[i, 1])
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{Data.LinkCommands[i, 3]}\n{Data.LinkCommands[i, 2]}");
					});
			return Task.FromResult(0);
		}

		public static Task Text(int i)
		{
			CService.CreateGroup("quote", GBuilder =>
			{
				GBuilder.CreateCommand(Data.QuoteNames[i, 0])
						.Description("Could be true or not.")
						.AddCheck(Permissions.VipGuildsOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"*{Data.QuoteNames[i, 1]}*");
						});
			});
			return Task.FromResult(0);
		}
	}
}