using FluentValidation;
using Loca.Services.Identity.Commands;

namespace Loca.Services.Identity.Validators;

public class GoogleLoginValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().WithMessage("Google ID token is required");
    }
}
