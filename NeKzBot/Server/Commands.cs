using System.Threading.Tasks;
using Discord.Commands;

namespace NeKzBot.Server
{
	public abstract class Commands
	{
		public static CommandService CService { get; private set; }

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Commands", LogColor.Init);
			Bot.Client.UsingCommands(cs =>
			{
				cs.CustomPrefixHandler = Handlers.PrefixHandler;
				cs.ErrorHandler = Handlers.ErrorHandlerAsync;
			});

			CService = Bot.Client.GetService<CommandService>();
			CService.Root.AddCheck(Permissions.DisallowDMs);
			//CService.Root.AddCheck(Permissions.DisallowBots);
		}
	}
}