namespace Loca.Application.DTOs;

public record BalanceDto(int CoinBalance, int TotalPurchased, int TotalSpent);

public record PurchaseRequest(string Platform, string ReceiptData, string ProductId);

public record PurchaseResponse(int CoinsAdded, int NewBalance, Guid TransactionId);

public record GiftDto(
    Guid Id,
    string Name,
    string? NameAz,
    string Tier,
    int CoinPrice,
    string? IconUrl,
    string? AnimationUrl,
    Guid? VenueId
);

public record SendGiftRequest(
    Guid GiftId,
    Guid RecipientId,
    string Context, // public_chat or private_chat
    Guid? VenueId,
    Guid? ConversationId
);

public record SendGiftResponse(Guid TransactionId, int NewBalance);
