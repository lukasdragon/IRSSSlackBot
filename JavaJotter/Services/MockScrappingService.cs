using JavaJotter.Helpers;
using JavaJotter.Interfaces;
using SlackNet;
using SlackNet.Events;

namespace JavaJotter.Services;

internal class MockScrappingService : IMessageScrapper
{
    private static readonly Random Random = new();


    public Task<List<MessageEvent>> Scrape(DateTime? date)
    {
        var messages = new List<MessageEvent>();

        var timeStamp = date ?? DateTime.Now -
            new TimeSpan(0, Random.Next(24), Random.Next(60), Random.Next(60), Random.Next(1000));

        const int numberOfMessages = 100;
        for (var i = 0; i < numberOfMessages; i++)
        {
            var isRoll = Random.Next(2) == 0;

            var attachments = new List<Attachment>();


            timeStamp += new TimeSpan(0, Random.Next(1), Random.Next(30), Random.Next(60), Random.Next(1000));

            var text = "";
            var userId = "";

            if (isRoll)
            {
                attachments.Add(new Attachment
                {
                    Text = GetRollMessage()
                });
            }
            else
            {
                text = "This is a message";
                userId = MockDataHelper.GetRandomUserId();
            }


            messages.Add(new MessageEvent
            {
                ClientMsgId = Guid.NewGuid(),
                Text = text,
                User = userId,
                Channel = "#general",
                Attachments = attachments,
                Ts = timeStamp.ToTimestamp()
            });
        }

        return Task.FromResult(messages);
    }


    private static string GetRollMessage()
    {
        var isValid = Random.Next(2) == 0;

        var name = MockDataHelper.GetRandomUserId();

        var notValidMessages = new[]
        {
            $"{name} rolled *20*",
            $"<@{name}> rolledd *100*",
            $"<@{name}> rolled 67",
            $"<@{name}> rolled *twelve*",
            $"<@>{name} rolled *12*",
            $"<@{name}> *12* rolled",
            $"rolled *12* <@{name}>",
            $"<@{name}> rolled *12* extra text"
        };

        if (isValid) return $"<@{MockDataHelper.GetRandomUserId()}> rolled *{Random.Next(0, 100)}*";

        var index = Random.Next(notValidMessages.Length);
        return notValidMessages[index];
    }
}