using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public.Vip
{
	public class Cloud : CommandModule
	{
		private static readonly Fetcher _fetchClient = new Fetcher();
		private static readonly string _cacheKey = "dropbox";

		private const uint _maxFilesPerFolder = 20;
		private const uint _maxfilesize = 5000 * 1024;   // 5MB

		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Cloud Module", LogColor.Init);
			await CloudCommands();
		}

		public static Task CloudCommands()
		{
			// Dropbox stuff
			CService.CreateCommand("upload")
					.Description("Uploads your attachments ending with .dem or .sav to a Dropbox account (owned by the bot owner) as a backup.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						try
						{
							// I've never seen a message that had more than one attachment
							if (e.Message.Attachments.Length == 1)
							{
								var file = e.Message.Attachments.First();

								// Check file extension
								var filename = file.Filename;
								var extension = Path.GetExtension(filename) ?? string.Empty;

								// Only allow demos and saves because Source Engine games :^)
								if ((extension == ".dem")
								|| (extension == ".sav"))
								{
									var mention = e.User.Mention;
									// Maximum 5000KB
									if (file.Size > _maxfilesize)
										await e.Channel.SendMessage("Your file is too large for an upload (max. 5MB).");
									else if (file.Size == 0)
										await e.Channel.SendMessage("Your file seems to be broken.");
									else
									{
										// Should give every user his own cache too
										var cachekey = $"{_cacheKey}{e.User.Id}";

										// Download data
										try
										{
											await _fetchClient.GetFileAndCacheAsync(file.Url, cachekey);
										}
										catch (Exception ex)
										{
											await Logger.SendAsync("Fetching.GetFileAndCacheAsync Error (AutoDownloader.CheckDropboxAsync)", ex);
											return;
										}

										// Get file
										var cachefile = await Caching.CFile.GetPathAndSaveAsync(cachekey);
										if (string.IsNullOrEmpty(cachefile))
											await Logger.SendAsync("Caching.CFile.GetPathAndSaveAsync Error (AutoDownloader.CheckDropboxAsync)", LogColor.Error);
										else
										{
											// Every user has his own folder
											var path = $"{Configuration.Default.DropboxFolderName}/{e.User.Id}";

											// Check if folder is full
											var files = await DropboxCom.GetFilesAsync(path);
											if (files?.Count >= _maxFilesPerFolder)
												await e.Channel.SendMessage($"Your folder is full. Try to list all files with `{Configuration.Default.PrefixCmd}dbfolder` and delete one with `{Configuration.Default.PrefixCmd}dbdelete <filename>`.");
											else
											{
												// Send file to Dropbox
												var msg = await e.Channel.SendMessage($"{mention} Uploading...");
												var sent = $"{mention} Uploaded {await Utils.AsRawText(filename)} to Dropbox.";
												if (await DropboxCom.UploadAsync(path, filename, cachefile))
													await msg.Edit(sent);
												else
													await msg.Edit($"{mention} **Upload error.**");

												var link = await DropboxCom.CreateLinkAsync($"{path}/{filename}");
												if (!(string.IsNullOrEmpty(link)))
													await msg.Edit($"{sent}\nDownload: <{link}>");
											}
										}
									}
								}
								else
									await e.Channel.SendMessage("Only .dem and .sav files are allowed.");
							}
							else if (e.Message.Attachments.Length > 1)
								await e.Channel.SendMessage("How did you do that? :ok_hand:");
							else
								await e.Channel.SendMessage("Please attach a .dem or .sav file to your message.");
						}
						catch (Exception ex)
						{
							await Logger.SendAsync("Cloud.CloudCommands Upload Command Error", ex);
							await e.Channel.SendMessage("**Error.**");
						}
					});

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
					.Description("Lists the latest files you've stored on Dropbox (max. five files if there are more than that).")
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
								foreach (var file in files.OrderBy(file => file.ModifiedDate).Take(5))
								{
									var duration = await Utils.GetDurationAsync(file.ModifiedDate);
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