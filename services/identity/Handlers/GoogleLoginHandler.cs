using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Identity.Handlers;

public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, Result<LoginResultDto>>
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

    public async Task<Result<LoginResultDto>> Handle(GoogleLoginCommand cmd, CancellationToken ct)
    {
        // In production, validate the Google ID token with Google's API
        // For now, we extract info from the token payload (simplified)
        var (email, googleId, name) = DecodeGoogleToken(cmd.IdToken);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            return Result<LoginResultDto>.Failure("INVALID_TOKEN", "Invalid Google ID token");

        var isNewUser = false;
        var user = await _users.GetByGoogleIdAsync(googleId, ct);

        if (user is null)
        {
            // Check if email already exists
            user = await _users.GetByEmailAsync(email, ct);
            if (user is not null)
            {
                // Link Google account to existing user
                user.GoogleId = googleId;
                await _users.UpdateAsync(user, ct);
            }
            else
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    GoogleId = googleId,
                    DisplayName = name ?? email.Split('@')[0],
                    Profile = new UserProfile(),
                    Wallet = new Wallet()
                };
                await _users.AddAsync(user, ct);
                isNewUser = true;
                _logger.LogInformation("New user registered via Google: {UserId} ({Email})", user.Id, email);
            }
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            DeviceInfo = cmd.DeviceInfo
        });
        user.LastActiveAt = DateTime.UtcNow;
        await _users.UpdateAsync(user, ct);

        _logger.LogInformation("User logged in via Google: {UserId}", user.Id);

        return Result<LoginResultDto>.Success(new LoginResultDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            User: MapToDto(user),
            IsNewUser: isNewUser
        ));
    }

    private static (string? Email, string? GoogleId, string? Name) DecodeGoogleToken(string idToken)
    {
        // Simplified: In production, use Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync
        // For development, we accept a formatted token: "google:{email}:{googleId}:{name}"
        if (idToken.StartsWith("google:"))
        {
            var parts = idToken.Split(':');
            if (parts.Length >= 4)
                return (parts[1], parts[2], parts[3]);
        }

        return (null, null, null);
    }

    private static UserDto MapToDto(User user) => new(
        Id: user.Id,
        Email: user.Email,
        DisplayName: user.DisplayName,
        FirstName: user.FirstName,
        LastName: user.LastName,
        ProfilePhotoUrl: user.ProfilePhotoUrl,
        Bio: user.Bio,
        IsOnboardingComplete: user.IsOnboardingComplete,
        CreatedAt: user.CreatedAt
    );
}
