using System;
using System.Collections;
using System.Collections.Generic;
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

		public Task<Portal2Map> Search(string value)
		{
			var index = -1;
			if ((index = Maps.FindIndex(map => string.Equals(map.BestTimeId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
				if ((index = Maps.FindIndex(map => string.Equals(map.BestPortalsId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
					if ((index = Maps.FindIndex(map => string.Equals(map.Name, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						if ((index = Maps.FindIndex(map => string.Equals(map.ChallengeModeName, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
							if ((index = Maps.FindIndex(map => string.Equals(map.ThreeLetterCode, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
								return Task.FromResult(default(Portal2Map));
			return Task.FromResult(Maps[index]);
		}
	}
}