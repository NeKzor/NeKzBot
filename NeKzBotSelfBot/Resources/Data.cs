using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using TweetSharp;
using NeKzBot.Classes;
using NeKzBot.Server;

namespace NeKzBot.Resources
{
	public static class Data
	{
		public static Color BasicColor { get; private set; }
		public static Color BoardColor { get; private set; }
		public static Color SteamColor { get; private set; }
		public static Color TwitchColor { get; private set; }
		public static Color DropboxColor { get; private set; }
		public static Color TwitterColor { get; private set; }
		public static Color SpeedruncomColor { get; private set; }
		public static List<Portal2Map> Portal2Maps { get; private set; }
		public static TwitterService TwitterAccount { get; private set; }

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Data", LogColor.Init);
			SteamColor = new Color(22, 26, 33);
			BoardColor = new Color(4, 128, 165);
			BasicColor = new Color(14, 186, 83);
			DropboxColor = new Color(0, 126, 229);
			TwitchColor = new Color(100, 65, 164);
			TwitterColor = new Color(29, 161, 242);
			SpeedruncomColor = new Color(229, 227, 87);

			// Parse Portal 2 data
			var temp = await Utils.ReadDataAsync(await Utils.GetPath() + "/Data/Private/p2maps.dat") as object[];
			Portal2Maps = new List<Portal2Map>(temp.Length);
			foreach (string[] item in temp)
			{
				Portal2Maps.Add(new Portal2Map()
				{
					BestTimeId = item[0],
					BestPortalsId = item[1],
					ChallengeModeName = item[2],
					Name = item[3],
					ElevatorTiming = item[4],
					ThreeLetterCode = item[5]
				});
			}

			TwitterAccount = await Twitter.Account.CreateServiceAsync(Credentials.Default.TwitterConsumerKey,
																	  Credentials.Default.TwitterConsumerSecret,
																	  Credentials.Default.TwitterAppToken,
																	  Credentials.Default.TwitterAppTokenSecret);
		}
	}
}