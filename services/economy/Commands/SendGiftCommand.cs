using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Economy.Commands;

public record SendGiftCommand(
    Guid GiftId,
    Guid RecipientId,
    string Context, // "public_chat" or "private_chat"
    Guid? VenueId,
    Guid? ConversationId
) : IRequest<Result<SendGiftResponse>>
{
    public Guid UserId { get; init; }
}
