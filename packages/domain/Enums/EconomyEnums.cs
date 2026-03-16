namespace Loca.Domain.Enums;

public enum TransactionType
{
    Purchase,
    GiftSent,
    GiftReceived,
    GameReward,
    SkipPenalty,
    VibeBomb,
    ChainPartyReward,
    Bonus
}

public enum GiftTier
{
    Basic,
    Premium,
    Luxury,
    VenueSpecific
}

public enum GiftContext
{
    PublicChat,
    PrivateChat
}
