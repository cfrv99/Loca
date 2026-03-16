using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class CheckInRepository : ICheckInRepository
{
    private readonly LocaDbContext _context;

    public CheckInRepository(LocaDbContext context) => _context = context;

    public async Task<CheckIn?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.CheckIns.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<CheckIn?> GetActiveByUserAndVenueAsync(Guid userId, Guid venueId, CancellationToken ct = default)
        => await _context.CheckIns
            .FirstOrDefaultAsync(c => c.UserId == userId && c.VenueId == venueId && c.Status == CheckInStatus.Active, ct);

    public async Task<CheckIn?> GetRecentAsync(Guid userId, Guid venueId, TimeSpan window, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - window;
        return await _context.CheckIns
            .FirstOrDefaultAsync(c => c.UserId == userId && c.VenueId == venueId && c.CheckInAt > cutoff, ct);
    }

    public async Task<List<CheckIn>> GetActiveByVenueAsync(Guid venueId, CancellationToken ct = default)
        => await _context.CheckIns
            .Include(c => c.User)
            .Where(c => c.VenueId == venueId && c.Status == CheckInStatus.Active)
            .ToListAsync(ct);

    public async Task AddAsync(CheckIn checkIn, CancellationToken ct = default)
    {
        await _context.CheckIns.AddAsync(checkIn, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CheckIn checkIn, CancellationToken ct = default)
    {
        _context.CheckIns.Update(checkIn);
        await _context.SaveChangesAsync(ct);
    }
}
