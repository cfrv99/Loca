using FluentAssertions;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Commands;
using Loca.Services.Identity.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class GoogleLoginHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly ILogger<GoogleLoginHandler> _logger = Substitute.For<ILogger<GoogleLoginHandler>>();
    private readonly GoogleLoginHandler _handler;

    public GoogleLoginHandlerTests()
    {
        _handler = new GoogleLoginHandler(_users, _tokenService, _logger);
    }

    [Fact]
    public async Task Should_CreateNewUser_When_FirstLogin()
    {
        _users.GetByAuthProviderAsync(AuthProvider.Google, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");
        _tokenService.HashToken(Arg.Any<string>()).Returns("hashed-token");

        var cmd = new GoogleLoginCommand("test-google-token");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsNewUser.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value!.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Should_ReturnExistingUser_When_AlreadyRegistered()
    {
        var existingUser = new User
        {
            Email = "test@loca.az",
            DisplayName = "Test User",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male,
            AuthProvider = AuthProvider.Google,
            AuthProviderId = "test-google-token"
        };

        _users.GetByAuthProviderAsync(AuthProvider.Google, "test-google-token", Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");
        _tokenService.HashToken(Arg.Any<string>()).Returns("hashed-token");

        var cmd = new GoogleLoginCommand("test-google-token");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsNewUser.Should().BeFalse();
        result.Value!.User.DisplayName.Should().Be("Test User");
    }
}
