using System.Net;
using System.Threading.Tasks;
using TweetSharp;
using NeKzBot.Server;

namespace NeKzBot.Classes
{
	public static class Twitter
	{
		public const int TweetLimit = 140;

		public static async Task<TwitterAsyncResult<TwitterStatus>> GetTweetAsync(TwitterService service, long id)
		{
			var api = await service.GetTweetAsync(new GetTweetOptions { Id = id });

			if (api.Response?.StatusCode == HttpStatusCode.OK)
				await Logger.SendAsync("Tweet Has Been Fetched", LogColor.Twitter);
			else
				await Logger.SendAsync("Twitter.GetTweetAsync Error\n" + api.Response?.Error.Message, LogColor.Error);
			return api;
		}

		internal static class Account
		{
			private static TwitterService _service;

			public static async Task<TwitterService> CreateServiceAsync(string ckey, string csecret, string token, string stoken)
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
				_service.AuthenticateWith(token, stoken);
				return _service;
			}
		}
	}
}