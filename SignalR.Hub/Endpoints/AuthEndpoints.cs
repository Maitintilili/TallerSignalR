using System.Text.RegularExpressions;
using SignalR.HubServer.Data;
using SignalR.HubServer.Services;
using SignalR.Shared.Contracts;

namespace SignalR.HubServer.Endpoints;

public static class AuthEndpoints
{
    private static readonly Regex UsernameRegex = new("^[A-Za-z0-9_]{3,20}$", RegexOptions.Compiled);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest request, UserRepository users, TokenService tokens) =>
        {
            var username = (request.Username ?? "").Trim();
            var password = request.Password ?? "";

            if (!UsernameRegex.IsMatch(username))
                return Results.BadRequest(new { error = "El usuario debe tener de 3 a 20 caracteres (letras, números o guion bajo)." });

            if (password.Length < 6 || password.Length > 100)
                return Results.BadRequest(new { error = "La contraseña debe tener al menos 6 caracteres." });

            var (hash, salt) = PasswordHasher.Hash(password);
            var id = await users.CreateAsync(username, hash, salt);

            if (id is null)
                return Results.Conflict(new { error = "Ese nombre de usuario ya existe." });

            var (token, expiresAt) = tokens.Create(id.Value, username);
            return Results.Ok(new AuthResponse(token, username, expiresAt));
        });

        group.MapPost("/login", async (LoginRequest request, UserRepository users, TokenService tokens) =>
        {
            var username = (request.Username ?? "").Trim();
            var password = request.Password ?? "";

            var user = await users.GetByUsernameAsync(username);

            // Mismo mensaje si el usuario no existe o la clave está mal (no revela cuál falló)
            if (user is null || !PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
                return Results.Json(new { error = "Usuario o contraseña incorrectos." }, statusCode: StatusCodes.Status401Unauthorized);

            var (token, expiresAt) = tokens.Create(user.Id, user.Username);
            return Results.Ok(new AuthResponse(token, user.Username, expiresAt));
        });
    }
}
