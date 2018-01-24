using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.API;
using NeKzBot.Data;
using SourceDemoParser;
using SourceDemoParser.Extensions;

namespace NeKzBot.Services
{
	public class SourceDemoService
	{
		public event Func<string, Exception, Task> Log;

		private WebClient _client;
		private SourceParser _parser;

		// Using database to find user's SourceDemo is really expensive
		// because it has to generate the object everytime on request
		// Let's just cache everything and use the DB for saving
		private ConcurrentDictionary<ulong, SourceDemoData> _cache;

		private readonly IConfiguration _config;
		private readonly LiteDatabase _dataBase;

		public SourceDemoService(IConfiguration config, LiteDatabase dataBase)
		{
			_config = config;
			_dataBase = dataBase;
		}

		public Task Initialize(string demoPath = null)
		{
			_client = new WebClient(_config["user_agent"]);
			_parser = new SourceParser();
			SourceExtensions.DiscoverAsync();
			_cache = new ConcurrentDictionary<ulong, SourceDemoData>();

			// Load existing demos into cache
			foreach (var data in _dataBase
				.GetCollection<SourceDemoData>(nameof(SourceDemoService))
				.FindAll())
			{
				_cache.TryAdd(data.UserId, data);
			}

			return Task.CompletedTask;
		}

		public async Task<bool> DownloadDemoAsync(ulong userId, string demoLink)
		{
			var (success, result) = await _client.TryGetBytesAsync(demoLink);
			if (success)
			{
				try
				{
					var demo = await _parser.ParseContentAsync(result);
					return await SaveDemoAsync(userId, demo, demoLink);
				}
				catch (Exception ex)
				{
					await LogException(ex);
				}
			}
			return false;
		}
		public async Task<bool> SaveDemoAsync(ulong userId, SourceDemo demo, string downloadUrl = null)
		{
			var db = _dataBase
				.GetCollection<SourceDemoData>(nameof(SourceDemoService));
			
			var data = await Get(userId);
			
			if (data == null)
			{
				data = new SourceDemoData()
				{
					UserId = userId,
					DownloadUrl = downloadUrl,
					Demo = demo,
					CreatedAt = DateTime.UtcNow
				};
			}
			else
			{
				// Demo has been adjusted
				if (string.IsNullOrEmpty(downloadUrl))
					data.CreatedAt = DateTime.UtcNow;
				else
					data.DownloadUrl = downloadUrl;
				
				data.Demo = demo;
			}
			
			return db.Upsert(data)
				&& _cache.TryRemove(userId, out _)
				&& _cache.TryAdd(userId, data);
		}

		// From cache
		public Task<SourceDemoData> Get(ulong userId)
		{
			_cache.TryGetValue(userId, out var data);
			return Task.FromResult(data ?? default(SourceDemoData));
		}
		public Task<SourceDemo> GetDemo(ulong userId)
		{
			_cache.TryGetValue(userId, out var data);
			return Task.FromResult(data?.Demo ?? default(SourceDemo));
		}
		
		internal Task DeleteExpiredDemos()
		{
			var db = _dataBase
				.GetCollection<SourceDemoData>(nameof(SourceDemoService));
			
			foreach (var demo in db.FindAll())
			{
				if ((DateTime.UtcNow - demo.CreatedAt).Days > 21)
				{
					_ = LogWarning($"Deleting expired demo from user {demo.UserId}");
					if (!db.Delete(demo.Id))
						_ = LogWarning("Database failed to delete data");
				}
			}
			return Task.CompletedTask;
		}

		protected Task LogWarning(string message)
		{
			_ = Log.Invoke($"{nameof(SourceDemoData)}\t{message}!", null);
			return Task.CompletedTask;
		}
		protected Task LogException(Exception ex)
		{
			_ = Log.Invoke(nameof(SourceDemoData), ex);
			return Task.CompletedTask;
		}
	}
}