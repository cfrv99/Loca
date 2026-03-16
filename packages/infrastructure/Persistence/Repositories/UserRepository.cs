using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LocaDbContext _context;

    public UserRepository(LocaDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    public async Task<User?> GetByAppleIdAsync(string appleId, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.AppleId == appleId, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Id == id, ct);
}
