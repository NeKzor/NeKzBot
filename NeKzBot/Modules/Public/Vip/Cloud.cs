using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public.Vip
{
	public class Cloud : CommandModule
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
					.Alias("folder", "db")
					.Description("Returns the main link for the public demo folder.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage((Credentials.Default.DropboxFolderQuery != string.Empty) ? $"<https://www.dropbox.com/sh/{Credentials.Default.DropboxFolderQuery}?dl=0>" : "Not available.");
					});

			CService.CreateCommand("dbfolder")
					.Alias("myfolder", "dbfiles")
					.Description("Returns the list of files you've stored on Dropbox.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var path = $"{Configuration.Default.DropboxFolderName}/{e.User.Id}";
						var files = await DropboxCom.GetFilesAsync(path);
						if (files != null)
						{
							if (files.Count > 0)
							{
								var output = string.Empty;
								foreach (var file in files)
								{
									var duration = await Utils.GetDuration(file.ModifiedDate);
									output += $"\n{await Utils.AsRawText(file.Name)}{((duration != default(string)) ? $" (modified {duration} ago)" : string.Empty)}";
								}
								var link = await DropboxCom.CreateLinkAsync(path);
								await e.Channel.SendMessage((string.IsNullOrEmpty(link)) ? output : $"<{link}>{output}");
							}
							else
								await e.Channel.SendMessage("You don't have any files in your folder.");
						}
						else
							await e.Channel.SendMessage("Could not find your folder.");
					});

			CService.CreateCommand("dbdelete")
					.Alias("dbdel")
					.Description($"Deletes the file from your own Dropbox folder. For bot owner try `{Configuration.Default.PrefixCmd}dbdelete <file> <folder>` to delete files from other users. The folder name is the id of a user.")
					.Parameter("file", ParameterType.Required)
					.Parameter("folder", ParameterType.Optional)
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var filename = e.GetArg("file");
						var foldername = e.GetArg("folder");
						if (string.IsNullOrEmpty(foldername))
							await e.Channel.SendMessage(await DropboxCom.DeleteFileAsync($"{Configuration.Default.DropboxFolderName}/{e.User.Id}", filename));
						else
						{
							if (e.User.Id == Credentials.Default.DiscordBotOwnerId)
								await e.Channel.SendMessage(await DropboxCom.DeleteFileAsync($"{Configuration.Default.DropboxFolderName}/{foldername}", filename, true));
							else if (e.User.Id.ToString() == foldername)
								await e.Channel.SendMessage("No need to set a folder.");
							else
								await e.Channel.SendMessage("You are not allowed to delete files from other folders.");
						}
					});
			return Task.FromResult(0);
		}
	}
}