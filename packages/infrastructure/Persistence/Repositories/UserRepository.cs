using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LocaDbContext _db;

    public UserRepository(LocaDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Users
            .Include(u => u.VibePreferences)
            .Include(u => u.RefreshTokens.Where(rt => rt.RevokedAt == null))
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByAuthProviderAsync(AuthProvider provider, string providerId, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.AuthProvider == provider && u.AuthProviderId == providerId, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken ct = default)
        => await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null, ct);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeRefreshTokenAsync(Guid tokenId, CancellationToken ct = default)
    {
        var token = await _db.RefreshTokens.FindAsync(new object[] { tokenId }, ct);
        if (token is not null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
