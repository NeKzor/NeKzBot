using System.Threading.Tasks;
using TweetSharp;
using NeKzBot.Server;

namespace NeKzBot.Modules.Twitter
{
	public class Twitter
	{
		public const int tweetLimit = 140;

		public static async Task SendTweet(TwitterService tws, string msg)
		{
			var api = await tws.SendTweetAsync(new SendTweetOptions { Status = msg });
			if (api.Response.StatusCode == System.Net.HttpStatusCode.OK)
				await Logging.CON("Tweet send", System.ConsoleColor.Green);
			else
				await Logging.CON("Twitter error " + api.Response.Error.Message, System.ConsoleColor.Red);
		}

		internal class Account
		{
			private static TwitterService twService;

			public static TwitterService CreateService(string ckey, string csecret, string token, string stoken)
			{
				twService = new TwitterService(new TwitterClientInfo
				{
					ClientName = Settings.Default.AppName,
					ClientUrl = Settings.Default.AppUrl,
					ClientVersion = Settings.Default.AppVersion,
					ConsumerKey = ckey,
					ConsumerSecret = csecret
				});
				twService.AuthenticateWith(token, stoken);
				return twService;
			}
		}
	}
}