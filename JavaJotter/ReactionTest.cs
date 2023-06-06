using SlackNet;
using SlackNet.Events;
public class ReactionTest : IEventHandler<ReactionAdded>
{
    private readonly ISlackApiClient _slack;
    public ReactionTest(ISlackApiClient slack)
    {
        _slack = slack;
    }


    public async Task Handle(ReactionAdded slackEvent)
    {
        Console.WriteLine($"Received reaction from {(await _slack.Users.Info(slackEvent.User)).Name}.");
    }
}
