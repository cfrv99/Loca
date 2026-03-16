using Loca.Domain.Entities;

namespace Loca.Domain.Interfaces;

public interface ICheckInRepository
{
    Task<CheckIn?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CheckIn?> GetActiveCheckInAsync(Guid userId, Guid venueId, CancellationToken ct = default);
    Task<CheckIn?> GetActiveCheckInByUserAsync(Guid userId, CancellationToken ct = default);
    Task<CheckIn?> GetRecentAsync(Guid userId, Guid venueId, TimeSpan window, CancellationToken ct = default);
    Task<List<CheckIn>> GetActiveCheckInsForVenueAsync(Guid venueId, CancellationToken ct = default);
    Task AddAsync(CheckIn checkIn, CancellationToken ct = default);
    Task UpdateAsync(CheckIn checkIn, CancellationToken ct = default);
}
