﻿using System.Net.Http;
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
			//Logger.SendAsync($"{Method} {Endpoint}", LogColor.Default).GetAwaiter().GetResult();
		}
	}

	public static class CustomRequest
	{
		public static Request SendMessage(ulong id)
			=> new Request(HttpMethod.Post, Endpoint.CreateMessage(id));

		public static Request AddReaction(ulong channel, ulong message, string emoji)
			=> new Request(HttpMethod.Put, Endpoint.CreateReaction(channel, message, emoji));
	}

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

	public static class Endpoint
	{
		public static string CreateMessage(ulong id)
			=> $"channels/{id}/messages";

		public static string CreateReaction(ulong channel, ulong message, string emoji)
			=> $"/channels/{channel}/messages/{message}/reactions/{emoji}/@me";
	}
}