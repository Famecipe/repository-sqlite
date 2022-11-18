using Famecipe.Models;
using Famecipe.Common;
using Microsoft.Data.Sqlite;

namespace Famecipe.Repository.Sqlite;

public class MetadataRepositorySqlite : IRepository<Metadata>
{
    private readonly string _metadataConnectionString;

    public MetadataRepositorySqlite()
    {
        var metadataDatabase = Environment.GetEnvironmentVariable("METADATA_DATA_SOURCE");

        if (string.IsNullOrEmpty(metadataDatabase))
        {
            this._metadataConnectionString = $"Data Source=Metadata.db";
        }
        else
        {
            this._metadataConnectionString = $"Data Source={metadataDatabase}";
        }

        using (var connection = new SqliteConnection(this._metadataConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Metadata (Key TEXT, Value TEXT);
            ";
            command.ExecuteNonQuery();
        }
    }

    public async Task<Metadata> Create(Metadata metadata)
    {
        using (var connection = new SqliteConnection(this._metadataConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO Metadata
                VALUES($Key, $Value)
            ";
            command.Parameters.AddWithValue("$Key", metadata.Key);
            command.Parameters.AddWithValue("$Value", System.Text.Json.JsonSerializer.Serialize<object>(metadata.Value!));

            await command.ExecuteNonQueryAsync();

            return await Get(metadata.Key!);
        }
    }

    public async Task Delete(string key)
    {
        using (var connection = new SqliteConnection(this._metadataConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                DELETE
                FROM Metadata
                WHERE Key = $Key
            ";
            command.Parameters.AddWithValue("$Key", key);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<IEnumerable<Metadata>> Get()
    {
        using (var connection = new SqliteConnection(this._metadataConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Metadata
            ";

            var metadata = new List<Metadata>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                var keyOrdinal = reader.GetOrdinal("Key");
                var valueOrdinal = reader.GetOrdinal("Value");

                while (await reader.ReadAsync())
                {
                    metadata.Add(new Metadata()
                    {
                        Key = reader.GetString(keyOrdinal),
                        Value = System.Text.Json.JsonSerializer.Deserialize<object>(reader.GetString(valueOrdinal))
                    });
                }
            }

            return metadata;
        }
    }

    public async Task<Metadata> Get(string key)
    {
        using (var connection = new SqliteConnection(this._metadataConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Metadata
                WHERE Key = $Key
            ";
            command.Parameters.AddWithValue("$Key", key);

            Metadata metadata = null;

            using (var reader = await command.ExecuteReaderAsync())
            {
                var keyOrdinal = reader.GetOrdinal("Key");
                var valueOrdinal = reader.GetOrdinal("Value");

                while (await reader.ReadAsync())
                {
                    metadata = new Metadata()
                    {
                        Key = reader.GetString(keyOrdinal),
                        Value = System.Text.Json.JsonSerializer.Deserialize<object>(reader.GetString(valueOrdinal))
                    };
                }
            }

            return metadata;
        }
    }

    public async Task Update(string key, Metadata metadata)
    {
        using (var connection = new SqliteConnection(this._metadataConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                UPDATE Metadata
                SET Value = $Value
                WHERE Key = $Key
            ";
            command.Parameters.AddWithValue("$Key", key);
            command.Parameters.AddWithValue("$Value", System.Text.Json.JsonSerializer.Serialize<object>(metadata.Value!));

            await command.ExecuteNonQueryAsync();
        }
    }
}
