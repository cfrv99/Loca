using Loca.Domain.Entities;

namespace Loca.Domain.Interfaces;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Venue?> GetByQrSecretKeyValidatingPayload(string qrPayload, CancellationToken ct = default);
    Task<List<Venue>> GetNearbyAsync(double lat, double lng, int radiusMeters, string? category, int limit, string? cursor, CancellationToken ct = default);
    Task AddAsync(Venue venue, CancellationToken ct = default);
    Task UpdateAsync(Venue venue, CancellationToken ct = default);
}
