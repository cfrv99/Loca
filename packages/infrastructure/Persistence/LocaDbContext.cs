using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence;

public class LocaDbContext : DbContext
{
    public LocaDbContext(DbContextOptions<LocaDbContext> options) : base(options) { }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Venue
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<QrCode> QrCodes => Set<QrCode>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    // Social
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();

    // Matching
    public DbSet<MatchRequest> MatchRequests => Set<MatchRequest>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();

    // Game
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();

    // Economy
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Gift> Gifts => Set<Gift>();
    public DbSet<GiftTransaction> GiftTransactions => Set<GiftTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable PostGIS
        modelBuilder.HasPostgresExtension("postgis");

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocaDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Venue>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Post>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ChatMessage>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
