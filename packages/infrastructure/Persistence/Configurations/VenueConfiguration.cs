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
        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Address).HasMaxLength(500).IsRequired();
        builder.Property(v => v.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(v => v.CoverPhotoUrl).HasMaxLength(500);
        builder.Property(v => v.Phone).HasMaxLength(20);
        builder.Property(v => v.Website).HasMaxLength(500);
        builder.Property(v => v.GoogleRating).HasPrecision(2, 1);
        builder.Property(v => v.QrSecretKey).HasMaxLength(64).IsRequired();
        builder.Property(v => v.SubscriptionPlan).HasMaxLength(20).HasDefaultValue("basic");

        builder.HasIndex(v => v.Category);
        builder.HasIndex(v => v.OwnerUserId);
    }
}

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("checkins", "venue");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.QrPayloadHash).HasMaxLength(128).IsRequired();
        builder.Property(c => c.DeviceFingerprint).HasMaxLength(255);
        builder.Property(c => c.CheckOutReason).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Lat).HasPrecision(10, 7);
        builder.Property(c => c.Lng).HasPrecision(10, 7);

        builder.HasIndex(c => new { c.VenueId, c.CheckOutAt })
            .HasFilter("check_out_at IS NULL");
        builder.HasIndex(c => new { c.UserId, c.CheckInAt })
            .IsDescending(false, true);

        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
        builder.HasOne(c => c.Venue).WithMany(v => v.CheckIns).HasForeignKey(c => c.VenueId);
    }
}
