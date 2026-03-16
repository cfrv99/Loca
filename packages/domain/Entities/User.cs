using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public string? AuthProviderId { get; set; }
    public bool IsOnboarded { get; set; }
    public bool IsAnonymousDefault { get; set; }
    public bool IsPremium { get; set; }
    public DateTime? PremiumExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public List<string> Interests { get; set; } = new();
    public List<string> Purposes { get; set; } = new();
    public List<UserVibePreference> VibePreferences { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    public int GetAge() => (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 365.25);
    public bool IsAdult() => GetAge() >= 18;
}

public class UserVibePreference
{
    public Guid UserId { get; set; }
    public string Vibe { get; set; } = string.Empty;
    public decimal Weight { get; set; } = 1.0m;
}
