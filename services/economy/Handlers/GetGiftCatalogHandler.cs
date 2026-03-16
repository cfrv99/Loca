using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Economy.Queries;
using MediatR;

namespace Loca.Services.Economy.Handlers;

public class GetGiftCatalogHandler : IRequestHandler<GetGiftCatalogQuery, Result<List<GiftDto>>>
{
    private readonly IEconomyRepository _economy;

    public GetGiftCatalogHandler(IEconomyRepository economy) => _economy = economy;

    public async Task<Result<List<GiftDto>>> Handle(GetGiftCatalogQuery query, CancellationToken ct)
    {
        var items = await _economy.GetGiftCatalogAsync(ct);

        var dtos = items.Select(g => new GiftDto(
            Id: g.Id,
            Name: g.Name,
            NameAz: g.NameAz,
            Tier: g.Tier.ToString(),
            CoinPrice: g.CoinPrice,
            IconUrl: g.IconUrl,
            AnimationUrl: g.AnimationUrl,
            VenueId: g.VenueId
        )).ToList();

        return Result<List<GiftDto>>.Success(dtos);
    }
}
