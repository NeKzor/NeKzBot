using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Server;

namespace NeKzBot.Modules.Private.MainServer
{
	public class Cloud : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Cloud Module", LogColor.Init);
			await CloudCommands();
		}

		public static Task CloudCommands()
		{
			// Dropbox stuff
			CService.CreateCommand("cloud")
					.Alias("folder")
					.Description("Returns the link for the public demo folder. Just attach your demo and it'll be automatically uploaded to Dropbox.")
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Credentials.Default.DropboxFolderQuery != string.Empty ? $"<https://www.dropbox.com/sh/{Credentials.Default.DropboxFolderQuery}?dl=0>" : "Not available.");
					});

			CService.CreateCommand("dbfolder")
					.Alias("myfolder")
					.Description("Returns the list of files you've stored on Dropbox.")
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await DropboxCom.ListFilesAsync($"{Configuration.Default.DropboxFolderName}/{e.User.Id}"));
					});

			CService.CreateCommand("dbdelete")
					.Alias("myfolder")
					.Description($"Deletes the file from your own Dropbox folder. For master server-admin only try `{Configuration.Default.PrefixCmd}dbdelete <folder> <file>` to delete files from other users. The folder name is the id of a user.")
					.Parameter("file", ParameterType.Unparsed)
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if ((e.Args[0].Contains('|'))
						&& (e.User.Id == Credentials.Default.DiscordBotOwnerId))
						{
							var values = e.Args[0].Split('|');
							if (values.Length == 2)
								await e.Channel.SendMessage(await DropboxCom.DeleteFileAsync($"{Configuration.Default.DropboxFolderName}/{values[0]}", values[1]));
							else
								await e.Channel.SendMessage("Invalid parameters.");
						}
						else
							await e.Channel.SendMessage(await DropboxCom.DeleteFileAsync($"{Configuration.Default.DropboxFolderName}/{e.User.Id}", e.Args[0]));
					});
			return Task.FromResult(0);
		}
	}
}