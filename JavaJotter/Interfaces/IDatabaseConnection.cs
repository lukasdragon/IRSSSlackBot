using JavaJotter.Types;
namespace JavaJotter.Interfaces;

public interface IDatabaseConnection
{
    public Task InsertRoll(Roll roll);

    public Task InsertUsername(Username username);

    public Task<Roll?> GetLastScrape();
}
