using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SignalR.HubServer.Data;
using SignalR.HubServer.Endpoints;
using SignalR.HubServer.Hubs;
using SignalR.HubServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPolicy", policy =>
    {
        policy
        .WithOrigins(
            "https://localhost:7002",
            "https://localhost:7147",
            "http://localhost:5293")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddOpenApi();

// Base de datos y servicios
builder.Services.AddSingleton<Db>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<MessageRepository>();
builder.Services.AddSingleton<TokenService>();

// Autenticación con JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; 

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateLifetime = true,
            NameClaimType = "name"
        };

        // SignalR (WebSockets) manda el token en la URL (?access_token=...)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();

// Crea las tablas Usuarios y Mensajes si no existen
app.Services.GetRequiredService<Db>().Initialize();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("ClientPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapHub<ChatHub>("/chatHub");

app.Run();
