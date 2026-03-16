using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Loca.Infrastructure.Persistence.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly LocaDbContext _context;

    public VenueRepository(LocaDbContext context) => _context = context;

    public async Task<Venue?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Venues.FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Venue?> GetByQrPayloadAsync(string qrPayload, CancellationToken ct = default)
    {
        var qrCode = await _context.QrCodes
            .Include(q => q.Venue)
            .FirstOrDefaultAsync(q => q.Payload == qrPayload && q.IsActive && q.ExpiresAt > DateTime.UtcNow, ct);

        return qrCode?.Venue;
    }

    public async Task<List<Venue>> GetNearbyAsync(double lat, double lng, double radiusMeters, int limit = 20, CancellationToken ct = default)
    {
        var userLocation = new Point(lng, lat) { SRID = 4326 };

        return await _context.Venues
            .Where(v => v.IsActive && v.Location.IsWithinDistance(userLocation, radiusMeters / 111320.0))
            .OrderBy(v => v.Location.Distance(userLocation))
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<Venue>> GetAllActiveAsync(CancellationToken ct = default)
        => await _context.Venues.Where(v => v.IsActive).ToListAsync(ct);

    public async Task AddAsync(Venue venue, CancellationToken ct = default)
    {
        await _context.Venues.AddAsync(venue, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Venue venue, CancellationToken ct = default)
    {
        _context.Venues.Update(venue);
        await _context.SaveChangesAsync(ct);
    }
}
