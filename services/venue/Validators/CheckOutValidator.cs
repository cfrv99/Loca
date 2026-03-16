using FluentValidation;
using Loca.Services.Venue.Commands;

namespace Loca.Services.Venue.Validators;

public class CheckOutValidator : AbstractValidator<CheckOutCommand>
{
    public CheckOutValidator()
    {
        RuleFor(x => x.CheckInId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
