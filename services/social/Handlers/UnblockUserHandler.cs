using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Social.Handlers;

public class UnblockUserHandler : IRequestHandler<UnblockUserCommand, Result<BlockResponse>>
{
    private readonly IMatchRepository _matches;
    private readonly ILogger<UnblockUserHandler> _logger;

    public UnblockUserHandler(IMatchRepository matches, ILogger<UnblockUserHandler> logger)
    {
        _matches = matches;
        _logger = logger;
    }

    public async Task<Result<BlockResponse>> Handle(UnblockUserCommand cmd, CancellationToken ct)
    {
        var block = await _matches.GetBlockAsync(cmd.BlockerId, cmd.BlockedId, ct);
        if (block is null)
            return Result<BlockResponse>.Success(new BlockResponse(false));

        await _matches.RemoveBlockAsync(block, ct);
        _logger.LogInformation("User {BlockerId} unblocked user {BlockedId}", cmd.BlockerId, cmd.BlockedId);

        return Result<BlockResponse>.Success(new BlockResponse(false));
    }
}
