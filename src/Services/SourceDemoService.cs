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
		private readonly IConfiguration _config;
		private readonly LiteDatabase _dataBase;
		private string _defaultDemoPath;
		private WebClient _client;
		private SourceParser _parser;

		public SourceDemoService(IConfiguration config, LiteDatabase dataBase)
		{
			_config = config;
			_dataBase = dataBase;
		}

		public Task Initialize(string demoPath = null)
		{
			_defaultDemoPath = demoPath ?? @"demos\\";
			_client = new WebClient(_config["user_agent"]);
			_parser = new SourceParser(true);
			SourceExtensions.DiscoverAsync();
			return Task.CompletedTask;
		}

		public async Task<bool> GetNewDemoAsync(ulong userId, string demoLink)
		{
			var download = await _client.TryGetBytesAsync(demoLink);
			if (download.Sucess)
			{
				try
				{
					var demo = await _parser.ParseContentAsync(download.Result);
					await SaveDemo(userId, demo, demoLink);
					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			return false;
		}

		public Task SaveDemo(ulong userId, SourceDemo demo, string downloadUrl = default)
		{
			var db = _dataBase.GetCollection<SourceDemoData>();
			var data = db.FindOne(d => d.Id == userId) ?? new SourceDemoData { Id = userId };
			data.Demo = demo;
			data.DownloadUrl = downloadUrl;
			db.Upsert(data);
			return Task.CompletedTask;
		}

		public Task<SourceDemo> GetDemo(ulong userId)
		{
			var db = _dataBase.GetCollection<SourceDemoData>();
			var data = db.FindOne(d => d.Id == userId);
			return Task.FromResult(data?.Demo);
		}

		public Task<SourceDemoData> GetDemoData(ulong userId)
		{
			var db = _dataBase.GetCollection<SourceDemoData>();
			var data = db.FindOne(d => d.Id == userId);
			return Task.FromResult(data);
		}
	}
}