using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Venue.Commands;

public record AddCommentCommand(
    Guid PostId,
    string Content
) : IRequest<Result<CommentDto>>
{
    public Guid UserId { get; init; }
}
