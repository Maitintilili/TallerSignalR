using System.Globalization;
using Dapper;
using SignalR.Shared.Contracts;

namespace SignalR.HubServer.Data;

public sealed record MessageRow(string Username, string Mensaje, string SentAt);

public sealed class MessageRepository(Db db)
{
    public async Task AddAsync(long userId, string message, DateTimeOffset sentAt)
    {
        using var connection = db.Open();
        await connection.ExecuteAsync(
            """
            INSERT INTO Mensajes (UsuarioId, Mensaje, SentAt)
            VALUES (@userId, @message, @sentAt)
            """,
            new { userId, message, sentAt = sentAt.ToString("O") });
    }

    /// <summary>Últimos mensajes, del más antiguo al más nuevo.</summary>
    public async Task<List<ChatMessage>> GetLastAsync(int count = 50)
    {
        using var connection = db.Open();
        var rows = await connection.QueryAsync<MessageRow>(
            """
            SELECT u.Username, m.Mensaje, m.SentAt
            FROM Mensajes m
            JOIN Usuarios u ON u.Id = m.UsuarioId
            ORDER BY m.Id DESC
            LIMIT @count
            """,
            new { count });

        return rows
            .Reverse()
            .Select(r => new ChatMessage(
                r.Username,
                r.Mensaje,
                DateTimeOffset.Parse(r.SentAt, CultureInfo.InvariantCulture)))
            .ToList();
    }
}
