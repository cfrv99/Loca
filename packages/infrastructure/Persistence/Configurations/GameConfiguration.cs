using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("game_sessions", "game");
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => new { g.VenueId, g.Status });
        builder.Property(g => g.GameState).HasMaxLength(10000);

        builder.HasOne(g => g.Venue).WithMany().HasForeignKey(g => g.VenueId);
        builder.HasOne(g => g.Host).WithMany().HasForeignKey(g => g.HostId);
    }
}

public class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.ToTable("game_players", "game");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.GameSessionId, p.UserId }).IsUnique();
        builder.Property(p => p.RoleData).HasMaxLength(2000);

        builder.HasOne(p => p.GameSession).WithMany(g => g.Players).HasForeignKey(p => p.GameSessionId);
        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
    }
}
