using FluentValidation;
using Loca.Services.Social.Commands;

namespace Loca.Services.Social.Validators;

public class RespondToMatchValidator : AbstractValidator<RespondToMatchCommand>
{
    public RespondToMatchValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty()
            .Must(a => a.ToLowerInvariant() is "accept" or "decline")
            .WithMessage("Action 'accept' və ya 'decline' olmalıdır");
    }
}
