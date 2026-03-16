using Loca.Domain.Entities;
using Loca.Domain.Enums;

namespace Loca.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByAuthProviderAsync(AuthProvider provider, string providerId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(Guid tokenId, CancellationToken ct = default);
    Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken ct = default);
}
