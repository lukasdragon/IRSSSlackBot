using System.Configuration;
using Autofac;
using Config.Net;
using JavaJotter.Configuration.Interfaces;
using JavaJotter.Interfaces;
using JavaJotter.Services;
using JavaJotter.Services.Databases;
using JavaJotter.Types;
using SlackNet;
using SlackNet.Autofac;
using ILogger = JavaJotter.Interfaces.ILogger;

namespace JavaJotter;

public static class Program
{
    private static readonly CancellationTokenSource CancellationToken = new();
    private static ISlackSocketModeClient? _client;
    private static IContainer? _container;

    private static bool _onlineMode = true;

    private static async Task Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "--offline", StringComparison.OrdinalIgnoreCase))
            _onlineMode = false;
        else
            _onlineMode = true;

        _container = BuildContainer();

        var logger = _container.Resolve<ILogger>();

        logger.Log("Connecting...");

        if (_onlineMode)
        {
            _client = _container.SlackServices().GetSocketModeClient();
            await _client.Connect();
            logger.Log("Connected. Waiting for events...");
        }
        else
        {
            logger.Log("Offline mode. Ready.");
        }

        Scrape();

        await MaintainLoop(logger);
    }

    private static IAppAuthSettings RetrieveAuthSettings()
    {
        var settings = new ConfigurationBuilder<IAppAuthSettings>()
            .UseYamlFile("token.yaml").Build();

        return settings.OAuthToken == string.Empty
            ? throw new ConfigurationErrorsException("OAuthToken is empty. Please add it to token.yaml")
            : settings;
    }

    private static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<ConsoleLoggingService>().As<ILogger>().SingleInstance();

        var settings = RetrieveAuthSettings();

        builder.Register(c => settings).As<IAppAuthSettings>().SingleInstance();

        if (_onlineMode)
            builder.AddSlackNet(c => c.UseApiToken(settings.OAuthToken).UseAppLevelToken(settings.AppLevelToken)
                //Register our slack events here
                //   .RegisterEventHandler<MessageEvent, MessageHandler>()
            );


        if (_onlineMode)
        {
            builder.RegisterType<SlackScrappingService>().As<IMessageScrapper>();
            builder.RegisterType<SlackUsernameService>().As<IUsernameService>();
            builder.RegisterType<SlackChannelService>().As<IChannelService>();
        }
        else
        {
            builder.RegisterType<MockScrappingService>().As<IMessageScrapper>();
            builder.RegisterType<MockUsernameService>().As<IUsernameService>();
            builder.RegisterType<MockChannelService>().As<IChannelService>();
        }

        builder.RegisterType<RollFilter>().As<IRollFilter>();
        builder.RegisterType<PostgresDatabaseService>().As<IDatabaseConnection>();


        return builder.Build();
    }

    private static async void Scrape()
    {
        await using var scope = _container.BeginLifetimeScope();

        var scrapper = scope.Resolve<IMessageScrapper>();
        var logger = scope.Resolve<ILogger>();
        var rollFilter = scope.Resolve<IRollFilter>();
        var usernameService = scope.Resolve<IUsernameService>();
        var channelService = scope.Resolve<IChannelService>();

        var databaseConnection = scope.Resolve<IDatabaseConnection>();


        var lastRoll = await databaseConnection.GetLastScrape();

        var lastScrape = lastRoll?.DateTime;

        logger.Log(
            $"Last scrape: {(lastScrape.HasValue ? lastScrape.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never")}");

        var messages = await scrapper.Scrape(lastScrape);

        List<Roll> rolls = new();
        foreach (var roll in messages.Select(message => rollFilter.ProcessMessage(message)))
        {
            if (roll == null)
                continue;
            logger.Log($"Found roll: {roll}");
            rolls.Add(roll);
        }


        logger.Log($"Found {rolls.Count} rolls");

        foreach (var roll in rolls)
        {
            logger.Log(roll);
            await databaseConnection.InsertRoll(roll);
        }

        var nullUsernames = await databaseConnection.GetNullUsernames();
        foreach (var username in nullUsernames)
        {
            logger.Log($"Getting username for {username.Id}");
            var user = await usernameService.GetUsername(username.Id);
            if (user != null)
            {
                logger.Log($"Got username for {user.Id}: {user.Name}");
                await databaseConnection.UpdateUsername(user);
            }
        }

        var nullChannels = await databaseConnection.GetNullChannels();
        foreach (var channel in nullChannels)
        {
            logger.Log($"Getting channel for {channel.Id}");
            var channelInfo = await channelService.GetChannel(channel.Id);
            if (channelInfo != null)
                await databaseConnection.UpdateChannel(channelInfo);
        }
    }

    private static async Task MaintainLoop(ILogger logger)
    {
        Console.CancelKeyPress += delegate(object? _, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            CancellationToken.Cancel();
        };

        try
        {
            await Task.Delay(-1, CancellationToken.Token);
        }
        catch (TaskCanceledException)
        {
            logger.Log("Exiting...");
        }
    }
}