using Loca.Domain.Entities;

namespace Loca.Domain.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Post>> GetVenueFeedAsync(Guid venueId, int pageSize, string? cursor, CancellationToken ct = default);
    Task AddAsync(Post post, CancellationToken ct = default);
    Task UpdateAsync(Post post, CancellationToken ct = default);

    // Likes
    Task<Like?> GetLikeAsync(Guid postId, Guid userId, CancellationToken ct = default);
    Task AddLikeAsync(Like like, CancellationToken ct = default);
    Task RemoveLikeAsync(Like like, CancellationToken ct = default);

    // Comments
    Task<List<Comment>> GetCommentsAsync(Guid postId, int pageSize, string? cursor, CancellationToken ct = default);
    Task AddCommentAsync(Comment comment, CancellationToken ct = default);
}
