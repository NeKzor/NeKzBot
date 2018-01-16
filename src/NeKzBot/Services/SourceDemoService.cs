using System;
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

		private readonly IConfiguration _config;
		private readonly LiteDatabase _dataBase;
		private WebClient _client;
		private SourceParser _parser;

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
			return Task.CompletedTask;
		}

		public async Task<bool> DownloadNewDemoAsync(ulong userId, string demoLink)
		{
			var (success, result) = await _client.TryGetBytesAsync(demoLink);
			if (success)
			{
				try
				{
					var demo = await _parser.ParseContentAsync(result);
					await SaveDemo(userId, demo, demoLink);
					return true;
				}
				catch (Exception ex)
				{
					await LogException(ex);
				}
			}
			return false;
		}
		public Task SaveDemo(ulong userId, SourceDemo demo, string downloadUrl = default)
		{
			var db = _dataBase
				.GetCollection<SourceDemoData>(nameof(SourceDemoService));
			var data = db
				.FindOne(d => d.UserId == userId);
			
			db.Upsert(data = new SourceDemoData()
			{
				UserId = userId,
				DownloadUrl = downloadUrl,
				Demo = demo
			});
			return Task.CompletedTask;
		}
		public Task<SourceDemo> GetDemo(ulong userId)
		{
			var data = _dataBase
				.GetCollection<SourceDemoData>(nameof(SourceDemoService))
				.FindOne(d => d.UserId == userId);
			return Task.FromResult(data?.Demo);
		}
		public Task<SourceDemoData> GetDemoData(ulong userId)
		{
			var data = _dataBase
				.GetCollection<SourceDemoData>(nameof(SourceDemoService))
				.FindOne(d => d.UserId == userId);
			return Task.FromResult(data);
		}
		
		internal Task DeleteExpiredDemos()
		{
			var db = _dataBase
				.GetCollection<SourceDemoData>(nameof(SourceDemoService));
			
			foreach (var demo in db.FindAll())
			{
				if ((DateTime.UtcNow - demo.CreatedAt).Days > 21)
				{
					db.Delete(demo.Id);
					_ = LogWarning($"Deleted expired demo from user {demo.UserId}");
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