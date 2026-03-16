using Loca.API.Extensions;
using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Services.Economy.Commands;
using Loca.Services.Economy.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loca.API.Controllers;

[ApiController]
[Route("api/v1/economy")]
[Authorize]
public class EconomyController : ControllerBase
{
    private readonly IMediator _mediator;

    public EconomyController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get current coin balance
    /// </summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(ApiResponse<BalanceDto>), 200)]
    public async Task<IActionResult> GetBalance()
    {
        var result = await _mediator.Send(new GetBalanceQuery(User.GetUserId()));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<BalanceDto>.Ok(data)),
            error => StatusCode(500, ApiResponse<BalanceDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get gift catalog
    /// </summary>
    [HttpGet("gifts")]
    [ProducesResponseType(typeof(ApiResponse<List<GiftDto>>), 200)]
    public async Task<IActionResult> GetGiftCatalog()
    {
        var result = await _mediator.Send(new GetGiftCatalogQuery());
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<List<GiftDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<List<GiftDto>>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Send a gift to another user
    /// </summary>
    [HttpPost("gifts/send")]
    [ProducesResponseType(typeof(ApiResponse<SendGiftResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SendGiftResponse>), 400)]
    public async Task<IActionResult> SendGift([FromBody] SendGiftRequest request)
    {
        var cmd = new SendGiftCommand(
            request.GiftId,
            request.RecipientId,
            request.Context,
            request.VenueId,
            request.ConversationId
        ) { UserId = User.GetUserId() };

        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<SendGiftResponse>.Ok(data)),
            error => error.Code switch
            {
                "INSUFFICIENT_BALANCE" => BadRequest(ApiResponse<SendGiftResponse>.Fail(error.Code, error.Message)),
                "GIFT_NOT_FOUND" => NotFound(ApiResponse<SendGiftResponse>.Fail(error.Code, error.Message)),
                _ => BadRequest(ApiResponse<SendGiftResponse>.Fail(error.Code, error.Message))
            }
        );
    }

    /// <summary>
    /// Purchase coins via IAP receipt validation
    /// </summary>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseResponse>), 400)]
    public async Task<IActionResult> PurchaseCoins([FromBody] PurchaseRequest request)
    {
        var cmd = new PurchaseCoinsCommand(request.Platform, request.ReceiptData, request.ProductId)
        {
            UserId = User.GetUserId()
        };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<PurchaseResponse>.Ok(data)),
            error => BadRequest(ApiResponse<PurchaseResponse>.Fail(error.Code, error.Message))
        );
    }
}
