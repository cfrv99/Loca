using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly LocaDbContext _db;

    public VenueRepository(LocaDbContext db) => _db = db;

    public async Task<Venue?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Venues.FirstOrDefaultAsync(v => v.Id == id && v.IsActive, ct);

    public async Task<Venue?> GetByQrSecretKeyValidatingPayload(string qrPayload, CancellationToken ct = default)
    {
        // Check all active venues' TOTP against the payload
        var venues = await _db.Venues.Where(v => v.IsActive).ToListAsync(ct);
        return venues.FirstOrDefault(v => QrCodeGenerator.ValidateTotp(v.QrSecretKey, qrPayload));
    }

    public async Task<List<Venue>> GetNearbyAsync(double lat, double lng, int radiusMeters, string? category, int limit, string? cursor, CancellationToken ct = default)
    {
        var query = _db.Venues.Where(v => v.IsActive);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(v => v.Category.ToString() == category);

        var venues = await query.ToListAsync(ct);

        // Calculate distance and filter
        var results = venues
            .Select(v => new { Venue = v, Distance = Venue.CalculateDistance(lat, lng, v.Latitude, v.Longitude) })
            .Where(x => x.Distance <= radiusMeters)
            .OrderBy(x => x.Distance)
            .ToList();

        // Cursor pagination
        if (!string.IsNullOrEmpty(cursor) && Guid.TryParse(cursor, out var cursorId))
        {
            var cursorIndex = results.FindIndex(x => x.Venue.Id == cursorId);
            if (cursorIndex >= 0)
                results = results.Skip(cursorIndex + 1).ToList();
        }

        return results.Take(limit).Select(x =>
        {
            // Store distance in a transient way (we'll use DTO mapping)
            return x.Venue;
        }).ToList();
    }

    public async Task AddAsync(Venue venue, CancellationToken ct = default)
    {
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Venue venue, CancellationToken ct = default)
    {
        venue.UpdatedAt = DateTime.UtcNow;
        _db.Venues.Update(venue);
        await _db.SaveChangesAsync(ct);
    }
}
