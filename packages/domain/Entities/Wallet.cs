using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public int Balance { get; set; } // In coins
    public int TotalEarned { get; set; }
    public int TotalSpent { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public List<Transaction> Transactions { get; set; } = new();
}

public class Transaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public TransactionType Type { get; set; }
    public int Amount { get; set; }
    public int BalanceAfter { get; set; }
    public string? Description { get; set; }
    public string? ReferenceId { get; set; } // Gift ID, IAP receipt, etc.

    // Navigation
    public Wallet Wallet { get; set; } = null!;
}

public class Gift : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CoinPrice { get; set; }
    public GiftTier Tier { get; set; }
    public string AnimationUrl { get; set; } = string.Empty; // Lottie JSON URL
    public string IconUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class GiftTransaction : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid GiftId { get; set; }
    public Guid? VenueId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public int CoinAmount { get; set; }

    // Navigation
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public Gift GiftItem { get; set; } = null!;
}
