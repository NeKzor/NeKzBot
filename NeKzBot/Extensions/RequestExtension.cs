using Discord.API;

namespace NeKzBot.Extensions
{
	public sealed class RequestExtension<ResponseClassT> : IRestRequest
	{
		public string Method { get;}
		public string Endpoint { get; }
		public object Payload { get; }

		internal RequestExtension(Request request, object payload)
		{
			Method = request.Method.Method;
			Endpoint = request.Endpoint;
			Payload = payload;
		}
	}
}