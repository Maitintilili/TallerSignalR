using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SignalR.HubServer.Services;

public sealed class TokenService(IConfiguration config)
{
    public (string Token, DateTimeOffset ExpiresAt) Create(long userId, string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var expires = DateTime.UtcNow.AddHours(8);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", userId.ToString()),
                new Claim("name", username)
            ]),
            Expires = expires,
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, new DateTimeOffset(expires));
    }
}
