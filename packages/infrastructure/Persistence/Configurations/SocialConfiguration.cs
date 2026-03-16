using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("chat_rooms", "social");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.VenueId);
        builder.Property(c => c.Name).HasMaxLength(200);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("messages", "social");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.ChatRoomId, m.CreatedAt });

        builder.Property(m => m.Content).HasMaxLength(2000).IsRequired();
        builder.Property(m => m.MediaUrl).HasMaxLength(500);
        builder.Property(m => m.GiftId).HasMaxLength(100);

        builder.HasOne(m => m.ChatRoom)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.ChatRoomId);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId);
    }
}

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts", "social");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.VenueId, p.CreatedAt });
        builder.Property(p => p.Caption).HasMaxLength(1000);

        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
        builder.HasOne(p => p.Venue).WithMany().HasForeignKey(p => p.VenueId);
    }
}

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments", "social");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).HasMaxLength(500).IsRequired();
        builder.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId);
        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
    }
}

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("likes", "social");
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
        builder.HasOne(l => l.Post).WithMany(p => p.Likes).HasForeignKey(l => l.PostId);
        builder.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId);
    }
}

public class MatchRequestConfiguration : IEntityTypeConfiguration<MatchRequest>
{
    public void Configure(EntityTypeBuilder<MatchRequest> builder)
    {
        builder.ToTable("match_requests", "matching");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.SenderId, m.ReceiverId });
        builder.Property(m => m.IntroMessage).HasMaxLength(500);

        builder.HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Receiver).WithMany().HasForeignKey(m => m.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Venue).WithMany().HasForeignKey(m => m.VenueId);
    }
}

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations", "matching");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.User1Id, c.User2Id }).IsUnique();

        builder.HasOne(c => c.MatchRequest).WithOne(m => m.Conversation).HasForeignKey<Conversation>(c => c.MatchRequestId);
        builder.HasOne(c => c.User1).WithMany().HasForeignKey(c => c.User1Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.User2).WithMany().HasForeignKey(c => c.User2Id).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable("user_blocks", "social");
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => new { b.BlockerId, b.BlockedId }).IsUnique();
        builder.Property(b => b.Description).HasMaxLength(500);

        builder.HasOne(b => b.Blocker).WithMany().HasForeignKey(b => b.BlockerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Blocked).WithMany().HasForeignKey(b => b.BlockedId).OnDelete(DeleteBehavior.Restrict);
    }
}
