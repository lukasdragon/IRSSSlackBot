using JavaJotter.Interfaces;
using SlackNet.Events;
namespace JavaJotter;

public static class MessageEventExtensions
{
    public static void Log(this ILogger logger, MessageEvent messageEvent)
    {

        var message = messageEvent.Attachments?.FirstOrDefault()?.Text ?? messageEvent.Text;

        string user;
        if (messageEvent.User != null && !string.IsNullOrEmpty(messageEvent.User))
            user = $"[{messageEvent.User}]";
        else
            user = "(system)";

        logger.Log($"{messageEvent.Timestamp} {user}: {message}");



    }
}
