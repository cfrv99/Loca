using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets", "economy");
        builder.HasKey(w => w.UserId);
        builder.Property(w => w.CoinBalance).HasDefaultValue(0);
    }
}

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", "economy");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.ReferenceType).HasMaxLength(30);
        builder.Property(t => t.Description).HasMaxLength(200);

        builder.HasIndex(t => new { t.UserId, t.CreatedAt })
            .IsDescending(false, true);
    }
}

public class GiftCatalogConfiguration : IEntityTypeConfiguration<GiftCatalogItem>
{
    public void Configure(EntityTypeBuilder<GiftCatalogItem> builder)
    {
        builder.ToTable("gift_catalog", "economy");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.NameAz).HasMaxLength(100);
        builder.Property(g => g.Tier).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(g => g.AnimationUrl).HasMaxLength(500);
        builder.Property(g => g.IconUrl).HasMaxLength(500);
    }
}

public class CoinPackageConfiguration : IEntityTypeConfiguration<CoinPackage>
{
    public void Configure(EntityTypeBuilder<CoinPackage> builder)
    {
        builder.ToTable("coin_packages", "economy");
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Name).HasMaxLength(50).IsRequired();
        builder.Property(cp => cp.NameAz).HasMaxLength(50);
        builder.Property(cp => cp.PriceAzn).HasPrecision(10, 2);
        builder.Property(cp => cp.IosProductId).HasMaxLength(100);
        builder.Property(cp => cp.AndroidProductId).HasMaxLength(100);
    }
}
