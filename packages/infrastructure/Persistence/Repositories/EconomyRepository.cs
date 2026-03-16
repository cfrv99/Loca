using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class EconomyRepository : IEconomyRepository
{
    private readonly LocaDbContext _db;

    public EconomyRepository(LocaDbContext db) => _db = db;

    public async Task<Wallet?> GetWalletAsync(Guid userId, CancellationToken ct = default)
        => await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);

    public async Task<Wallet> GetOrCreateWalletAsync(Guid userId, CancellationToken ct = default)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
        if (wallet is not null)
            return wallet;

        wallet = new Wallet { UserId = userId };
        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync(ct);
        return wallet;
    }

    public async Task UpdateWalletAsync(Wallet wallet, CancellationToken ct = default)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _db.Wallets.Update(wallet);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<GiftCatalogItem>> GetGiftCatalogAsync(CancellationToken ct = default)
        => await _db.GiftCatalog
            .Where(g => g.IsActive)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.CoinPrice)
            .ToListAsync(ct);

    public async Task<GiftCatalogItem?> GetGiftByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.GiftCatalog.FirstOrDefaultAsync(g => g.Id == id && g.IsActive, ct);

    public async Task<CoinPackage?> GetCoinPackageByProductIdAsync(string productId, CancellationToken ct = default)
        => await _db.CoinPackages.FirstOrDefaultAsync(cp =>
            cp.IsActive && (cp.IosProductId == productId || cp.AndroidProductId == productId), ct);

    public async Task<List<CoinPackage>> GetActiveCoinPackagesAsync(CancellationToken ct = default)
        => await _db.CoinPackages
            .Where(cp => cp.IsActive)
            .OrderBy(cp => cp.SortOrder)
            .ToListAsync(ct);
}
