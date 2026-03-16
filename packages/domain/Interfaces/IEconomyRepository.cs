using Loca.Domain.Entities;

namespace Loca.Domain.Interfaces;

public interface IEconomyRepository
{
    // Wallets
    Task<Wallet?> GetWalletAsync(Guid userId, CancellationToken ct = default);
    Task<Wallet> GetOrCreateWalletAsync(Guid userId, CancellationToken ct = default);
    Task UpdateWalletAsync(Wallet wallet, CancellationToken ct = default);

    // Transactions
    Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default);

    // Gift catalog
    Task<List<GiftCatalogItem>> GetGiftCatalogAsync(CancellationToken ct = default);
    Task<GiftCatalogItem?> GetGiftByIdAsync(Guid id, CancellationToken ct = default);

    // Coin packages
    Task<CoinPackage?> GetCoinPackageByProductIdAsync(string productId, CancellationToken ct = default);
    Task<List<CoinPackage>> GetActiveCoinPackagesAsync(CancellationToken ct = default);
}
