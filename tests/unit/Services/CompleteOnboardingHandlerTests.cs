using FluentAssertions;
using Loca.Application.DTOs;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Commands;
using Loca.Services.Identity.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class CompleteOnboardingHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ILogger<CompleteOnboardingHandler> _logger = Substitute.For<ILogger<CompleteOnboardingHandler>>();
    private readonly CompleteOnboardingHandler _handler;

    public CompleteOnboardingHandlerTests()
    {
        _handler = new CompleteOnboardingHandler(_users, _logger);
    }

    [Fact]
    public async Task Should_ReturnError_When_UserNotFound()
    {
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var cmd = new CompleteOnboardingCommand(
            new List<string> { "Music" },
            new List<string> { "Friends" },
            new List<VibePreferenceDto> { new("Chill", 0.8m) },
            new PrivacySettingsDto(false, true)
        ) { UserId = Guid.NewGuid() };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Should_ReturnError_When_AlreadyOnboarded()
    {
        var user = new User { IsOnboarded = true, Gender = Gender.Male };
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var cmd = new CompleteOnboardingCommand(
            new List<string> { "Music" },
            new List<string> { "Friends" },
            new List<VibePreferenceDto> { new("Chill", 0.8m) },
            new PrivacySettingsDto(false, true)
        ) { UserId = user.Id };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("ALREADY_ONBOARDED");
    }

    [Fact]
    public async Task Should_SuccessfullyCompleteOnboarding_When_Valid()
    {
        var user = new User
        {
            DisplayName = "Test User",
            Email = "test@loca.az",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male,
            IsOnboarded = false
        };
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var cmd = new CompleteOnboardingCommand(
            new List<string> { "Music", "Sport" },
            new List<string> { "Friends", "Networking" },
            new List<VibePreferenceDto> { new("Chill", 0.8m), new("Party", 0.5m) },
            new PrivacySettingsDto(false, true)
        ) { UserId = user.Id };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsOnboarded.Should().BeTrue();
        result.Value.Interests.Should().Contain("Music");
        result.Value.Interests.Should().Contain("Sport");
        result.Value.Purposes.Should().Contain("Friends");
        await _users.Received(1).UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
