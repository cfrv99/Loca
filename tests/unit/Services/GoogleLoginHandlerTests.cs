using FluentAssertions;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Commands;
using Loca.Services.Identity.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class GoogleLoginHandlerTests
{
    private readonly IUserRepository _userRepo;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GoogleLoginHandler> _logger;
    private readonly GoogleLoginHandler _handler;

    public GoogleLoginHandlerTests()
    {
        _userRepo = Substitute.For<IUserRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<GoogleLoginHandler>>();
        _handler = new GoogleLoginHandler(_userRepo, _tokenService, _logger);
    }

    [Fact]
    public async Task Should_CreateNewUser_When_GoogleIdNotFound()
    {
        var cmd = new GoogleLoginCommand("google:test@example.com:google123:Test User", "iPhone 15");
        _userRepo.GetByGoogleIdAsync("google123", Arg.Any<CancellationToken>()).Returns((User?)null);
        _userRepo.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);
        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token-123");
        _tokenService.GenerateRefreshToken().Returns("refresh-token-123");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsNewUser.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token-123");
        result.Value.RefreshToken.Should().Be("refresh-token-123");
        result.Value.User.Email.Should().Be("test@example.com");

        await _userRepo.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnExistingUser_When_GoogleIdFound()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            GoogleId = "google123",
            DisplayName = "Existing User",
            RefreshTokens = new List<RefreshToken>()
        };

        var cmd = new GoogleLoginCommand("google:test@example.com:google123:Test User", "iPhone 15");
        _userRepo.GetByGoogleIdAsync("google123", Arg.Any<CancellationToken>()).Returns(existingUser);
        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token-123");
        _tokenService.GenerateRefreshToken().Returns("refresh-token-123");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsNewUser.Should().BeFalse();
        result.Value.User.Id.Should().Be(existingUser.Id);

        await _userRepo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnError_When_InvalidGoogleToken()
    {
        var cmd = new GoogleLoginCommand("invalid-token", "iPhone 15");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("INVALID_TOKEN");
    }
}
