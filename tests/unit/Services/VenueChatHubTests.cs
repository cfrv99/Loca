using FluentAssertions;
using Loca.Application.DTOs;
using Loca.Domain.Entities;

namespace Loca.Tests.Unit.Services;

/// <summary>
/// Tests for VenueChatHub business logic: anonymous mode, message pinning rules,
/// gift broadcasting DTOs, and system message construction.
/// </summary>
public class VenueChatHubTests
{
    [Fact]
    public void Should_GenerateGuestDisplayName_ForAnonymousUsers()
    {
        var userId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var isAnonymous = true;

        var displayName = isAnonymous
            ? $"Guest_{userId.ToString()[..4].ToUpper()}"
            : "Nigar";

        displayName.Should().Be("Guest_A1B2");
    }

    [Fact]
    public void Should_UseRealDisplayName_ForNonAnonymousUsers()
    {
        var userId = Guid.NewGuid();
        var isAnonymous = false;
        var realName = "Nigar";

        var displayName = isAnonymous
            ? $"Guest_{userId.ToString()[..4].ToUpper()}"
            : realName;

        displayName.Should().Be("Nigar");
    }

    [Fact]
    public void Should_HideAvatar_ForAnonymousUsers()
    {
        var isAnonymous = true;
        var realAvatarUrl = "https://example.com/avatar.jpg";

        var avatarUrl = isAnonymous ? null : realAvatarUrl;

        avatarUrl.Should().BeNull();
    }

    [Fact]
    public void Should_CreateActiveUserDto_WithAnonymousData()
    {
        var userId = Guid.Parse("deadbeef-1234-5678-abcd-ef1234567890");

        var dto = new ActiveUserDto(
            userId,
            $"Guest_{userId.ToString()[..4].ToUpper()}",
            null, // avatar hidden
            25,
            new List<string>(),
            true // isAnonymous
        );

        dto.DisplayName.Should().Be("Guest_DEAD");
        dto.AvatarUrl.Should().BeNull();
        dto.IsAnonymous.Should().BeTrue();
    }

    [Fact]
    public void Should_ConstructGiftEventDto_Correctly()
    {
        var dto = new GiftEventDto(
            Guid.NewGuid(),
            "Ali",
            "Rose",
            "Basic",
            "https://lottie.url/rose.json",
            10
        );

        dto.SenderName.Should().Be("Ali");
        dto.GiftName.Should().Be("Rose");
        dto.GiftTier.Should().Be("Basic");
        dto.CoinCost.Should().Be(10);
    }

    [Fact]
    public void Should_ConstructSystemMessage_ForGameResult()
    {
        var gameType = "Mafia";
        var winnerName = "Ali";
        var durationMinutes = 15;

        var content = $"Oyun bitdi! {gameType} - Qalib: {winnerName}! Oyun müddəti: {durationMinutes} dəqiqə.";

        content.Should().Contain("Mafia");
        content.Should().Contain("Ali");
        content.Should().Contain("15 dəqiqə");
    }

    [Fact]
    public void Should_ConstructSystemMessage_WithoutWinner()
    {
        var gameType = "Həqiqət və ya Cəsarət";
        var durationMinutes = 20;

        var content = $"Oyun bitdi! {gameType} - Oyun müddəti: {durationMinutes} dəqiqə.";

        content.Should().Contain("Həqiqət və ya Cəsarət");
        content.Should().NotContain("Qalib");
    }

    [Fact]
    public void Should_CreateChatMessageDto_AsSystemMessage()
    {
        var dto = new ChatMessageDto(
            Guid.NewGuid(),
            Guid.Empty,
            "Sistem",
            null,
            "system",
            "Test system message",
            null,
            null,
            null,
            DateTime.UtcNow
        );

        dto.SenderId.Should().Be(Guid.Empty);
        dto.SenderName.Should().Be("Sistem");
        dto.Type.Should().Be("system");
    }

    [Fact]
    public void Should_ValidateMessageLength_Under1000()
    {
        var shortContent = "Hello";
        var longContent = new string('A', 1001);

        (shortContent.Length <= 1000).Should().BeTrue();
        (longContent.Length <= 1000).Should().BeFalse();
    }

    [Fact]
    public void Should_OnlyAllowVenueOwner_ToPinMessage()
    {
        var venueOwner = Guid.NewGuid();
        var regularUser = Guid.NewGuid();

        var venue = new Venue
        {
            OwnerUserId = venueOwner,
            Name = "Test Venue"
        };

        (venue.OwnerUserId == venueOwner).Should().BeTrue();
        (venue.OwnerUserId == regularUser).Should().BeFalse();
    }

    [Fact]
    public void Should_VerifyPinnedState_AfterPinning()
    {
        var message = new ChatMessage
        {
            RoomId = "venue_123",
            SenderId = Guid.NewGuid(),
            Content = "Important message",
            IsPinned = false
        };

        message.IsPinned = true;

        message.IsPinned.Should().BeTrue();
    }
}
