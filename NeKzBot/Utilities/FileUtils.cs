using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Webhooks;

namespace NeKzBot.Utilities
{
	public static partial class Utils
	{
		public const char DataSeparator = '|';
		private const int _maxarraycount = 128;

		public static async Task<T> ReadJson<T>(string file)
			where T : class, new()
		{
			var path = Path.Combine(await GetAppPath(), Configuration.Default.DataPath, file);
			if (File.Exists(path))
			{
				try
				{
					return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
				}
				catch (Exception e)
				{
					await Logger.SendAsync("Utils.ReadJson Error", e);
				}
			}
			return default(T);
		}

		public static async Task<bool> WriteJson(object obj, string file)
		{
			var path = Path.Combine(await GetAppPath(), Configuration.Default.DataPath, file);
			try
			{
				File.WriteAllText(path, JsonConvert.SerializeObject(obj));
				return true;
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Utils.WriteJson Error", e);
			}
			return false;
		}

		public static async Task<DataChangeResult> ChangeDataAsync(string name, string values, DataChangeMode mode)
		{
			switch (mode)
			{
				case DataChangeMode.Add:
					return await AddDataAsync(name, values);
				case DataChangeMode.Delete:
					return await RemoveDataAsync(name, values);
			}
			return default(DataChangeResult);
		}

		public static async Task<DataChangeResult> AddDataAsync(string name, string values)
		{
			if (await Data.Get(name) is IData data)
			{
				if (data.ReadWriteAllowed)
				{
					var memory = data.Memory;
					if (memory is Simple simple)
					{
						if (values.Contains(DataSeparator))
							return DataChangeResult.InvalidInput;
						else
						{
							if (simple.Search(values) != null)
								return DataChangeResult.Dupulicate;
							else
							{
								simple.Value.Add(values);
								await Data.ChangeAsync<Simple>(name, simple);
								if (await Data.ExportAsync<Simple>(name))
									return DataChangeResult.Success;
								else
									return DataChangeResult.ExportFailed;
							}
						}
					}
					else if (memory is Complex complex)
					{
						if (!(values.Contains(DataSeparator)))
							return DataChangeResult.SeparatorNotFound;
						else
						{
							if (complex.Values.Find(temp => temp.Value.FirstOrDefault() == values.Split('|').First()) != null)
								return DataChangeResult.Dupulicate;
							else
							{
								var temp = values.Split('|').ToList();
								if (temp.Count != complex.Values.First().Value.Count)
									return DataChangeResult.InvalidCount;
								else
								{
									complex.Values.Add(new Simple(temp));
									await Data.ChangeAsync<Complex>(name, complex);
									if (await Data.ExportAsync<Complex>(name))
										return DataChangeResult.Success;
									else
										return DataChangeResult.ExportFailed;
								}
							}
						}
					}
					else if (memory is Subscription subscription)
					{
						var temp = values.Split(DataSeparator).ToList();
						if (temp?.Count == 4)
						{
							if ((ulong.TryParse(temp[0], out var id))
							&& (ulong.TryParse(temp[2], out var guildid))
							&& (ulong.TryParse(temp[3], out var userid))
							&& !(string.IsNullOrEmpty(temp[1])))
							{
								if (subscription.Subscribers.Find(sub => sub.Id == id) != null)
									return DataChangeResult.Dupulicate;
								else
								{
									subscription.Subscribers.Add(new WebhookData
									{
										Id = id,
										Token = temp[1],
										GuildId = guildid,
										UserId = userid
									});
									await Data.ChangeAsync<Subscription>(name, subscription);
									if (await Data.ExportAsync<Subscription>(name))
										return DataChangeResult.Success;
									else
										return DataChangeResult.ExportFailed;
								}
							}
							else
								return DataChangeResult.IncorrectType;
						}
						else
							return DataChangeResult.IncorrectValues;
					}
					else
						return DataChangeResult.NotImplemented;
				}
				else
					return DataChangeResult.NotAllowed;
			}
			else
				return DataChangeResult.NameNotFound;
		}

		public static async Task<DataChangeResult> RemoveDataAsync(string name, string value)
		{
			if (await Data.Get(name) is IData data)
			{
				if (data.ReadWriteAllowed)
				{
					var memory = data.Memory;
					if (memory is Simple simple)
					{
						if (simple.Search(value) == null)
							return DataChangeResult.NoMatch;
						else
						{
							simple.Value.Remove(value);
							await Data.ChangeAsync<Simple>(name, simple);
							if (await Data.ExportAsync<Simple>(name))
								return DataChangeResult.Success;
							else
								return DataChangeResult.ExportFailed;
						}
					}
					else if (memory is Complex complex)
					{
						var found = complex.Values.Find(temp => temp.Value.FirstOrDefault() == value);
						if (found == null)
							return DataChangeResult.NoMatch;
						else
						{
							complex.Values.Remove(found);
							await Data.ChangeAsync<Complex>(name, complex);
							if (await Data.ExportAsync<Complex>(name))
								return DataChangeResult.Success;
							else
								return DataChangeResult.ExportFailed;
						}
					}
					else if (memory is Subscription subscription)
					{
						if (ulong.TryParse(value, out var id))
						{
							var found = subscription.Subscribers.Find(sub => sub.Id == id);
							if (found == null)
								return DataChangeResult.NoMatch;
							else
							{
								subscription.Subscribers.Remove(found);
								await Data.ChangeAsync<Subscription>(name, subscription);
								if (await Data.ExportAsync<Subscription>(name))
									return DataChangeResult.Success;
								else
									return DataChangeResult.ExportFailed;
							}
						}
						else
							return DataChangeResult.IncorrectType;
					}
					else
						return DataChangeResult.NotImplemented;
				}
				else
					return DataChangeResult.NotAllowed;
			}
			else
				return DataChangeResult.NameNotFound;
		}
	}

	public enum DataChangeResult
	{
		Error,
		Success,
		InvalidInput,
		Dupulicate,
		ExportFailed,
		SeparatorNotFound,
		IncorrectType,
		IncorrectValues,
		NotAllowed,
		NotImplemented,
		NameNotFound,
		NoMatch,
		InvalidCount
	}
}