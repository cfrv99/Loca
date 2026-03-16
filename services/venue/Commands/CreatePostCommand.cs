using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Venue.Commands;

public record CreatePostCommand(
    Guid VenueId,
    string? Content,
    List<string>? MediaUrls
) : IRequest<Result<PostDto>>
{
    public Guid UserId { get; init; }
}
