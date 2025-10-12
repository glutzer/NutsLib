using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace NutsLib;

public enum DatabaseKey
{
    INTEGER,
    STRING
}

/// <summary>
/// Database file that allows multiple tables, with a key : blob structure.
/// Base folder is the game data path.
/// </summary>
public class DatabaseFile
{
    private SqliteConnection? currentConnection;
    private readonly string connectionString;

    public DatabaseFile(string pathAndDbName)
    {
        pathAndDbName = Path.Combine(GamePaths.DataPath, pathAndDbName);

        string? directory = Path.GetDirectoryName(pathAndDbName);
        if (directory != null) Directory.CreateDirectory(directory);

        // Make sure path ends with .db.
        if (!pathAndDbName.EndsWith(".db")) pathAndDbName += ".db";

        connectionString = $"Data Source={pathAndDbName};";
    }

    /// <summary>
    /// Open a db connection.
    /// </summary>
    public void Open()
    {
        if (currentConnection != null)
        {
            return; // Already open.
        }

        currentConnection = new SqliteConnection(connectionString);
        currentConnection.Open();
    }

    /// <summary>
    /// Takes a protobuf serializable object and inserts it into the database.
    /// </summary>
    public void Insert<T>(string table, int key, T data)
    {
        if (currentConnection == null)
        {
            throw new Exception("Trying to insert into unopened table!");
        }

        TryInitializeTable(table, DatabaseKey.INTEGER);

        byte[] serializedData = SerializerUtil.Serialize(data);

        using SqliteCommand command = new($"INSERT OR REPLACE INTO {table} (data) VALUES (@data)", currentConnection);
        command.Parameters.AddWithValue("@id", key);
        command.Parameters.AddWithValue("@data", serializedData);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Takes a protobuf serializable object and inserts it into the database.
    /// </summary>
    public void Insert<T>(string table, string key, T data)
    {
        if (currentConnection == null) return;

        TryInitializeTable(table, DatabaseKey.STRING);

        byte[] serializedData = SerializerUtil.Serialize(data);

        using SqliteCommand command = new($"INSERT OR REPLACE INTO {table} (id, data) VALUES (@id, @data)", currentConnection);
        command.Parameters.AddWithValue("@id", key);
        command.Parameters.AddWithValue("@data", serializedData);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Takes many serializable objects and inserts them into the database.
    /// </summary>
    public void InsertMany<T>(string table, IEnumerable<KeyValuePair<int, T>> data)
    {
        if (currentConnection == null) return;

        TryInitializeTable(table, DatabaseKey.INTEGER);

        using SqliteTransaction transaction = currentConnection.BeginTransaction();
        string query = $"INSERT OR REPLACE INTO {table} (id, data) VALUES (@id, @data)";

        using SqliteCommand command = new(query, currentConnection);
        command.Parameters.Add("@id");
        command.Parameters.Add("@data");

        foreach (KeyValuePair<int, T> item in data)
        {
            byte[] serializedData = SerializerUtil.Serialize(item);
            command.Parameters["@id"].Value = item.Key;
            command.Parameters["@data"].Value = serializedData;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    /// <summary>
    /// Takes many serializable objects and inserts them into the database.
    /// </summary>
    public void InsertMany<T>(string table, IEnumerable<KeyValuePair<string, T>> data)
    {
        if (currentConnection == null) return;

        TryInitializeTable(table, DatabaseKey.STRING);

        using SqliteTransaction transaction = currentConnection.BeginTransaction();
        string query = $"INSERT OR REPLACE INTO {table} (id, data) VALUES (@id, @data)";

        using SqliteCommand command = new(query, currentConnection);
        command.Parameters.Add("@id");
        command.Parameters.Add("@data");

        foreach (KeyValuePair<string, T> item in data)
        {
            byte[] serializedData = SerializerUtil.Serialize(item);

            command.Parameters["@id"].Value = item.Key;
            command.Parameters["@data"].Value = serializedData;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    /// <summary>
    /// Get a value. If the class is reference it may be null.
    /// </summary>
    public T? Get<T>(string table, int key)
    {
        if (currentConnection == null) return default!;

        TryInitializeTable(table, DatabaseKey.INTEGER);

        using SqliteCommand command = new($"SELECT data FROM {table} WHERE id = @id", currentConnection);
        command.Parameters.AddWithValue("@id", key);

        using SqliteDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return default!;

        byte[] data = (byte[])reader["data"];
        return SerializerUtil.Deserialize<T>(data);
    }

    /// <summary>
    /// Get every blob of data in the database.
    /// </summary>
    public List<T> GetAll<T>(string table)
    {
        if (currentConnection == null) return default!;

        List<T> data = [];

        // Tries to get data, if this table exists.
        try
        {
            using SqliteCommand command = new($"SELECT data FROM {table}", currentConnection);
            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                byte[] blob = (byte[])reader["data"];
                data.Add(SerializerUtil.Deserialize<T>(blob));
            }
        }
        catch
        {

        }


        return data;
    }

    /// <summary>
    /// Get a value. If the class is reference it may be null.
    /// </summary>
    public T? Get<T>(string table, string key)
    {
        if (currentConnection == null) return default!;

        TryInitializeTable(table, DatabaseKey.STRING);

        using SqliteCommand command = new($"SELECT data FROM {table} WHERE id = @id", currentConnection);
        command.Parameters.AddWithValue("@id", key);

        using SqliteDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return default!;

        byte[] data = (byte[])reader["data"];
        return SerializerUtil.Deserialize<T>(data);
    }

    /// <summary>
    /// Close a db connection.
    /// </summary>
    public void Close()
    {
        currentConnection?.Close();
        currentConnection?.Dispose();
        currentConnection = null;
    }

    /// <summary>
    /// Initialize a table if it doesn't exist.
    /// </summary>
    private void TryInitializeTable(string table, DatabaseKey key)
    {
        string keyType = key switch
        {
            DatabaseKey.INTEGER => "INTEGER",
            DatabaseKey.STRING => "TEXT",
            _ => throw new NotImplementedException()
        };

        string createTableQuery = @$"
                CREATE TABLE IF NOT EXISTS {table} (
                    id {keyType} PRIMARY KEY,
                    data BLOB NOT NULL
                );
            ";

        using SqliteCommand command = new(createTableQuery, currentConnection);
        command.ExecuteNonQuery();
    }
}