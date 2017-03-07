using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using TweetSharp;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;

namespace NeKzBot.Modules
{
	public class Others : ModuleBase
	{
		[Command("item")]
		public async Task GetSteamWorkshopItem(Uri link)
		{
			var workshopitem = await Steam.GetSteamWorkshopAsync(link);
			if (workshopitem == null)
				return;

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = workshopitem.UserName,
					Url = workshopitem.UserLink,
					IconUrl = workshopitem.UserAvatar
				},
				Color = Data.SteamColor,
				Title = $"{workshopitem.GameName} Workshop Item",
				Description = $"{workshopitem.ItemTitle} made by [{workshopitem.UserName}]({workshopitem.UserLink})",
				Url = workshopitem.ItemLink,
				ImageUrl = workshopitem.ItemImage,
				Footer = new EmbedFooterBuilder
				{
					Text = "steamcommunity.com",
					IconUrl = "https://steamcommunity.com/favicon.ico"
				}
			});
		}

		[Command("stream")]
		public async Task GetTwitchStream(string channel)
		{
			var stream = await TwitchTv.GetPreviewAsync(channel);
			if (stream == null)
				return;

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = stream.ChannelName,
					Url = stream.StreamLink,
					IconUrl = stream.AvatarLink
				},
				Color = Data.TwitchColor,
				Title = "Twitch Stream",
				Description = $"[{stream.ChannelName}]({stream.StreamLink}) is {stream.GameName} for {stream.ChannelViewers} viewer{(stream.ChannelViewers == 1 ? string.Empty : "s")}!\n*{stream.StreamTitle}*",
				Url = "https://twitch.tv",
				ImageUrl = stream.PreviewLink,
				Footer = new EmbedFooterBuilder
				{
					Text = "twitch.tv",
					IconUrl = "https://www.twitch.tv/favicon.ico"
				}
			});
		}

		[Command("google")]
		public async Task GetGoogleQuery([Remainder]string query)
		{
			var listRequest = new CustomsearchService(new BaseClientService.Initializer
			{
				ApplicationName = Configuration.Default.AppName,
				ApiKey = Credentials.Default.GoogleApiKey
			})
			.Cse.List(query);
			listRequest.Cx = Credentials.Default.GoogleSearchEngineId;

			var result = listRequest.Execute()?.Items[0];
			if (result == null)
				return;

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = "Google Search",
				Url = $"https://google.com?q={query}",
				Footer = new EmbedFooterBuilder
				{
					Text = "google.com",
					IconUrl = "https://www.google.at/images/branding/product/ico/googleg_lodp.ico"
				}
			}
			.AddField(field =>
			{
				field.Name = result.Title;
				field.Value = result.Link;
			}));
		}

		[Command("evaluate"), Alias("eval")]
		public async Task Evaluate([Remainder]string expression)
		{
			var result = default(object);
			try
			{
				result = await CSharpScript.EvaluateAsync(expression);
			}
			catch (Exception e)
			{
				result = e.Message;
			}

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = "Evaluated C# Code"
			}
			.AddField(field =>
			{
				field.Name = "Result";
				field.Value = result?.ToString() ?? "**Error**";
			}),
			$"```cs\n{expression}\n```");	// Format sent message with markdown in c# style
		}

		[Command("tweet")]
		public async Task GetTweet(long id)
			=> await GetTweet(id, Context.Message);

		[Command("tweet")]
		public async Task GetTweet(Uri link)
		{
			// Check if link seems ok
			var path = link.GetLeftPart(UriPartial.Authority);
			if ((path != "https://twitter.com")
			&& (path != "https://t.co"))
				return;

			// Parse the id
			var found = link.AbsolutePath.LastIndexOf('/');
			if (!(long.TryParse(link.AbsolutePath.Substring(found + 1, link.AbsolutePath.Length - found - 1), out var id)))
				return;

			await GetTweet(id, Context.Message);
		}

		[Command("upload")]
		public async Task UploadToDropbox([Remainder]string text = "")
		{
			// I still don't know how you can have more than one attachment but whatever, handle only one at the time
			if (Context.Message.Attachments?.Count != 1)
				return;

			var file = Context.Message.Attachments.FirstOrDefault();
			var index = file.Filename.LastIndexOf('.');
			var extension = file.Filename.Substring(index, file.Filename.Length - index);

			var embed = new EmbedBuilder
			{
				Color = Data.DropboxColor,
				Title = "Attachment Found",
				Description = "Downloading file...",
				Url = "https://github.com/NeKzor"
			};
			await Message.EditAsync(Context.Message, embed, text);

			// Download
			const string cachekey = "db";
			await Fetching.GetFileAndCacheAsync(file.Url, cachekey);
			await Message.EditAsync(Context.Message, embed.WithTitle("Downloaded").WithDescription("Uploading file..."), text);

			// Upload
			var path = await Caching.CFile.GetPathAndSaveAsync(cachekey);
			var upload = await DropboxCom.UploadAsync(Configuration.Default.DropboxFolderName, file.Filename, path);
			if (!(upload))
			{
				await Message.EditAsync(Context.Message, embed.WithTitle("Error").WithDescription("Failed to upload file."), text);
				return;
			}
			await Message.EditAsync(Context.Message, embed.WithTitle("Uploaded").WithDescription("Creating link..."), text);

			// Create link
			var link = await DropboxCom.CreateLinkAsync($"{Configuration.Default.DropboxFolderName}/{file.Filename}");
			if (link == null)
			{
				await Message.EditAsync(Context.Message, embed.WithTitle("Error").WithDescription("Failed to create link."), text);
				return;
			}

			embed.AddField(field =>
			{
				field.Name = "Name";
				field.Value = file.Filename.Substring(0, index);
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "ID";
				field.Value = file.Id.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Extension";
				field.Value = extension;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Size";
				field.Value = $"{file.Size / 1024}KB";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Discord Links";
				field.Value = $"[File]({file.Url})\n[Proxy]({file.ProxyUrl})";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = $"Dropbox Link{(Credentials.Default.DropboxFolderQuery != string.Empty ? "s": string.Empty)}";
				field.Value = $"[File]({link}){(Credentials.Default.DropboxFolderQuery != string.Empty ? "\n[Folder](https://www.dropbox.com/sh/{Credentials.Default.DropboxFolderQuery})" : string.Empty)}";
			})
			.AddField(async field =>
			{
				field.IsInline = true;
				field.Name = "Is Image?";
				field.Value = (await Utils.IsImage(extension))
											? $"Height • {file.Height}px\nWidth • {file.Width}px"
											: "No";
			})
			.WithFooter(footer =>
			{
				footer.Text = "File hosted by dropbox.com";
				footer.IconUrl = "https://cfl.dropboxstatic.com/static/images/favicon-vflk5FiAC.ico";
			})
			.WithTitle("Uploaded & Shared")
			.WithDescription(string.Empty);
			await Message.EditAsync(Context.Message, embed, text);
		}

		// Removing redundancy
		private async Task GetTweet(long id, IUserMessage message)
		{
			var tweet = await Twitter.GetTweetAsync(Data.TwitterAccount, id);
			if (tweet.Response?.StatusCode != HttpStatusCode.OK)
				return;

			var text = tweet.Value.Text;
			tweet.Value.Entities.Where(entity => entity.EntityType == TwitterEntityType.Mention)
								.Cast<dynamic>()
								.Select(mention => mention.Name)
								.Cast<string>()
								.ToList()
								.ForEach(async item => text = await ReplaceMention(text, item));

			await Message.EditAsync(message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = tweet.Value.User.Name,
					Url = $"https://twitter.com/{tweet.Value.User.Name}",
					IconUrl = tweet.Value.Author.ProfileImageUrl
				},
				Color = Data.TwitterColor,
				Title = "Tweet",
				Url = $"https://twitter.com/Portal2Records/status/{id}",
				Description = text,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = "https://abs.twimg.com/favicons/favicon.ico",
					Text = "twitter.com"
				},
				Timestamp = tweet.Value.CreatedDate
			});
		}

		// Replace mentions with a link to make it look nicer
		private Task<string> ReplaceMention(string text, string mention)
			=> Task.FromResult(text.Replace($"@{mention}", $"[@{mention}](https://twitter.com/{mention})"));
	}
}