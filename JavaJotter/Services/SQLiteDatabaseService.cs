using System.Data;
using System.Data.SQLite;
using JavaJotter.Extensions;
using JavaJotter.Interfaces;
using JavaJotter.Types;

namespace JavaJotter.Services;

public class SqLiteDatabaseService : IDatabaseConnection, IDisposable
{
    private readonly ILogger _logger;

    public SqLiteDatabaseService(ILogger logger)
    {
        _logger = logger;
    }

    public bool IsConnected => _sqLiteConnection?.State == ConnectionState.Open;


    private SQLiteConnection? _sqLiteConnection;

    public Task Connect()
    {
        const string connectionString = "Data Source=identifier.sqlite";

        _sqLiteConnection = new SQLiteConnection(connectionString);

        _sqLiteConnection.Open();

        var version = GetSqLiteVersion();

        if (version != null) _logger.Log(version);


        CreateUsernameTableIfNotExist(_sqLiteConnection);
        CreateRollTableIfNotExist(_sqLiteConnection);


        return Task.CompletedTask;
    }

    private string? GetSqLiteVersion()
    {
        const string sql = @"SELECT SQLITE_VERSION()";

        using var versionCommand = new SQLiteCommand(sql, _sqLiteConnection);
        var version = versionCommand.ExecuteScalar().ToString();
        return version;
    }

    private static void CreateRollTableIfNotExist(SQLiteConnection sqLiteConnection)
    {
        const string sql = @"CREATE TABLE IF NOT EXISTS Rolls (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                            UnixMs INTEGER NOT NULL, 
                            UserId TEXT NOT NULL, 
                            Value INTEGER NOT NULL)";
        
        using var createRollTableCommand = new SQLiteCommand(sql, sqLiteConnection);
        createRollTableCommand.ExecuteNonQuery();
    }

    private static void CreateUsernameTableIfNotExist(SQLiteConnection sqLiteConnection)
    {
        const string sql = @"CREATE TABLE IF NOT EXISTS UsernameTable (
                                    UserId TEXT PRIMARY KEY, 
                                    UserName TEXT NOT NULL)";
        
        using var createUsernameTableCommand = new SQLiteCommand(sql, sqLiteConnection);
        createUsernameTableCommand.ExecuteNonQuery();
    }

    public Task Disconnect()
    {
        return Task.CompletedTask;
    }

    public async Task InsertRoll(Roll roll)
    {
        if (!IsConnected)
        {
            await Connect();
        }

        const string sql = "INSERT INTO Rolls (UnixMs, UserId, Value) VALUES (@UnixMs, @UserId, @Value)";

        await using var command = new SQLiteCommand(sql, _sqLiteConnection);
        command.Parameters.AddWithValue("@UnixMs", roll.DateTime.ToUnixTimeMilliseconds());
        command.Parameters.AddWithValue("@UserId", roll.UserId);
        command.Parameters.AddWithValue("@Value", roll.Value);

        await command.ExecuteNonQueryAsync();
    }

    public Task InsertUsername(Username username)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _sqLiteConnection?.Dispose();
    }
}