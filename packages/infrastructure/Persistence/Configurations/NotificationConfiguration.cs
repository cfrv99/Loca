using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("device_tokens", "notification");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Token).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Platform).HasMaxLength(10).IsRequired();

        builder.HasIndex(d => new { d.UserId, d.Token }).IsUnique();
    }
}

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_log", "notification");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).HasMaxLength(50).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(200);
        builder.Property(n => n.Body).HasMaxLength(500);
        builder.Property(n => n.DataJson).HasColumnType("jsonb");

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .IsDescending(false, true);
    }
}
