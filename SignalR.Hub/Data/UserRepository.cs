using Dapper;
using Microsoft.Data.Sqlite;

namespace SignalR.HubServer.Data;

public sealed record UserRow(long Id, string Username, string PasswordHash, string PasswordSalt);

public sealed class UserRepository(Db db)
{
    public async Task<UserRow?> GetByUsernameAsync(string username)
    {
        using var connection = db.Open();
        return await connection.QuerySingleOrDefaultAsync<UserRow>(
            """
            SELECT Id, Username, PasswordHash, PasswordSalt
            FROM Usuarios
            WHERE Username = @username
            """,
            new { username });
    }

    /// <summary>Devuelve el Id del usuario creado, o null si el nombre ya existe.</summary>
    public async Task<long?> CreateAsync(string username, string passwordHash, string passwordSalt)
    {
        using var connection = db.Open();
        try
        {
            return await connection.ExecuteScalarAsync<long>(
                """
                INSERT INTO Usuarios (Username, PasswordHash, PasswordSalt, CreatedAt)
                VALUES (@username, @passwordHash, @passwordSalt, @createdAt);
                SELECT last_insert_rowid();
                """,
                new
                {
                    username,
                    passwordHash,
                    passwordSalt,
                    createdAt = DateTimeOffset.UtcNow.ToString("O")
                });
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint
        {
            return null;
        }
    }
}
