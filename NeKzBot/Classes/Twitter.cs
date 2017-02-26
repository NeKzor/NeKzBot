using System.Net;
using System.Threading.Tasks;
using TweetSharp;
using NeKzBot.Server;

namespace NeKzBot.Classes
{
	public static class Twitter
	{
		public const int TweetLimit = 140;

		public static async Task<TwitterAsyncResult<TwitterStatus>> SendTweetAsync(TwitterService service, string status)
		{
			var api = await service.SendTweetAsync(new SendTweetOptions { Status = status });

			if (api.Response?.StatusCode == HttpStatusCode.OK)
				await Logger.SendAsync("Tweet Has Been Sent", LogColor.Twitter);
			else
				await Logger.SendAsync("Twitter.SendTweetAsync Error\n" + api.Response?.Error.Message, LogColor.Error);
			return api;
		}

		public static async Task<TwitterAsyncResult<TwitterStatus>> SendReplyAsync(TwitterService service, string status, long id)
		{
			var api = await service.SendTweetAsync(new SendTweetOptions
			{
				Status = status,
				InReplyToStatusId = id
			});

			if (api.Response?.StatusCode == HttpStatusCode.OK)
				await Logger.SendAsync("Tweet Replay Has Been Sent", LogColor.Twitter);
			else
				await Logger.SendAsync("Twitter.SendReplyAsync Error\n" + api.Response?.Error.Message, LogColor.Error);
			return api;
		}

		public static async Task<TwitterAsyncResult<TwitterUser>> UpdateDescriptionAsync(TwitterService service, string description)
		{
			var api = await service.UpdateProfileAsync(new UpdateProfileOptions { Description = description });

			if (api.Response?.StatusCode == HttpStatusCode.OK)
				await Logger.SendAsync("Updated Twitter Description", LogColor.Twitter);
			else
				await Logger.SendAsync("Twitter.UpdateDescriptionAsync Error\n" + api.Response?.Error.Message, LogColor.Error);
			return api;
		}

		public static async Task<TwitterAsyncResult<TwitterUser>> UpdateLocationAsync(TwitterService service, string location)
		{
			var api = await service.UpdateProfileAsync(new UpdateProfileOptions { Location = location });

			if (api.Response?.StatusCode == HttpStatusCode.OK)
				await Logger.SendAsync("Updated Twitter Location", LogColor.Twitter);
			else
				await Logger.SendAsync("Twitter.UpdateLocationAsync Error\n" + api.Response?.Error.Message, LogColor.Error);
			return api;
		}

		internal static class Account
		{
			private static TwitterService _service;

			public static async Task<TwitterService> CreateServiceAsync(string ckey, string csecret, string token = null, string stoken = null)
			{
				await Logger.SendAsync("Creating Twitter Client", LogColor.Twitter);
				_service = new TwitterService(new TwitterClientInfo
				{
					ClientName = Configuration.Default.AppName,
					ClientUrl = Configuration.Default.AppUrl,
					ClientVersion = Configuration.Default.AppVersion,
					ConsumerKey = ckey,
					ConsumerSecret = csecret
				});
				if ((token != null)
				&& (stoken != null))
					_service.AuthenticateWith(token, stoken);
				return _service;
			}
		}
	}
}