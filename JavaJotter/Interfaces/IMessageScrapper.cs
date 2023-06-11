using SlackNet.Events;

namespace JavaJotter.Interfaces;

public interface IMessageScrapper
{
    public event EventHandler<MessagesScrapedArgs>? MessagesScraped;

    public Task Scrape(DateTime date);


    public void Scrape()
    {
        Scrape(DateTime.MinValue);
    }
}

public class MessagesScrapedArgs
{
    public MessagesScrapedArgs(List<MessageEvent> messages)
    {
        ScrappedMessages = messages;
    }

    public List<MessageEvent> ScrappedMessages { get; }
}