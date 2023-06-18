using JavaJotter.Interfaces;
using SlackNet;
using SlackNet.Events;

namespace JavaJotter.Services;

public class MockScrappingService : IMessageScrapper
{
    private static readonly Random Random = new();


    public Task<List<MessageEvent>> Scrape(DateTime date)
    {
        var messages = new List<MessageEvent>();

        const int numberOfMessages = 10;
        for (int i = 0; i < numberOfMessages; i++)
        {
            var isRoll = Random.Next(2) == 0;


            var attachments = new List<Attachment>();

            var text = "";
            var user = "";

            if (isRoll)
            {
                attachments.Add(new Attachment()
                {
                    Text = GetRollMessage()
                });
            }
            else
            {
                text = "This is a message";
                user = GetRandomUserName();
            }


            messages.Add(new MessageEvent()
            {
                Text = text,
                User = user,
                Channel = "#general",
                Attachments = attachments,
                Ts = new DateTimeOffset(DateTime.Today).ToUnixTimeSeconds().ToString()
                
            });
        }

        return Task.FromResult(messages);
    }


    private static string GetRollMessage()
    {
        var isValid = Random.Next(2) == 0;


        var notValidMessages = new[]
        {
            "JohnDoe rolled *20*",
            "<@Alice123>rolled *5*",
            "<@player8675> rolledd *100*",
            "<@ID9876> rolled 67",
            "<@RollingBot> rolled *twelve*",
            "<@> rolled *12*",
            "<@RollingBot> *12* rolled",
            "rolled *12* <@RollingBot>",
            "<@RollingBot rolled *12*",
            "<@RollingBot> rolled *12* extra text"
        };

        if (isValid)
        {
            return $"<@{GetRandomUserName()}> rolled *{Random.Next(0, 100)}*";
        }

        var index = Random.Next(notValidMessages.Length);
        return notValidMessages[index];
    }

    private static string GetRandomUserName()
    {
        var names = new[]
        {
            "LuckyLuke",
            "UnluckyLuke",
            "LukeSkywalker",
            "LukeCage",
            "LukeWarm",
            "LukeDuke",
            "LukePerry",
            "LukeWilson",
            "LukeEvans",
            "LukeHemsworth",
            "LukeBracey",
            "LukeMitchell",
            "LukeArnold",
            "LukeGrimes",
            "LukeBenward",
            "LukeMacfarlane",
            "LukePasqualino",
            "LukeTreadaway",
            "LukeNewberry",
            "LukeMably",
            "LukeYoungblood",
            "LukeGoss",
            "LukeKleintank",
            "LukeBilyk",
            "LukeMitchell",
            "LukeYoungblood",
            "LukeGoss",
            "LukeKleintank",
            "LukeBilyk",
            "LukeMitchell",
        };
        return names[Random.Next(names.Length)];
    }
    
    
}