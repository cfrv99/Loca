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
        builder.Property(g => g.GameType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(g => g.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue("Lobby");
        builder.Property(g => g.SettingsJson).HasColumnType("jsonb");
        builder.Property(g => g.StateJson).HasColumnType("jsonb");
        builder.Property(g => g.CurrentPhase).HasMaxLength(50);

        builder.HasIndex(g => new { g.VenueId, g.Status })
            .HasFilter("status IN ('Lobby','InProgress')");
    }
}

public class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.ToTable("game_players", "game");
        builder.HasKey(gp => new { gp.SessionId, gp.UserId });
        builder.Property(gp => gp.Role).HasMaxLength(50);

        builder.HasOne(gp => gp.Session)
            .WithMany(gs => gs.Players)
            .HasForeignKey(gp => gp.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
