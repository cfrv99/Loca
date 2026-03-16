using Loca.Application.Interfaces;
using Loca.Domain.Interfaces;
using Loca.Infrastructure.Persistence;
using Loca.Infrastructure.Persistence.Repositories;
using Loca.Infrastructure.Redis;
using Loca.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Loca.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL + PostGIS
        services.AddDbContext<LocaDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ??
                    "Host=localhost;Database=loca;Username=loca_admin;Password=loca_dev_2024",
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite();
                    npgsqlOptions.MigrationsAssembly(typeof(LocaDbContext).Assembly.FullName);
                }));

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<IRedisService, RedisService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();

        // Services
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
