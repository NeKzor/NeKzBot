using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Server;
using NeKzBot.Webhooks;
using NeKzBot.Internals;

namespace NeKzBot.Resources
{
	public static partial class Data
	{
		public static List<InternalData> Manager { get; private set; }

		public static Task<InternalData> GetDataByName(string name, out int index)
			=> Task.FromResult(((index = Manager.FindIndex(data => data.Name == name)) != -1)
												? Manager[index]
												: null);

		public static Task<bool> DataExists(string name, out int index)
			 => Task.FromResult((index = Manager.FindIndex(data => data.Name == name)) != -1);

		public static Task<List<string>> GetDataNames()
			=> Task.FromResult(Manager.Select(data => data.Name)
									  .ToList());

		public static async Task InitMangerAsync()
		{
			await Logger.SendAsync("Initializing Data Manger", LogColor.Init);
			Manager = new List<InternalData>
			{
				new InternalData("cc", true, true, fileNameConsoleCommands, ConsoleCommands),
				new InternalData("aa", false, false, fileNameAudioAliases, AudioAliases),
				new InternalData("playingstatus", true, true, fileNameRandomGames, RandomGames),
				new InternalData("credits", true, true, fileNameSpecialThanks, SpecialThanks),
				new InternalData("twitch", true, true, fileNameTwitchStreamers, TwitchStreamers),
				new InternalData("scripts", false, false, fileNameScriptFiles, ScriptFiles),
				new InternalData("memes", true, true, fileNameMemeCommands, MemeCommands),
				new InternalData("twitch", true, true, fileNameTwitchStreamers, TwitchStreamers),
				new InternalData("tools", true, true, fileNameToolCommands, ToolCommands),
				new InternalData("links", true, true, fileNameLinkCommands, LinkCommands),
				new InternalData("runs", true, true, fileNameProjectNames, ProjectNames),
				new InternalData("p2maps", false, false, fileNamePortal2Maps, Portal2Maps),
				new InternalData("quotes", true, true, fileNameQuoteNames, QuoteNames),
				new InternalData("sounds", false, false, fileNameSoundNames, SoundNames),
				new InternalData("exploits", true, true, fileNameP2Exploits, Portal2Exploits),
				new InternalData("p2hook", true, true, fileNameP2Subscribers, P2Subscribers),
				new InternalData("srcomhook", true, true, fileNameSRComSubscribers, SRComSubscribers),
				new InternalData("twtvhook", true, true, fileNameTwitchTvSubscribers, TwitchTvSubscribers)
			};
		}

		public static async Task<bool> ReloadAsync(int index)
		{
			try
			{
				// Reload data
				await InitAsync(index);

				// Reload data manager
				if (Manager[index].Data.GetType() == typeof(string[]))
					await Manager[index].ChangeData(await Utils.ReadFromFileAsync(Manager[index].FileName) as string[]);
				else if (Manager[index].Data.GetType() == typeof(string[,]))
					await Manager[index].ChangeData(await Utils.ReadFromFileAsync(Manager[index].FileName) as string[,]);
				else if (Manager[index].Data.GetType() == typeof(List<WebhookData>))
					await Manager[index].ChangeData(await WebhookData.ParseDataAsync(Manager[index].FileName));

				// Reload command
				await InitCommandByIndexAsync(index);
				return true;
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Data.ReloadAsync Error", e);
				return false;
			}
		}
	}
}