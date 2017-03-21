using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeKzBot.Utilities
{
	public static partial class Utils
	{
		private static readonly Random _rand = new Random(DateTime.Now.Millisecond);
		private static int _temp = -1;

		private const int _luckynumber = 7;

		public static Task<int> Rng(int from, int to)
		{
			var numb = default(int);
			lock (new object())
			{
				do
					numb = _rand.Next(from, to);
				while ((numb == _temp) || ((numb != 0) && ((numb % _luckynumber) == 0)));
			}
			return Task.FromResult(_temp = numb);
		}

		public static async Task<int> RngAsync(int to)
			=> await Rng(0, to);

		public static async Task<T> RngAsync<T>(IEnumerable<T> collection)
			=> collection.ToList()[await Rng(0, collection.ToList().Count)];

		public static async Task<string> RngStringAsync(params string[] s)
			=> s[await Rng(0, s.Length)];

		public static async Task<string> RngStringAsync(IEnumerable<string> collection)
			=> collection.ToList()[await Rng(0, collection.ToList().Count)];
	}
}