using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SourceDemoParser.Net;
using NeKzBot.Server;
using NeKzBot.Utilities;
using NeKzBot.Resources;
using NeKzBot.Internals.Entities;

namespace NeKzBot.Modules.Public
{
	public class Contest : CommandModule
	{
		private static readonly Fetcher _fetchClient = new Fetcher();
		private static readonly string _cacheKey = "submission";

		// Private
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Contest Module", LogColor.Init);
			await LoadCommands();
		}

		private static Task LoadCommands()
		{
			CService.CreateCommand("contestrules")
					.Description("Changes the contest rules.")
					.Parameter("rules", ParameterType.Unparsed)
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var contest = await Data.Get<Submissions>("contest");
						contest.Rules = e.GetArg("rules");
						await Data.ChangeAsync<Submissions>("contest", contest);
						if (await Data.ExportAsync<Submissions>("contest"))
							await e.Channel.SendMessage("Rules have been changed.");
						else
							await e.Channel.SendMessage("Error. Could not export new change.");
					});

			CService.CreateCommand("contestgame")
					.Description("Changes the game name of the contest.")
					.Parameter("game_name", ParameterType.Unparsed)
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var contest = await Data.Get<Submissions>("contest");
						contest.GameName = e.GetArg("game_name");
						await Data.ChangeAsync<Submissions>("contest", contest);
						if (await Data.ExportAsync<Submissions>("contest"))
							await e.Channel.SendMessage("Game name has been changed.");
						else
							await e.Channel.SendMessage("Error. Could not export new change.");
					});

			CService.CreateCommand("contestmap")
					.Description("Changes the game name of the contest.")
					.Parameter("map_name", ParameterType.Unparsed)
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var contest = await Data.Get<Submissions>("contest");
						contest.MapName = e.GetArg("map_name");
						await Data.ChangeAsync<Submissions>("contest", contest);
						if (await Data.ExportAsync<Submissions>("contest"))
							await e.Channel.SendMessage("Map name has been changed.");
						else
							await e.Channel.SendMessage("Error. Could not export new change.");
					});

			CService.CreateCommand("setcontest")
					.Description("Changes the state of the contest.")
					.Parameter("state", ParameterType.Required)
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (bool.TryParse(e.GetArg("state"), out var result))
							await e.Channel.SendMessage((await ChangeContestStatus(result)) ? "Contest is now available for new submissions." : "Error.Could not export new change.");
						else
							await e.Channel.SendMessage("Parameter should be a boolean.");
					});

			CService.CreateCommand("opencontest")
					.Alias("contest open")
					.Description("Opens the contest.")
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage((await ChangeContestStatus(true)) ? "Contest is now available for new submissions." : "Error.Could not export new change.");
					});

			CService.CreateCommand("closecontest")
					.Alias("contest close")
					.Description("Closes the contest.")
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage((await ChangeContestStatus(true)) ? "Contest is now available for new submissions." : "Error.Could not export new change.");
					});
			return Task.FromResult(0);
		}

		public static async Task<bool> ChangeContestStatus(bool state)
		{
			var contest = await Data.Get<Submissions>("contest");
			contest.IsAvailable = state;
			return await Data.ExportAsync<Submissions>("contest");
		}

		// Public
		public static async Task CheckCommand(Message msg)
		{
			if (string.Equals(msg.RawText, $"{Configuration.Default.PrefixCmd}submit", StringComparison.CurrentCultureIgnoreCase))
			{
				await msg.Channel.SendIsTyping();
				switch (await CheckForSubmissioAsync(new MessageEventArgs(msg)))
				{
					case SubmissionError.NoFileAttached:
						await msg.Channel.SendMessage("You didn't attach a file.");
						break;
					case SubmissionError.InvalidFileExtension:
						await msg.Channel.SendMessage("Invalid file. Only .dem files are allowed.");
						break;
					case SubmissionError.InvalidFileContents:
						await msg.Channel.SendMessage("Invalid demo file.");
						break;
					case SubmissionError.WrongGame:
						await msg.Channel.SendMessage("Wrong game. Did you submit the right demo?");
						break;
					case SubmissionError.WrongMap:
						await msg.Channel.SendMessage("Wrong map. Did you submit the right demo?");
						break;
					case SubmissionError.SubmissionsClosed:
						await msg.Channel.SendMessage("Submissions are closed.");
						break;
					case SubmissionError.OK:
						await msg.Channel.SendMessage($"Demo file has been submitted. Try {Configuration.Default.PrefixCmd}submission to see some information about your latest entry.");
						break;
					default:
						await msg.Channel.SendMessage("You found a bug. Your submission didn't count. Please contact the server owner about this issue.");
						break;
				}
			}
			else if (string.Equals(msg.RawText, $"{Configuration.Default.PrefixCmd}submission", StringComparison.CurrentCultureIgnoreCase))
			{
				await msg.Channel.SendIsTyping();
				var data = (await Data.Get<Submissions>("contest")).Players.Find(s => s.UserId == msg.User.Id);
				if (data != null)
				{
					if (await SourceDemo.TryParseFile(await Utils.GetAppPath() + $"/Resources/Private/submissions/{msg.User.Id}.dem", out var demo))
						await msg.Channel.SendMessage($"**[{data.SubmisionDate.ToString("s")}]**\n{demo.MapName} in {demo.AdjustedTicks} ({demo.GetAdjustedTime().ToString("N3")}s) by {demo.Client}.");
					else
						await msg.Channel.SendMessage("Your demo could not be found or parsed. Please contact the bot owner.");
				}
				else
					await msg.Channel.SendMessage("You didn't submit anything.");
			}
			else if (string.Equals(msg.RawText, $"{Configuration.Default.PrefixCmd}rules", StringComparison.CurrentCultureIgnoreCase))
			{
				await msg.Channel.SendIsTyping();
				var contest = await Data.Get<Submissions>("contest");
				if (contest.IsAvailable)
					await msg.Channel.SendMessage(contest.Rules);
				else
					await msg.Channel.SendMessage("There's currently no contest available.");
			}
		}

		public static async Task<SubmissionError> CheckForSubmissioAsync(MessageEventArgs args)
		{
			try
			{
				var contest = await Data.Get<Submissions>("contest");
				if (contest.IsAvailable)
				{
					if (args.Message.Attachments.Length == 1)
					{
						var file = args.Message.Attachments[0];
						var extension = Path.GetExtension(file.Filename) ?? string.Empty;
						if (extension == ".dem")
						{
							var cachekey = $"{_cacheKey}_{args.User.Id}_{args.User.Name}_{args.User.Discriminator}";
							try
							{
								await _fetchClient.GetFileAndCacheAsync(file.Url, cachekey);
							}
							catch (Exception e)
							{
								await Logger.SendAsync("Fetching.GetFileAndCacheAsync Error (Contest.CheckForSubmissioAsync)", e);
								return SubmissionError.DownloadFailed;
							}

							var cachefile = await Caching.CFile.GetPathAndSaveAsync(cachekey);
							if (string.IsNullOrEmpty(cachefile))
								await Logger.SendAsync("Caching.CFile.GetPathAndSaveAsync Error (Contest.CheckForSubmissioAsync)", LogColor.Error);
							else
							{
								// Validate file
								if (await SourceDemo.TryParseFile(cachefile, out var demo))
								{
									if (demo.GameInfo.Name != contest.GameName)
										return SubmissionError.WrongGame;
									if (demo.MapName != contest.MapName)
										return SubmissionError.WrongMap;

									// Copy to submission folder
									File.Copy(cachefile, await Utils.GetAppPath() + $"/Resources/Private/submissions/{args.User.Id}.dem");
									// Change internal data
									var found = contest.Players.FindIndex(s => s.UserId == args.User.Id);
									if (found != -1)
										contest.Players.RemoveAt(found);
									contest.Players.Add(new Submission(args.User.Name, args.User.Discriminator, args.User.Id));
									// Export new data
									await Data.ChangeAsync<Submissions>("contest", contest);
									if (await Data.ExportAsync<Submissions>("contest"))
										return SubmissionError.OK;
								}
								else
									return SubmissionError.InvalidFileContents;
							}
						}
						else
							return SubmissionError.UnknownError;
					}
					else
						return SubmissionError.NoFileAttached;
				}
				else
					return SubmissionError.SubmissionsClosed;
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Contest.CheckForSubmissioAsync Error", e);
			}
			return SubmissionError.UnknownError;
		}
	}

	public enum SubmissionError
	{
		NoFileAttached,
		InvalidFileExtension,
		InvalidFileContents,
		WrongGame,
		WrongMap,
		SubmissionsClosed,
		DownloadFailed,
		UnknownError,
		OK
	}
}