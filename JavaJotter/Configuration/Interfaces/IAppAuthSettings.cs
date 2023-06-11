namespace JavaJotter.Configuration.Interfaces;

public interface IAppAuthSettings
{
    public string OAuthToken { get; }
    public string SigningSecret { get; }
    public string AppLevelToken { get; }
}