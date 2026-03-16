using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? GoogleId { get; set; }
    public string? AppleId { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsOnboardingComplete { get; set; }
    public DateTime? LastActiveAt { get; set; }

    // Onboarding data
    public List<string> Interests { get; set; } = new();
    public List<UserPurpose> Purposes { get; set; } = new();
    public VibePreference? VibePreference { get; set; }
    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.Public;

    // Navigation
    public UserProfile? Profile { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<CheckIn> CheckIns { get; set; } = new();
    public Wallet? Wallet { get; set; }
}
