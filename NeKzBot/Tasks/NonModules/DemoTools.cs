using System;
using System.Threading.Tasks;
using Discord;

namespace NeKzBot.Tasks.NonModules
{
	public static class DemoTools
	{
		private static readonly string _tickKeyword = "tick";
		private static readonly string _secKeyword = "sec";
		private static readonly string _tickrateSeparator = "=";
		private static readonly string _startdemosSeparator = "->";

		private const uint _limit = 100;

		public static async Task<bool> CheckTickCalculatorAsync(MessageEventArgs args)
		{
			var result = await TickrateParserAsync(args.Message.RawText);
			if (result == string.Empty)
				return false;
			await args.Channel.SendMessage(result);
			return true;
		}

		public static async Task<bool> CheckStartdemosGeneratorAsync(MessageEventArgs args)
		{
			var result = await StartdemosParserAsync(args.Message.RawText);
			if (result == "startdemos")
				return false;
			await args.Channel.SendMessage(result);
			return true;
		}

		private static async Task<string> TickrateParserAsync(string msg)
		{
			var parsed = await ParseIntegers(msg, _tickKeyword, _tickrateSeparator);
			var value = parsed.Item1;
			var result = parsed.Item2;
			if ((value > 0)
			&& (result > 0))
			{
				result = Math.Round(value / result, 3);
				return $"{value} {(value != 1 ? "ticks" : "tick")} = {result} {(result != 1 ? "seconds" : "second")}";
			}
			else
			{
				parsed = await ParseIntegers(msg, _secKeyword, _tickrateSeparator);
				value = parsed.Item1;
				result = parsed.Item2;
				if ((value > 0)
				&& (result > 0))
				{
					result = Math.Round(value * result, 3);
					return $"{value} {(value != 1 ? "seconds" : "second")} = {result} {(result != 1 ? "ticks" : "tick")}";
				}
			}
			return string.Empty;
		}

		private static async Task<string> StartdemosParserAsync(string msg)
		{
			var parsed = await ParseIntegerString(msg, string.Empty, _startdemosSeparator);
			var value = parsed.Item1;
			var name = parsed.Item2;
			var output = "startdemos";

			if ((value > 0)
			&& (value < _limit)
			&& (name != string.Empty))
			{
				for (int i = 0; i < value; i++)
				{
					var append = $" {parsed.Item2}{i}";
					if (append.Length + output.Length < 2000)
						output += append;
					else
						break;
				}
			}
			return output;
		}

		private static Task<Tuple<decimal, decimal>> ParseIntegers(string msg, string keyword, string separator)
		{
			var find = msg.IndexOf(separator + keyword);
			var tickrate = 0;
			var value = 0;

			if (find != -1)
			{
				// Set custom tickrate
				if (msg.Length != find + separator.Length + keyword.Length)
				{
					if (!(int.TryParse(msg.Substring(find + separator.Length + keyword.Length), out tickrate)))
					{
						var values = msg.Split(new string[] { separator, keyword, " " }, StringSplitOptions.None);
						var index = -1;
						foreach (var item in values)
						{
							index++;
							if (item == string.Empty)
								break;
						}

						if (index != -1)
						{
							if (!(int.TryParse(values[index + 1], out tickrate)))
								tickrate = (values[index + 1] == string.Empty)
															  ? 60
															  : 0;
						}
					}
				}
				else
					tickrate = 60;

				// Try to parse a value
				if (!(int.TryParse(msg.Substring(0, find), out value)))
				{
					var values = msg.Split(new string[] { separator, keyword, " " }, StringSplitOptions.None);
					var index = -1;
					foreach (var item in values)
					{
						index++;
						if (item == string.Empty)
							break;
					}

					if (index != -1)
						if (!(int.TryParse(values[index - 1], out value)))
							value = 0;
				}
			}
			return Task.FromResult(new Tuple<decimal, decimal>(value, tickrate));
		}

		private static Task<Tuple<int, string>> ParseIntegerString(string msg, string keyword, string separator)
		{
			var find = msg.IndexOf(separator + keyword);
			var name = string.Empty;
			var value = 0;

			if (find != -1)
			{
				// Get string name
				if (msg.Length != find + separator.Length + keyword.Length)
					name = msg.Substring(find + separator.Length + keyword.Length);

				// Try to parse a value
				if (!(int.TryParse(msg.Substring(0, find), out value)))
				{
					var values = msg.Split(new string[] { separator, " " }, StringSplitOptions.None);
					var index = -1;
					foreach (var item in values)
					{
						index++;
						if (item == separator + keyword)
							break;
					}

					if (index != -1)
						if (!(int.TryParse(values[index - 1], out value)))
							value = 0;
				}
			}
			return Task.FromResult(new Tuple<int, string>(value, name));
		}
	}
}