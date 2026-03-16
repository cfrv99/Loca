using Loca.Domain.Common;

namespace Loca.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? InstagramHandle { get; set; }
    public string? SpotifyId { get; set; }
    public int TotalCheckIns { get; set; }
    public int TotalGamesPlayed { get; set; }
    public int TotalGiftsReceived { get; set; }
    public int TotalMatchesMade { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
