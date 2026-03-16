using Loca.Domain.Common;

namespace Loca.Domain.Entities;

public class Post : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid VenueId { get; set; }
    public string? Content { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public string MediaType { get; set; } = "photo";
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsMemory { get; set; } = true;
    public bool IsDeleted { get; set; }

    // Navigation
    public User? User { get; set; }
    public Venue? Venue { get; set; }
    public List<Like> Likes { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}

public class Like
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Comment : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
}
