using FluentValidation;
using Loca.Services.Identity.Commands;

namespace Loca.Services.Identity.Validators;

public class GoogleLoginValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().MaximumLength(5000);
    }
}

public class AppleLoginValidator : AbstractValidator<AppleLoginCommand>
{
    public AppleLoginValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().MaximumLength(5000);
    }
}

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(500);
    }
}

public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DisplayName).MaximumLength(100).When(x => x.DisplayName != null);
        RuleFor(x => x.FirstName).MaximumLength(100).When(x => x.FirstName != null);
        RuleFor(x => x.LastName).MaximumLength(100).When(x => x.LastName != null);
        RuleFor(x => x.Bio).MaximumLength(500).When(x => x.Bio != null);
    }
}
