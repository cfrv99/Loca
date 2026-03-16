using FluentValidation;
using Loca.Services.Identity.Commands;

namespace Loca.Services.Identity.Validators;

public class AppleLoginValidator : AbstractValidator<AppleLoginCommand>
{
    public AppleLoginValidator()
    {
        RuleFor(x => x.IdentityToken).NotEmpty().WithMessage("Apple identity token tələb olunur");
        RuleFor(x => x.AuthorizationCode).NotEmpty().WithMessage("Authorization code tələb olunur");
        RuleFor(x => x.FullName).MaximumLength(100).When(x => x.FullName is not null);
    }
}
