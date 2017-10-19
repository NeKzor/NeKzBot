using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;

namespace NeKzBot.Services
{
	public enum CvarGameType
	{
		HalfLife2,
		Portal,
		Portal2
	}

	public class SourceCvarService
	{
		private readonly IConfiguration _config;
		private readonly LiteDatabase _dataBase;

		public SourceCvarService(IConfiguration config, LiteDatabase dataBase)
		{
			_config = config;
			_dataBase = dataBase;
		}

		public Task Initialize()
		{
			return Task.CompletedTask;
		}

		public Task<SourceCvarData> LookUpCvar(string cvar, CvarGameType type = CvarGameType.Portal2)
		{
			var db = _dataBase.GetCollection<SourceCvarData>();
			var data = db.FindOne(d => d.Cvar == cvar);
			return Task.FromResult(data);
		}
	}
}