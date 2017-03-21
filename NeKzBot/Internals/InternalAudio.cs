using Discord.Audio;

namespace NeKzBot.Internals
{
	public sealed class InternalAudio
	{
		public bool Connected { get; set; }             // Only connect when not in VC
		public bool IsPlaying { get; set; }             // Prevent playing multiple streams at once
		public bool ShouldStop { get; set; }            // Stop music, needed when playing long audio tracks
		public IAudioClient AudioClient { get; set; }

		public InternalAudio(IAudioClient client)
		{
			Connected = false;
			IsPlaying = false;
			ShouldStop = false;
			AudioClient = client;
		}
	}
}