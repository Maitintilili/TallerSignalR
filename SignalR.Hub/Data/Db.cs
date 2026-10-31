using Dapper;
using Microsoft.Data.Sqlite;

namespace SignalR.HubServer.Data;

public sealed class Db
{
    private readonly string _connectionString;

    public Db(IConfiguration config, IHostEnvironment env)
    {
        var builder = new SqliteConnectionStringBuilder(
            config.GetConnectionString("Default") ?? "Data Source=chat.db");

        // El archivo .db se guarda dentro de la carpeta del proyecto Hub
        if (!Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.Combine(env.ContentRootPath, builder.DataSource);

        _connectionString = builder.ToString();
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>Crea las tablas si todavía no existen.</summary>
    public void Initialize()
    {
        using var connection = Open();
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                Username     TEXT NOT NULL UNIQUE COLLATE NOCASE,
                PasswordHash TEXT NOT NULL,
                PasswordSalt TEXT NOT NULL,
                CreatedAt    TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Mensajes (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                UsuarioId INTEGER NOT NULL,
                Mensaje   TEXT NOT NULL,
                SentAt    TEXT NOT NULL,
                FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
            );
            """);
    }
}
