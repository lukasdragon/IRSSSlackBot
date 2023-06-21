using JavaJotter.Configuration.Interfaces;
using JavaJotter.Extensions;
using JavaJotter.Interfaces;
using JavaJotter.Types;
using Npgsql;
namespace JavaJotter.Services.Databases;

public class PostgresDatabaseService : IDatabaseConnection
{
    private readonly NpgsqlDataSource _dataSource;
    private ILogger _logger;

    private bool _tablesCreated;

    public PostgresDatabaseService(ILogger logger, IAppAuthSettings appAuthSettings)
    {
        _logger = logger;
        _dataSource = NpgsqlDataSource.Create(appAuthSettings.DatabaseConnectionString);
    }

    public async Task InsertRoll(Roll roll)
    {
        await CreateTables();


        const string sql = "INSERT INTO rolls (unix_milliseconds, user_id, dice_value) " +
            "VALUES (:unix_milliseconds, :user_id, :dice_value);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.Add(new NpgsqlParameter("unix_milliseconds", roll.DateTime.ToUnixTimeMilliseconds()));
        command.Parameters.Add(new NpgsqlParameter("user_id", roll.UserId));
        command.Parameters.Add(new NpgsqlParameter("dice_value", roll.Value));

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertUsername(Username username)
    {
        await CreateTables();


        const string sql = @"INSERT INTO usernames (user_id, username) 
                         VALUES (:user_id, :username)
                         ON CONFLICT(user_id) DO UPDATE 
                         SET username = EXCLUDED.username;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.Add(new NpgsqlParameter("user_id", username.Id));
        command.Parameters.Add(new NpgsqlParameter("username", username.Name));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Roll?> GetLastScrape()
    {
        const string sql = @"SELECT * FROM rolls ORDER BY unix_milliseconds DESC LIMIT 1;";

        await using var command = _dataSource.CreateCommand(sql);


        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(reader.GetOrdinal("unix_milliseconds"))).DateTime;
        var userId = reader.GetString(reader.GetOrdinal("user_id"));
        var value = reader.GetInt32(reader.GetOrdinal("dice_value"));

        return new Roll(dateTime, userId, value);
    }
    private async Task CreateTables()
    {
        if (_tablesCreated)
            return;
        await CreateUsernameTableIfNotExist();
        await CreateRollTableIfNotExist();
        _tablesCreated = true;
    }


    private async Task CreateUsernameTableIfNotExist()
    {
        const string sql = @"CREATE TABLE IF NOT EXISTS usernames (
                                user_id TEXT PRIMARY KEY,
                                username TEXT NOT NULL
                        );";

        await using var command = _dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateRollTableIfNotExist()
    {
        const string sql = @"CREATE TABLE IF NOT EXISTS rolls (
                                id SERIAL PRIMARY KEY, 
                                unix_milliseconds BIGINT NOT NULL, 
                                user_id TEXT NOT NULL, 
                                dice_value INTEGER NOT NULL,
                                FOREIGN KEY (user_id) REFERENCES usernames(user_id));
                            CREATE INDEX IF NOT EXISTS unix_milliseconds_index ON rolls (unix_milliseconds);";


        await using var command = _dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync();
    }
}
