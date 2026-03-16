using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Economy.Commands;

public record PurchaseCoinsCommand(
    string Platform,
    string ReceiptData,
    string ProductId
) : IRequest<Result<PurchaseResponse>>
{
    public Guid UserId { get; init; }
}
