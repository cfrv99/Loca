using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class CheckInRepository : ICheckInRepository
{
    private readonly LocaDbContext _db;

    public CheckInRepository(LocaDbContext db) => _db = db;

    public async Task<CheckIn?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.CheckIns.Include(c => c.Venue).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<CheckIn?> GetActiveCheckInAsync(Guid userId, Guid venueId, CancellationToken ct = default)
        => await _db.CheckIns.FirstOrDefaultAsync(c =>
            c.UserId == userId && c.VenueId == venueId && c.CheckOutAt == null, ct);

    public async Task<CheckIn?> GetActiveCheckInByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.CheckIns
            .Include(c => c.Venue)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CheckOutAt == null, ct);

    public async Task<CheckIn?> GetRecentAsync(Guid userId, Guid venueId, TimeSpan window, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - window;
        return await _db.CheckIns.FirstOrDefaultAsync(c =>
            c.UserId == userId && c.VenueId == venueId && c.CheckInAt >= cutoff, ct);
    }

    public async Task<List<CheckIn>> GetActiveCheckInsForVenueAsync(Guid venueId, CancellationToken ct = default)
        => await _db.CheckIns
            .Include(c => c.User)
            .Where(c => c.VenueId == venueId && c.CheckOutAt == null)
            .ToListAsync(ct);

    public async Task AddAsync(CheckIn checkIn, CancellationToken ct = default)
    {
        _db.CheckIns.Add(checkIn);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CheckIn checkIn, CancellationToken ct = default)
    {
        checkIn.UpdatedAt = DateTime.UtcNow;
        _db.CheckIns.Update(checkIn);
        await _db.SaveChangesAsync(ct);
    }
}
