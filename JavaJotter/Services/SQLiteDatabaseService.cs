using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using JavaJotter.Extensions;
using JavaJotter.Interfaces;
using JavaJotter.Types;

namespace JavaJotter.Services;

public partial class SqLiteDatabaseService : IDatabaseConnection, IDisposable
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

#if DEBUG
        _logger.Log("Deleting all tables to ensure a clean start for testing in DEBUG mode.");
        DeleteAllTables(_sqLiteConnection);
#endif

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

    private static void CreateUsernameTableIfNotExist(SQLiteConnection sqLiteConnection)
    {
        const string sql =
            @"CREATE TABLE IF NOT EXISTS usernames (id TEXT PRIMARY KEY, username TEXT NOT NULL);";

        using var createUsernameTableCommand = new SQLiteCommand(sql, sqLiteConnection);
        createUsernameTableCommand.ExecuteNonQuery();
    }

    private static void CreateRollTableIfNotExist(SQLiteConnection sqLiteConnection)
    {
        const string sql = @"CREATE TABLE IF NOT EXISTS rolls (
                            id INTEGER PRIMARY KEY AUTOINCREMENT, 
                            unix_milliseconds INTEGER NOT NULL, 
                            user_id TEXT NOT NULL, 
                            dice_value INTEGER NOT NULL,
                            FOREIGN KEY (user_id) REFERENCES usernames(id));";

        using var createRollTableCommand = new SQLiteCommand(sql, sqLiteConnection);
        createRollTableCommand.ExecuteNonQuery();
    }


    private static void DeleteAllTables(SQLiteConnection sqLiteConnection)
    {
        const string getTableNamesSql =
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

        var tableNames = new List<string>();

        using (var command = new SQLiteCommand(getTableNamesSql, sqLiteConnection))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader["name"].ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        tableNames.Add(name);
                    }
                }
            }
        }

        foreach (var tableName in tableNames)
        {
            if (!IsValidIdentifier(tableName))
            {
                throw new ArgumentException($"Invalid table name: {tableName}");
            }

            var deleteTableSql = $"DROP TABLE IF EXISTS \"{tableName}\";";

            using var command = new SQLiteCommand(deleteTableSql, sqLiteConnection);
            command.ExecuteNonQuery();
        }
    }


    public async Task InsertRoll(Roll roll)
    {
        if (!IsConnected)
        {
            await Connect();
        }

        const string sql = "INSERT INTO rolls (unix_milliseconds, user_id, dice_value) " +
                           "VALUES (@unix_milliseconds, @user_id, @dice_value);";

        await using var command = new SQLiteCommand(sql, _sqLiteConnection);
        command.Parameters.AddWithValue("@unix_milliseconds", roll.DateTime.ToUnixTimeMilliseconds());
        command.Parameters.AddWithValue("@user_id", roll.UserId);
        command.Parameters.AddWithValue("@dice_value", roll.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertUsername(Username username)
    {
        if (!IsConnected)
        {
            await Connect();
        }

        const string sql = @"INSERT INTO usernames (id, username) 
                             VALUES (@id, @username)
                             ON CONFLICT(id) DO 
                             UPDATE SET username = @username";

        await using var command = new SQLiteCommand(sql, _sqLiteConnection);
        command.Parameters.AddWithValue("@id", username.Id);
        command.Parameters.AddWithValue("@username", username.Name);

        await command.ExecuteNonQueryAsync();
    }

    // For simplicity, we're just ensuring that the table name only contains alphanumeric characters and underscores
    private static bool IsValidIdentifier(string tableName)
    {
        return ValidIdentifier().IsMatch(tableName);
    }

    public Task Disconnect()
    {
        _sqLiteConnection?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _sqLiteConnection?.Dispose();
    }

    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex ValidIdentifier();
}