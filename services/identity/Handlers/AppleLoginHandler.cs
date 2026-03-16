using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Identity.Handlers;

public class AppleLoginHandler : IRequestHandler<AppleLoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AppleLoginHandler> _logger;

    public AppleLoginHandler(IUserRepository users, ITokenService tokenService, ILogger<AppleLoginHandler> logger)
    {
        _users = users;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(AppleLoginCommand cmd, CancellationToken ct)
    {
        try
        {
            // In production: validate Apple identity token with Apple's public keys
            // For development: accept the token as-is and extract mock data
            var providerId = cmd.IdentityToken;
            var user = await _users.GetByAuthProviderAsync(AuthProvider.Apple, providerId, ct);
            var isNewUser = false;

            if (user is null)
            {
                isNewUser = true;
                user = new User
                {
                    Email = $"apple_{Guid.NewGuid():N}@loca.az",
                    DisplayName = cmd.FullName ?? "Apple istifadəçi",
                    DateOfBirth = DateTime.UtcNow.AddYears(-25),
                    Gender = Gender.PreferNotToSay,
                    AuthProvider = AuthProvider.Apple,
                    AuthProviderId = providerId,
                };
                await _users.AddAsync(user, ct);
                _logger.LogInformation("New user registered via Apple: {UserId}", user.Id);
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = _tokenService.HashToken(refreshTokenValue),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            };
            await _users.AddRefreshTokenAsync(refreshToken, ct);

            var userDto = MapToDto(user);
            return Result<AuthResponse>.Success(new AuthResponse(accessToken, refreshTokenValue, userDto, isNewUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Apple login failed");
            return Result<AuthResponse>.Failure("AUTH_FAILED", "Apple authentication failed");
        }
    }

    private static UserDto MapToDto(User user) => new(
        Id: user.Id,
        Email: user.Email,
        DisplayName: user.DisplayName,
        AvatarUrl: user.AvatarUrl,
        DateOfBirth: user.DateOfBirth,
        Gender: user.Gender.ToString(),
        Interests: user.Interests,
        Purposes: user.Purposes,
        VibePreferences: user.VibePreferences.Select(v => new VibePreferenceDto(v.Vibe, v.Weight)).ToList(),
        IsOnboarded: user.IsOnboarded,
        IsPremium: user.IsPremium,
        CoinBalance: 0,
        CreatedAt: user.CreatedAt
    );
}
