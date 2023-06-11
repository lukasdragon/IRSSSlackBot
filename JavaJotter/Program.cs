using System.Configuration;
using Autofac;
using Config.Net;
using JavaJotter.Configuration.Interfaces;
using JavaJotter.Interfaces;
using JavaJotter.Services;
using SlackNet;
using SlackNet.Autofac;
using ILogger = JavaJotter.Interfaces.ILogger;

namespace JavaJotter;

public static class Program
{
    private static volatile CancellationTokenSource _cancellationToken = new();


    private static ISlackSocketModeClient? _client;
    private static IContainer? Container { get; set; }


    private static async Task Main()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => { Cleanup(); };

        Console.CancelKeyPress += CancelHandler;


        var settings = RetrieveSettings();
        Container = BuildContainer(settings);

        var logger = Container.Resolve<ILogger>();

        logger.Log("Connecting...");
        _client = Container.SlackServices().GetSocketModeClient();
        await _client.Connect();
        logger.Log("Connected. Waiting for events...");


        Container.Resolve<IMessageScrapper>().Scrape();

        await MaintainLoop(logger);
    }

    private static void CancelHandler(object? sender, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;
        _cancellationToken.Cancel();
    }

    private static IContainer BuildContainer(IAppAuthSettings settings)
    {
        var builder = new ContainerBuilder();
        builder.Register(c => settings).As<IAppAuthSettings>().SingleInstance();

        builder.RegisterType<ConsoleLoggingService>().As<ILogger>();

        builder.AddSlackNet(c => c
                .UseApiToken(settings.OAuthToken)
                .UseAppLevelToken(settings.AppLevelToken)

            //Register our slack events here
            //   .RegisterEventHandler<MessageEvent, MessageHandler>()
        );


        builder.RegisterType<ScrappingService>().As<IMessageScrapper>();


        return builder.Build();
    }

    private static IAppAuthSettings RetrieveSettings()
    {
        var settings = new ConfigurationBuilder<IAppAuthSettings>()
            .UseYamlFile("token.yaml").Build();

        if (settings.OAuthToken == string.Empty)
            throw new ConfigurationErrorsException("OAuthToken is empty. Please add it to token.yaml");
        return settings;
    }

    private static void Cleanup()
    {
        Console.WriteLine("Cleaning up...");

        try
        {
            _client?.Disconnect();
        }
        catch (ObjectDisposedException)
        {
        } // If we are disposed, no worries.
    }

    private static async Task MaintainLoop(ILogger logger)
    {
        await Task.Delay(-1, _cancellationToken.Token);

        logger.Log("Exiting...");
    }
}