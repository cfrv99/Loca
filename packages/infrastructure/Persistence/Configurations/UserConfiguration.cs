using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(500);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.ThumbnailUrl).HasMaxLength(500);
        builder.Property(u => u.DateOfBirth).IsRequired();
        builder.Property(u => u.Gender).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.AuthProvider).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.AuthProviderId).HasMaxLength(255);

        // Ignore navigation collections handled by join tables
        builder.Ignore(u => u.Interests);
        builder.Ignore(u => u.Purposes);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}

public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.ToTable("user_interests", "identity");
        builder.HasKey(ui => new { ui.UserId, ui.Interest });
        builder.Property(ui => ui.Interest).HasMaxLength(50).IsRequired();
    }
}

public class UserPurposeConfiguration : IEntityTypeConfiguration<UserPurpose>
{
    public void Configure(EntityTypeBuilder<UserPurpose> builder)
    {
        builder.ToTable("user_purposes", "identity");
        builder.HasKey(up => new { up.UserId, up.Purpose });
        builder.Property(up => up.Purpose).HasMaxLength(50).IsRequired();
    }
}

public class UserVibePreferenceConfiguration : IEntityTypeConfiguration<UserVibePreference>
{
    public void Configure(EntityTypeBuilder<UserVibePreference> builder)
    {
        builder.ToTable("user_vibe_preferences", "identity");
        builder.HasKey(vp => new { vp.UserId, vp.Vibe });
        builder.Property(vp => vp.Vibe).HasMaxLength(50).IsRequired();
        builder.Property(vp => vp.Weight).HasPrecision(3, 2).HasDefaultValue(1.0m);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", "identity");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.TokenHash).HasMaxLength(512).IsRequired();
        builder.Property(rt => rt.DeviceFingerprint).HasMaxLength(255);
        builder.HasIndex(rt => rt.UserId).HasFilter("revoked_at IS NULL");
    }
}
