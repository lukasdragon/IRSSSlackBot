using SlackNet;
using SlackNet.Events;
namespace JavaJotter.EventHandlers;

internal class MessageHandler : IEventHandler<MessageEvent>
{
    private readonly ISlackApiClient _slack;
    public MessageHandler(ISlackApiClient slack)
    {
        _slack = slack;
    }

    public async Task Handle(MessageEvent slackEvent)
    {


        Console.WriteLine($"Received {slackEvent.Text} from {(await _slack.Users.Info(slackEvent.User)).Name} in the {(await _slack.Conversations.Info(slackEvent.Channel)).Name} channel");

    }
}
