using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NeKzBot.Classes;

namespace NeKzBot.Internals.Entities
{
	[JsonObject("portal2maps")]
	public sealed class Portal2Maps : IMemory, IEnumerable<Portal2Map>
	{
		[JsonProperty("map_list")]
		public List<Portal2Map> Maps { get; set; }

		public Portal2Maps()
			=> Maps = new List<Portal2Map>();
		public Portal2Maps(List<Portal2Map> list)
			=> Maps = list;

		public IEnumerable<object> Values => Maps;

		public IEnumerator<Portal2Map> GetEnumerator()
			=> Maps.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public Task<List<Portal2Map>> Get(Portal2MapFilter filter)
			=> Task.FromResult(Maps.Where(m => m.Filter == filter).ToList());

		public Task<Portal2Map> Search(string value, Portal2MapsSearchBy filter = default(Portal2MapsSearchBy))
		{
			var index = -1;
			switch (filter)
			{
				case Portal2MapsSearchBy.BestTimeId:
					if ((index = Maps.FindIndex(map => string.Equals(map.BestTimeId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						return Task.FromResult(default(Portal2Map));
					break;
				case Portal2MapsSearchBy.BestPortalsId:
					if ((index = Maps.FindIndex(map => string.Equals(map.BestPortalsId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						return Task.FromResult(default(Portal2Map));
					break;
				case Portal2MapsSearchBy.Name:
					if ((index = Maps.FindIndex(map => string.Equals(map.Name, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						return Task.FromResult(default(Portal2Map));
					break;
				case Portal2MapsSearchBy.ChallengeModeName:
					if ((index = Maps.FindIndex(map => string.Equals(map.ChallengeModeName, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						return Task.FromResult(default(Portal2Map));
					break;
				case Portal2MapsSearchBy.ThreeLetterCode:
					if ((index = Maps.FindIndex(map => string.Equals(map.ThreeLetterCode, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						return Task.FromResult(default(Portal2Map));
					break;
				default:
					if ((index = Maps.FindIndex(map => string.Equals(map.BestTimeId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						if ((index = Maps.FindIndex(map => string.Equals(map.BestPortalsId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
							if ((index = Maps.FindIndex(map => string.Equals(map.Name, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
								if ((index = Maps.FindIndex(map => string.Equals(map.ChallengeModeName, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
									if ((index = Maps.FindIndex(map => string.Equals(map.ThreeLetterCode, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
										return Task.FromResult(default(Portal2Map));
					break;
			}
			return Task.FromResult(Maps[index]);
		}
	}

	public enum Portal2MapsSearchBy
	{
		Unknown,
		BestTimeId,
		BestPortalsId,
		Name,
		ChallengeModeName,
		ThreeLetterCode
	}
}