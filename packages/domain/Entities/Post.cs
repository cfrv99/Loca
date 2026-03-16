using Loca.Domain.Common;

namespace Loca.Domain.Entities;

public class Post : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid VenueId { get; set; }
    public string? Caption { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsPublic { get; set; } = true;

    // Navigation
    public User User { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public List<Comment> Comments { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
}

public class Comment : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;

    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class Like : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }

    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}
