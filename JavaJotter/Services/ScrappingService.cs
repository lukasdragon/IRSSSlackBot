﻿using JavaJotter.Interfaces;
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

    public async Task Scrape(DateTime date)
    {
        var conversationListResponse = await _slackClient.Conversations.List();

        var messageEvents = new List<MessageEvent>();

        foreach (var channel in conversationListResponse.Channels)
        {
            var messages = await GetMessages(channel);

            messages.Reverse();

            messageEvents.AddRange(messages);
        }

        MessagesScraped?.Invoke(this, new MessagesScrapedArgs(messageEvents));
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

        return messageEvents;
    }
}