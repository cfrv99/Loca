using Loca.Domain.Entities;

namespace Loca.Domain.Interfaces;

public interface ICheckInRepository
{
    Task<CheckIn?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CheckIn?> GetActiveByUserAndVenueAsync(Guid userId, Guid venueId, CancellationToken ct = default);
    Task<CheckIn?> GetRecentAsync(Guid userId, Guid venueId, TimeSpan window, CancellationToken ct = default);
    Task<List<CheckIn>> GetActiveByVenueAsync(Guid venueId, CancellationToken ct = default);
    Task AddAsync(CheckIn checkIn, CancellationToken ct = default);
    Task UpdateAsync(CheckIn checkIn, CancellationToken ct = default);
}
