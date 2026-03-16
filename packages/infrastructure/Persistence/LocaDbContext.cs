using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence;

public class LocaDbContext : DbContext
{
    public LocaDbContext(DbContextOptions<LocaDbContext> options) : base(options) { }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
    public DbSet<UserPurpose> UserPurposes => Set<UserPurpose>();
    public DbSet<UserVibePreference> UserVibePreferences => Set<UserVibePreference>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Venue
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    // Social
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<MatchRequest> MatchRequests => Set<MatchRequest>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Report> Reports => Set<Report>();

    // Game
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();

    // Economy
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<GiftCatalogItem> GiftCatalog => Set<GiftCatalogItem>();
    public DbSet<CoinPackage> CoinPackages => Set<CoinPackage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocaDbContext).Assembly);
    }
}
