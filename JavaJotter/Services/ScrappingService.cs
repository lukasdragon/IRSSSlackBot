using JavaJotter.Interfaces;
using SlackNet;
using SlackNet.Events;
using ILogger = JavaJotter.Interfaces.ILogger;
namespace JavaJotter.Services;

public class ScrappingService : IScrapper
{
    private readonly ILogger _logger;
    private ISlackApiClient _slackClient;

    public ScrappingService(ISlackServiceProvider slackServiceProvider, ILogger logger)
    {
        _slackClient = slackServiceProvider.GetApiClient();
        _logger = logger;
    }


    public async Task Scrape(DateTime date)
    {
        var conversationListResponse = await _slackClient.Conversations.List();


        foreach (var channel in conversationListResponse.Channels)
        {

            var messages = await GetMessages(channel);


            foreach (var messageEvent in messages)
                _logger.Log(messageEvent);

        }
    }

    public async Task<List<MessageEvent>> GetMessages(Conversation conversation)
    {
        var messageEvents = new List<MessageEvent>();

        var history = await _slackClient.Conversations.History(conversation.Id);

        messageEvents.AddRange(history.Messages);


        if (history.HasMore)
            _logger.Log($"There are more messages in {conversation.Name}");

        return messageEvents;

    }
}
