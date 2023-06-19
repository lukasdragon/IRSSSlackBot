using System.Configuration;
using Autofac;
using Config.Net;
using JavaJotter.Configuration.Interfaces;
using JavaJotter.Interfaces;
using JavaJotter.Services;
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
        {
            _onlineMode = false;
        }
        else
        {
            _onlineMode = true;
        }

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

    private static IAppAuthSettings RetrieveSettings()
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

        if (_onlineMode)
        {
            var settings = RetrieveSettings();
            builder.AddSlackNet(c => c.UseApiToken(settings.OAuthToken).UseAppLevelToken(settings.AppLevelToken)

                //Register our slack events here
                //   .RegisterEventHandler<MessageEvent, MessageHandler>()
            );
        }


        if (_onlineMode)
        {
            builder.RegisterType<SlackScrappingService>().As<IMessageScrapper>();
            builder.RegisterType<SlackUsernameService>().As<IUsernameService>();
        }
        else
        {
            builder.RegisterType<MockScrappingService>().As<IMessageScrapper>();
            builder.RegisterType<MockUsernameService>().As<IUsernameService>();

        }

        builder.RegisterType<RollFilter>().As<IRollFilter>();
        builder.RegisterType<SqLiteDatabaseService>().As<IDatabaseConnection>();


        return builder.Build();
    }

    private static async void Scrape()
    {
        await using var scope = _container.BeginLifetimeScope();

        var scrapper = scope.Resolve<IMessageScrapper>();
        var logger = scope.Resolve<ILogger>();
        var rollFilter = scope.Resolve<IRollFilter>();
        var usernameService = scope.Resolve<IUsernameService>();

        var databaseConnection = scope.Resolve<IDatabaseConnection>();

       
        var usernames = await usernameService.GetAllUsers();
        logger.Log($"Recording {usernames.Count} usernames");
        foreach (var user in usernames)
        {
            logger.Log(user);
            await databaseConnection.InsertUsername(user);
        }
        

        var messages = await scrapper.Scrape();

        List<Roll> rolls = new();
        foreach (var message in messages)
        {
            var roll = rollFilter.ProcessMessage(message);

            if (roll != null) rolls.Add(roll);
        }

       
        
        logger.Log($"Found {rolls.Count} rolls");

        foreach (var roll in rolls)
        {
            logger.Log(roll);
            await databaseConnection.InsertRoll(roll);
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