using FluentValidation;
using Loca.Services.Venue.Commands;

namespace Loca.Services.Venue.Validators;

public class CheckInValidator : AbstractValidator<CheckInCommand>
{
    public CheckInValidator()
    {
        RuleFor(x => x.QrPayload).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
        RuleFor(x => x.DeviceFingerprint).NotEmpty().MaximumLength(255);
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class GetNearbyVenuesValidator : AbstractValidator<Queries.GetNearbyVenuesQuery>
{
    public GetNearbyVenuesValidator()
    {
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
