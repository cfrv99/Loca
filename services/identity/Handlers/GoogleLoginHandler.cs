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

public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GoogleLoginHandler> _logger;

    public GoogleLoginHandler(IUserRepository users, ITokenService tokenService, ILogger<GoogleLoginHandler> logger)
    {
        _users = users;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand cmd, CancellationToken ct)
    {
        try
        {
            // In production: validate Google ID token with Google.Apis.Auth
            // For development: accept the token as-is and extract mock data
            // var payload = await GoogleJsonWebSignature.ValidateAsync(cmd.IdToken);

            // Dev mode: create/find user with token as identifier
            var providerId = cmd.IdToken;
            var user = await _users.GetByAuthProviderAsync(AuthProvider.Google, providerId, ct);
            var isNewUser = false;

            if (user is null)
            {
                isNewUser = true;
                user = new User
                {
                    Email = $"user_{Guid.NewGuid():N}@loca.az",
                    DisplayName = "Yeni istifadəçi",
                    DateOfBirth = DateTime.UtcNow.AddYears(-25),
                    Gender = Gender.PreferNotToSay,
                    AuthProvider = AuthProvider.Google,
                    AuthProviderId = providerId,
                };
                await _users.AddAsync(user, ct);
                _logger.LogInformation("New user registered via Google: {UserId}", user.Id);
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
            _logger.LogError(ex, "Google login failed");
            return Result<AuthResponse>.Failure("AUTH_FAILED", "Google authentication failed");
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
