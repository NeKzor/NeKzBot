using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NeKzBot.Webhooks;

namespace NeKzBot.Internals.Entities
{
	[JsonObject("subscription")]
	public sealed class Subscription : IMemory, IEnumerable<WebhookData>
	{
		[JsonProperty("subs")]
		public List<WebhookData> Subscribers { get; set; }

		public Subscription()
			=> Subscribers = new List<WebhookData>();
		public Subscription(List<WebhookData> subs)
			=> Subscribers = subs;

		public IEnumerable<object> Values => Subscribers;

		public IEnumerator<WebhookData> GetEnumerator()
			=> Subscribers.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}