using Loca.Domain.Entities;

namespace Loca.Domain.Interfaces;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Venue?> GetByQrPayloadAsync(string qrPayload, CancellationToken ct = default);
    Task<List<Venue>> GetNearbyAsync(double lat, double lng, double radiusMeters, int limit = 20, CancellationToken ct = default);
    Task<List<Venue>> GetAllActiveAsync(CancellationToken ct = default);
    Task AddAsync(Venue venue, CancellationToken ct = default);
    Task UpdateAsync(Venue venue, CancellationToken ct = default);
}
