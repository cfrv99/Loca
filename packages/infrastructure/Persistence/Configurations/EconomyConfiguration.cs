using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets", "economy");
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => w.UserId).IsUnique();
    }
}

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", "economy");
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.WalletId, t.CreatedAt });
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.ReferenceId).HasMaxLength(255);

        builder.HasOne(t => t.Wallet).WithMany(w => w.Transactions).HasForeignKey(t => t.WalletId);
    }
}

public class GiftConfiguration : IEntityTypeConfiguration<Gift>
{
    public void Configure(EntityTypeBuilder<Gift> builder)
    {
        builder.ToTable("gifts", "economy");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(500);
        builder.Property(g => g.AnimationUrl).HasMaxLength(500);
        builder.Property(g => g.IconUrl).HasMaxLength(500);
    }
}

public class GiftTransactionConfiguration : IEntityTypeConfiguration<GiftTransaction>
{
    public void Configure(EntityTypeBuilder<GiftTransaction> builder)
    {
        builder.ToTable("gift_transactions", "economy");
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => new { g.SenderId, g.CreatedAt });
        builder.HasIndex(g => new { g.ReceiverId, g.CreatedAt });

        builder.HasOne(g => g.Sender).WithMany().HasForeignKey(g => g.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(g => g.Receiver).WithMany().HasForeignKey(g => g.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(g => g.GiftItem).WithMany().HasForeignKey(g => g.GiftId);
    }
}
