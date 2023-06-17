using System.Configuration;
using Autofac;
using Config.Net;
using JavaJotter.Configuration.Interfaces;
using JavaJotter.Extensions;
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
    private static readonly IContainer Container = BuildContainer();


    private static async Task Main()
    {
        var logger = Container.Resolve<ILogger>();

        logger.Log("Connecting...");
        _client = Container.SlackServices().GetSocketModeClient();
        await _client.Connect();
        logger.Log("Connected. Waiting for events...");

        
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
        var settings = RetrieveSettings();
        var builder = new ContainerBuilder();

        builder.RegisterType<ConsoleLoggingService>().As<ILogger>().SingleInstance();

        builder.AddSlackNet(c => c
                .UseApiToken(settings.OAuthToken)
                .UseAppLevelToken(settings.AppLevelToken)

            //Register our slack events here
            //   .RegisterEventHandler<MessageEvent, MessageHandler>()
        );


        builder.RegisterType<ScrappingService>().As<IMessageScrapper>();

        builder.RegisterType<RollRollFilter>().As<IRollFilter>();


        return builder.Build();
    }

    public static void Scrape()
    {
        using var scope = Container.BeginLifetimeScope();

        var scrapper = scope.Resolve<IMessageScrapper>();
        var logger = scope.Resolve<ILogger>();
        var rollFilter = scope.Resolve<IRollFilter>();


        var messages = scrapper.Scrape().Result;

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