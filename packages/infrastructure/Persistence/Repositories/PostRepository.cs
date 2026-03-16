using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private readonly LocaDbContext _db;

    public PostRepository(LocaDbContext db) => _db = db;

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

    public async Task<List<Post>> GetVenueFeedAsync(Guid venueId, int pageSize, string? cursor, CancellationToken ct = default)
    {
        var query = _db.Posts
            .Include(p => p.User)
            .Where(p => p.VenueId == venueId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt);

        if (!string.IsNullOrEmpty(cursor) && Guid.TryParse(cursor, out var cursorId))
        {
            var cursorItem = await _db.Posts.FirstOrDefaultAsync(p => p.Id == cursorId, ct);
            if (cursorItem is not null)
                query = (IOrderedQueryable<Post>)query.Where(p => p.CreatedAt < cursorItem.CreatedAt);
        }

        return await query.Take(pageSize).ToListAsync(ct);
    }

    public async Task AddAsync(Post post, CancellationToken ct = default)
    {
        _db.Posts.Add(post);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        post.UpdatedAt = DateTime.UtcNow;
        _db.Posts.Update(post);
        await _db.SaveChangesAsync(ct);
    }

    // Likes
    public async Task<Like?> GetLikeAsync(Guid postId, Guid userId, CancellationToken ct = default)
        => await _db.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId, ct);

    public async Task AddLikeAsync(Like like, CancellationToken ct = default)
    {
        _db.Likes.Add(like);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveLikeAsync(Like like, CancellationToken ct = default)
    {
        _db.Likes.Remove(like);
        await _db.SaveChangesAsync(ct);
    }

    // Comments
    public async Task<List<Comment>> GetCommentsAsync(Guid postId, int pageSize, string? cursor, CancellationToken ct = default)
    {
        var query = _db.Comments
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt);

        if (!string.IsNullOrEmpty(cursor) && Guid.TryParse(cursor, out var cursorId))
        {
            var cursorItem = await _db.Comments.FirstOrDefaultAsync(c => c.Id == cursorId, ct);
            if (cursorItem is not null)
                query = (IOrderedQueryable<Comment>)query.Where(c => c.CreatedAt < cursorItem.CreatedAt);
        }

        return await query.Take(pageSize).ToListAsync(ct);
    }

    public async Task AddCommentAsync(Comment comment, CancellationToken ct = default)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);
    }
}
