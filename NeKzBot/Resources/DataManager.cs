using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Internals;
using NeKzBot.Server;
using NeKzBot.Webhooks;

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
				new InternalData("cc", true, true, _fileNameConsoleCommands, ConsoleCommands),
				new InternalData("aa", false, false, _fileNameAudioAliases, AudioAliases),
				new InternalData("playingstatus", true, true, _fileNameRandomGames, RandomGames),
				new InternalData("credits", true, true, _fileNameSpecialThanks, SpecialThanks),
				new InternalData("twitch", true, true, _fileNameTwitchStreamers, TwitchStreamers),
				new InternalData("scripts", false, false, _fileNameScriptFiles, ScriptFiles),
				new InternalData("memes", true, true, _fileNameMemeCommands, MemeCommands),
				new InternalData("tools", true, true, _fileNameToolCommands, ToolCommands),
				new InternalData("links", true, true, _fileNameLinkCommands, LinkCommands),
				new InternalData("runs", true, true, _fileNameProjectNames, ProjectNames),
				new InternalData("p2maps", false, false, _fileNamePortal2Maps, Portal2Maps),
				new InternalData("quotes", true, true, _fileNameQuoteNames, QuoteNames),
				new InternalData("sounds", false, false, _fileNameSoundNames, SoundNames),
				new InternalData("exploits", true, true, _fileNameP2Exploits, Portal2Exploits),
				new InternalData("p2hook", true, true, _fileNameP2Subscribers, P2Subscribers),
				new InternalData("srcomsourcehook", true, true, _fileNameSpeedrunComSourceSubscribers, SpeedrunComSourceSubscribers),
				new InternalData("twtvhook", true, true, _fileNameTwitchTvSubscribers, TwitchTvSubscribers),
				new InternalData("vip", true, true, _fileNameVipData, VipGuilds),
				new InternalData("p2cvars", true, true, _fileNamePortal2Cvars, Portal2Cvars),
				new InternalData("srcomportal2hook", true, true, _fileNameSpeedrunComSourceSubscribers, SpeedrunComPortal2Subscribers)
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