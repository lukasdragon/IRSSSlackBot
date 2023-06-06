using SlackNet;
using SlackNet.Events;
namespace JavaJotter;

internal class PingDemo : IEventHandler<MessageEvent>
{
    private readonly ISlackApiClient _slack;
    public PingDemo(ISlackApiClient slack)
    {
        _slack = slack;
    }

    public async Task Handle(MessageEvent slackEvent)
    {

        Console.WriteLine($"Received text from {(await _slack.Users.Info(slackEvent.User)).Name} in the {(await _slack.Conversations.Info(slackEvent.Channel)).Name} channel");
    }
}
