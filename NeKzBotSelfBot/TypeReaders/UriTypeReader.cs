using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace NeKzBot.TypeReaders
{
	public class UriTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input)
			=> ((Uri.IsWellFormedUriString(input, UriKind.Absolute))
			&& (Uri.TryCreate(input, UriKind.Absolute, out Uri result))
			&& (Uri.CheckSchemeName(result.Scheme)))
						? Task.FromResult(TypeReaderResult.FromSuccess(result))
						: Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a uri."));
	}
}