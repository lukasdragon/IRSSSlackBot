using JavaJotter.Interfaces;
using SlackNet;
using SlackNet.Events;
using ILogger = JavaJotter.Interfaces.ILogger;

namespace JavaJotter.Services;

public class SlackScrappingService : IMessageScrapper
{
    private readonly ILogger _logger;
    private readonly ISlackApiClient _slackClient;

    public SlackScrappingService(ISlackServiceProvider slackServiceProvider, ILogger logger)
    {
        _slackClient = slackServiceProvider.GetApiClient();
        _logger = logger;
    }


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

        return messageEvents;
    }

    private async Task<List<MessageEvent>> GetMessages(Conversation conversation, DateTime? oldest = null)
    {
        var oldestTs = "";

        if (oldest != null)
        {
            var dateTimeOffset = new DateTimeOffset(oldest.Value);
            oldestTs = dateTimeOffset.ToUnixTimeSeconds().ToString();
        }
        
        var messageEvents = new List<MessageEvent>();

        var latestTs = "";
        var hasMore = true;

        while (hasMore)
        {
            var history = await _slackClient.Conversations.History(conversation.Id, latestTs, oldestTs);
            messageEvents.AddRange(history.Messages);
            latestTs = history.Messages.LastOrDefault()?.Ts;
            hasMore = history.HasMore;
        }

        _logger.Log($"Scraped {messageEvents.Count} messages from {conversation.Name}.");
        return messageEvents;
    }
}