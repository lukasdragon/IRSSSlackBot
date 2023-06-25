using System.Text;
using JavaJotter.Interfaces;
using JavaJotter.Types;

namespace JavaJotter.Services;

public class MockChannelService : IChannelService
{
    public async Task<Channel?> GetChannel(string id)
    {
        var random = new Random(StringToSeed(id));

        var channel = new Channel(id, $"Mock Channel #{random.Next(100)}");
        return channel;
    }


    private static int StringToSeed(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);

        return bytes.Aggregate(0, (current, b) => current * 31 + b);
    }
}