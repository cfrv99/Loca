using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class Wallet
{
    public Guid UserId { get; set; }
    public int CoinBalance { get; set; }
    public int TotalPurchased { get; set; }
    public int TotalSpent { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool CanAfford(int amount) => CoinBalance >= amount;

    public void Debit(int amount)
    {
        if (!CanAfford(amount))
            throw new InvalidOperationException("Insufficient balance");
        CoinBalance -= amount;
        TotalSpent += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Credit(int amount)
    {
        CoinBalance += amount;
        TotalPurchased += amount;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public TransactionType Type { get; set; }
    public int Amount { get; set; }
    public int BalanceAfter { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
}

public class GiftCatalogItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAz { get; set; }
    public GiftTier Tier { get; set; }
    public int CoinPrice { get; set; }
    public string? AnimationUrl { get; set; }
    public string? IconUrl { get; set; }
    public Guid? VenueId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class CoinPackage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAz { get; set; }
    public decimal PriceAzn { get; set; }
    public int Coins { get; set; }
    public int BonusCoins { get; set; }
    public string? IosProductId { get; set; }
    public string? AndroidProductId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
