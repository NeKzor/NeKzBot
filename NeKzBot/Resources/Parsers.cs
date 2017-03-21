using System;
using System.Collections.Generic;
using NeKzBot.Classes;
using NeKzBot.Internals;
using NeKzBot.Webhooks;

namespace NeKzBot.Resources
{
	public static class Parsers
	{
		public static Func<string, object> CrossParser = file =>
		{
			var temp = Utils.ReadFromFileAsync(file).GetAwaiter().GetResult();
			if (temp.GetType() == typeof(string[]))
			{
				var simple = new Simple();
				foreach (var item in temp as string[])
					simple.Value.Add(item);
				return simple;
			}
			if (temp.GetType() == typeof(string[,]))
			{
				var complex = new Complex();
				for (int i = 0; i < (temp as string[,]).GetLength(0); i++)
				{
					var row = new Simple();
					for (int j = 0; j < (temp as string[,]).GetLength(1); j++)
						row.Value.Add((temp as string[,])[i, j]);
					complex.Values.Add(row);
				}
				return complex;
			}
			return null;
		};

		public static Func<string, object> SimpleParser = file =>
		{
			var temp = Utils.ReadFromFileAsync(file).GetAwaiter().GetResult() as string[];
			var simple = new Simple();
			foreach (var item in temp)
				simple.Value.Add(item);
			return simple;
		};

		public static Func<string, object> ComplexParser = file =>
		{
			var temp = Utils.ReadFromFileAsync(file).GetAwaiter().GetResult() as string[,];
			var complex = new Complex();
			for (int i = 0; i < temp.GetLength(0); i++)
			{
				var row = new Simple();
				for (int j = 0; j < temp.GetLength(1); j++)
					row.Value.Add(temp[i, j]);
				complex.Values.Add(row);
			}
			return complex;
		};

		public static Func<string, object> Portal2MapListParser = file =>
		{
			var maps = Utils.ReadFromFileAsync(file).GetAwaiter().GetResult() as string[,];
			var list = new List<Portal2Map>();
			for (int i = 0; i < maps.GetLength(0); i++)
			{
				list.Add(new Portal2Map
				{
					BestTimeId = maps[i, 0],
					BestPortalsId = maps[i, 1],
					ChallengeModeName = maps[i, 2],
					Name = maps[i, 3],
					ElevatorTiming = maps[i, 4],
					ThreeLetterCode = maps[i, 5],
					Filter = maps[i, 6] == "SP"
										? MapFilter.SinglePlayer
										: maps[i, 6] == "MP"
													 ? MapFilter.MultiPlayer
													 : MapFilter.Any
				});
			}
			return new Portal2Maps(list);
		};

		public static Func<string, object> WebhookDataParser = file =>
		{
			var list = new List<WebhookData>();
			var temp = Utils.ReadFromFileAsync(file).GetAwaiter().GetResult() as string[,];
			for (int i = 0; i < temp.GetLength(0); i++)
			{
				list.Add(new WebhookData
				{
					Id = ulong.Parse(temp[i, 0]),
					Token = temp[i, 1],
					GuildId = ulong.Parse(temp[i, 2]),
					UserId = ulong.Parse(temp[i, 3])
				});
			}
			return new Subscribers(list);
		};
	}
}