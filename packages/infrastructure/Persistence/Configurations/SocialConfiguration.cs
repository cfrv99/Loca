using Loca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loca.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("messages", "social");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.RoomId).HasMaxLength(100).IsRequired();
        builder.Property(m => m.MessageType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.MediaUrl).HasMaxLength(500);
        builder.Property(m => m.MetadataJson).HasColumnType("jsonb");

        builder.HasIndex(m => new { m.RoomId, m.CreatedAt })
            .IsDescending(false, true);

        builder.HasOne(m => m.ReplyTo).WithMany().HasForeignKey(m => m.ReplyToId);
    }
}

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("message_reactions", "social");
        builder.HasKey(r => new { r.MessageId, r.UserId, r.Emoji });
        builder.Property(r => r.Emoji).HasMaxLength(10).IsRequired();
    }
}

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts", "social");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.MediaType).HasMaxLength(20).HasDefaultValue("photo");

        builder.HasIndex(p => new { p.VenueId, p.CreatedAt })
            .IsDescending(false, true);
        builder.HasIndex(p => new { p.UserId, p.CreatedAt })
            .IsDescending(false, true);

        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
        builder.HasOne(p => p.Venue).WithMany().HasForeignKey(p => p.VenueId);
    }
}

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("likes", "social");
        builder.HasKey(l => new { l.PostId, l.UserId });
    }
}

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments", "social");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).HasMaxLength(500).IsRequired();
        builder.HasIndex(c => new { c.PostId, c.CreatedAt })
            .IsDescending(false, true);
    }
}

public class MatchRequestConfiguration : IEntityTypeConfiguration<MatchRequest>
{
    public void Configure(EntityTypeBuilder<MatchRequest> builder)
    {
        builder.ToTable("match_requests", "social");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.IntroMessage).HasMaxLength(200);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(m => new { m.ReceiverId, m.Status });
        builder.HasIndex(m => new { m.SenderId, m.CreatedAt })
            .HasFilter("status = 'Pending'");
    }
}

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations", "social");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.LastMessagePreview).HasMaxLength(100);
    }
}

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.ToTable("blocks", "social");
        builder.HasKey(b => new { b.BlockerId, b.BlockedId });
    }
}

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports", "social");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Reason).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
    }
}
