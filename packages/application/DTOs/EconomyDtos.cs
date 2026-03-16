namespace Loca.Application.DTOs;

public record WalletDto(
    Guid Id,
    int Balance,
    int TotalEarned,
    int TotalSpent
);

public record GiftCatalogItemDto(
    Guid Id,
    string Name,
    string Description,
    int CoinPrice,
    string Tier,
    string AnimationUrl,
    string IconUrl,
    int SortOrder
);

public record SendGiftResultDto(
    Guid TransactionId,
    string GiftName,
    string AnimationUrl,
    int CoinCost,
    int RemainingBalance
);

public record CoinPackageDto(
    string PackageId,
    string Name,
    int CoinAmount,
    decimal PriceAzn,
    string? BonusLabel
);

public record TransactionDto(
    Guid Id,
    string Type,
    int Amount,
    int BalanceAfter,
    string? Description,
    DateTime CreatedAt
);
