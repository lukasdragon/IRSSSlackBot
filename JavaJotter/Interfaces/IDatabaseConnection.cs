using JavaJotter.Types;
namespace JavaJotter.Interfaces;

public interface IDatabaseConnection
{
    public bool IsConnected { get; }
    public Task Connect();
    public Task Disconnect();

    public Task InsertRoll(Roll roll);

    public Task InsertUsername(Username username);

    public Task<Roll?> GetLastScrape();
}
