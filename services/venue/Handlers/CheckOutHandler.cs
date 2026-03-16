using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class CheckOutHandler : IRequestHandler<CheckOutCommand, Result<CheckOutResultDto>>
{
    private readonly ICheckInRepository _checkIns;
    private readonly IRedisService _redis;
    private readonly ILogger<CheckOutHandler> _logger;

    public CheckOutHandler(ICheckInRepository checkIns, IRedisService redis, ILogger<CheckOutHandler> logger)
    {
        _checkIns = checkIns;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Result<CheckOutResultDto>> Handle(CheckOutCommand cmd, CancellationToken ct)
    {
        var checkIn = await _checkIns.GetByIdAsync(cmd.CheckInId, ct);
        if (checkIn is null)
            return Result<CheckOutResultDto>.Failure("NOT_FOUND", "Check-in tapılmadı");

        if (checkIn.UserId != cmd.UserId)
            return Result<CheckOutResultDto>.Failure("FORBIDDEN", "Bu check-in sizə aid deyil");

        if (!checkIn.IsActive)
            return Result<CheckOutResultDto>.Failure("ALREADY_CHECKED_OUT", "Artıq çıxış etmisiniz");

        checkIn.CheckOut(CheckOutReason.Manual);
        await _checkIns.UpdateAsync(checkIn, ct);

        // Decrement Redis counter
        await _redis.DecrementVenueCountAsync(checkIn.VenueId);
        await _redis.RemoveActiveUserAsync(checkIn.VenueId, cmd.UserId);
        await _redis.RemoveUserFromVenueAsync(cmd.UserId, checkIn.VenueId);

        var duration = checkIn.CheckOutAt!.Value - checkIn.CheckInAt;
        _logger.LogInformation("User {UserId} checked out from venue {VenueId}, duration: {Duration}",
            cmd.UserId, checkIn.VenueId, duration);

        return Result<CheckOutResultDto>.Success(new CheckOutResultDto(true, duration.ToString(@"hh\:mm\:ss")));
    }
}
