using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues", "venue");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => v.Name);

        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Description).HasMaxLength(2000);
        builder.Property(v => v.Address).HasMaxLength(500).IsRequired();
        builder.Property(v => v.CoverPhotoUrl).HasMaxLength(500);
        builder.Property(v => v.Phone).HasMaxLength(20);
        builder.Property(v => v.Website).HasMaxLength(500);
        builder.Property(v => v.InstagramHandle).HasMaxLength(100);
        builder.Property(v => v.OpeningHours).HasMaxLength(1000);

        // PostGIS spatial column
        builder.Property(v => v.Location).HasColumnType("geometry (point, 4326)");
        builder.HasIndex(v => v.Location).HasMethod("gist");
    }
}

public class QrCodeConfiguration : IEntityTypeConfiguration<QrCode>
{
    public void Configure(EntityTypeBuilder<QrCode> builder)
    {
        builder.ToTable("qr_codes", "venue");
        builder.HasKey(q => q.Id);
        builder.HasIndex(q => q.Payload).IsUnique();

        builder.Property(q => q.Payload).HasMaxLength(500).IsRequired();
        builder.Property(q => q.Secret).HasMaxLength(255).IsRequired();

        builder.HasOne(q => q.Venue)
            .WithMany(v => v.QrCodes)
            .HasForeignKey(q => q.VenueId);
    }
}

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("checkins", "venue");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.UserId, c.VenueId, c.CheckInAt });
        builder.HasIndex(c => c.VenueId);

        builder.Property(c => c.QrPayloadHash).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DeviceFingerprint).HasMaxLength(255).IsRequired();

        builder.HasOne(c => c.User)
            .WithMany(u => u.CheckIns)
            .HasForeignKey(c => c.UserId);

        builder.HasOne(c => c.Venue)
            .WithMany(v => v.CheckIns)
            .HasForeignKey(c => c.VenueId);
    }
}
