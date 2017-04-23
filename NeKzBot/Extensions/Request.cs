using System.Net.Http;

namespace NeKzBot.Extensions
{
	public sealed class Request
	{
		public HttpMethod Method { get; }
		public string Endpoint { get; }

		public Request(HttpMethod method, string endpoint)
		{
			Method = method;
			Endpoint = endpoint;
		}
	}
}