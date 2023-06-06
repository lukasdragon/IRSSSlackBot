using SlackNet;
using SlackNet.Events;
public class UserTypingHandler : IEventHandler<UserTyping>
{
    private readonly ISlackApiClient _slack;
    public UserTypingHandler(ISlackApiClient slack)
    {
        _slack = slack;
    }


    public async Task Handle(UserTyping slackEvent)
    {
        Console.WriteLine($"User {(await _slack.Users.Info(slackEvent.User)).Name} is typing.");
    }
}
