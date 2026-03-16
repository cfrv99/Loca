using System.Text;
using FluentValidation;
using Loca.Application.Behaviors;
using Loca.Infrastructure.Configuration;
using Loca.Infrastructure.Persistence;
using Loca.Services.Game.Hubs;
using Loca.Services.Social.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── MediatR + FluentValidation ──
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(Loca.Services.Identity.Commands.GoogleLoginCommand).Assembly,
    typeof(Loca.Services.Venue.Commands.CheckInCommand).Assembly,
    typeof(Loca.Services.Social.Hubs.VenueChatHub).Assembly,
    typeof(Loca.Services.Game.Hubs.GameHub).Assembly
));
builder.Services.AddValidatorsFromAssemblies(new[]
{
    typeof(Loca.Services.Identity.Validators.GoogleLoginValidator).Assembly,
    typeof(Loca.Services.Venue.Validators.CheckInValidator).Assembly
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ── Infrastructure (EF Core + Redis + Repositories) ──
builder.Services.AddInfrastructure(builder.Configuration);

// ── Authentication (JWT) ──
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "super-secret-key-for-development-only-min-32-chars";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "loca-api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "loca-mobile",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // SignalR token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/chat") || path.StartsWithSegments("/hubs/game")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ── SignalR ──
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024; // 1KB max per convention
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// ── Controllers + Swagger ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Loca API",
        Version = "v1",
        Description = "Location-Based Social Discovery Platform API"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Middleware Pipeline ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<VenueChatHub>("/hubs/chat");
app.MapHub<GameHub>("/hubs/game");

// ── Apply Migrations on startup (Development only) ──
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LocaDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
