using Config.Net;
using SlackBot.Configuration.Interfaces;
using SlackNet.AspNetCore;
var settings = new ConfigurationBuilder<IAppAuthSettings>()
    .UseYamlFile("token.yaml")
    .Build();

Console.WriteLine(settings.OAuthToken);
Console.WriteLine(settings.SigningSecret);
Console.WriteLine(settings.AppLevelToken);



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSlackNet(c => c.UseApiToken(settings.OAuthToken).UseAppLevelToken(settings.AppLevelToken));
var app = builder.Build();

app.UseSlackNet(c =>
{
    c.UseSigningSecret(settings.SigningSecret).UseSocketMode(true);
});






app.MapGet("/", () => "Hello Slack!");

app.Run();
