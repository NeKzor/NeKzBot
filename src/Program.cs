using System.Threading.Tasks;

namespace NeKzBot
{
    internal static class Program
    {
        private static async Task Main()
            => await new Bot().RunAsync();
    }
}
