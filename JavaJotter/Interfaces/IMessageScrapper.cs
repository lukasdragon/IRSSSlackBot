using SlackNet.Events;

namespace JavaJotter.Interfaces;

public interface IMessageScrapper
{
    public Task<List<MessageEvent>> Scrape(DateTime? date);


    public Task<List<MessageEvent>> Scrape()
    {
        return Scrape(null);
    }
}