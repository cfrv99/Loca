using System.Text;
using FluentValidation;
using Loca.Application.Behaviors;
using Loca.Application.Interfaces;
using Loca.Domain.Interfaces;
using Loca.Infrastructure.Persistence;
using Loca.Infrastructure.Persistence.Repositories;
using Loca.Infrastructure.Redis;
using Loca.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Loca.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddLocaInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<LocaDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=loca;Username=postgres;Password=postgres",
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(LocaDbContext).Assembly.FullName);
                    npgsql.UseNetTopologySuite();
                }));

        // Redis
        var redisConnection = config.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<IRedisService, RedisService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IEconomyRepository, EconomyRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        // Services
        services.AddSingleton<ITokenService, TokenService>();

        // MediatR
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.StartsWith("Loca.") == true)
            .ToArray();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        // FluentValidation
        services.AddValidatorsFromAssemblies(
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.StartsWith("Loca.") == true)
                .ToArray()
        );

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // JWT Authentication
        var jwtSecret = config["Jwt:Secret"] ?? "loca-dev-secret-key-minimum-32-characters-long!!";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"] ?? "loca-api",
                    ValidAudience = config["Jwt:Audience"] ?? "loca-mobile",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };

                // Allow SignalR to use JWT from query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
