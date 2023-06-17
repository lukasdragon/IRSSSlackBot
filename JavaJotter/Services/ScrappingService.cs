using JavaJotter.Interfaces;
using SlackNet;
using SlackNet.Events;
using ILogger = JavaJotter.Interfaces.ILogger;

namespace JavaJotter.Services;

public class ScrappingService : IMessageScrapper
{
    private readonly ILogger _logger;
    private readonly ISlackApiClient _slackClient;

    public ScrappingService(ISlackServiceProvider slackServiceProvider, ILogger logger)
    {
        _slackClient = slackServiceProvider.GetApiClient();
        _logger = logger;
    }


    public event EventHandler<MessagesScrapedArgs>? MessagesScraped;

    public async Task<List<MessageEvent>> Scrape(DateTime date)
    {
        var conversationListResponse = await _slackClient.Conversations.List();

        var messageEvents = new List<MessageEvent>();

        foreach (var channel in conversationListResponse.Channels)
        {
            if (!channel.IsMember)
            {
                continue;
            }
            
            _logger.Log($"Scraping channel: {channel.Name}...");
            
            var messages = await GetMessages(channel);

            messageEvents.AddRange(messages);
        }

        _logger.Log($"Scraping complete. Invoking event with {messageEvents.Count} messages...");
        MessagesScraped?.Invoke(this, new MessagesScrapedArgs(messageEvents));

        return messageEvents;
    }

    private async Task<List<MessageEvent>> GetMessages(Conversation conversation)
    {
        var messageEvents = new List<MessageEvent>();

        var latestTs = "";
        var hasMore = true;

        while (hasMore)
        {
            var history = await _slackClient.Conversations.History(conversation.Id, latestTs);
            messageEvents.AddRange(history.Messages);
            latestTs = history.Messages.Last().Ts;
            hasMore = history.HasMore;
        }

        _logger.Log($"Scraped {messageEvents.Count} messages from {conversation.Name}.");
        return messageEvents;
    }
}