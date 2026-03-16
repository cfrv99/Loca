using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Social.Handlers;

public class BlockUserHandler : IRequestHandler<BlockUserCommand, Result<BlockResponse>>
{
    private readonly IMatchRepository _matches;
    private readonly ILogger<BlockUserHandler> _logger;

    public BlockUserHandler(IMatchRepository matches, ILogger<BlockUserHandler> logger)
    {
        _matches = matches;
        _logger = logger;
    }

    public async Task<Result<BlockResponse>> Handle(BlockUserCommand cmd, CancellationToken ct)
    {
        if (cmd.BlockerId == cmd.BlockedId)
            return Result<BlockResponse>.Failure("INVALID_REQUEST", "Özünüzü bloklaya bilməzsiniz");

        var existing = await _matches.GetBlockAsync(cmd.BlockerId, cmd.BlockedId, ct);
        if (existing is not null)
            return Result<BlockResponse>.Success(new BlockResponse(true));

        await _matches.AddBlockAsync(new Block
        {
            BlockerId = cmd.BlockerId,
            BlockedId = cmd.BlockedId
        }, ct);

        _logger.LogInformation("User {BlockerId} blocked user {BlockedId}", cmd.BlockerId, cmd.BlockedId);

        return Result<BlockResponse>.Success(new BlockResponse(true));
    }
}
