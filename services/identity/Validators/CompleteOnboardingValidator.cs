using FluentValidation;
using Loca.Services.Identity.Commands;

namespace Loca.Services.Identity.Validators;

public class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Interests).NotEmpty().WithMessage("Ən azı bir maraq sahəsi seçin");
        RuleForEach(x => x.Interests).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Purposes).NotEmpty().WithMessage("Ən azı bir məqsəd seçin");
        RuleForEach(x => x.Purposes).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VibePreferences).NotEmpty().WithMessage("Vibe preferences tələb olunur");
        RuleForEach(x => x.VibePreferences).ChildRules(vibe =>
        {
            vibe.RuleFor(v => v.Vibe).NotEmpty().MaximumLength(50);
            vibe.RuleFor(v => v.Weight).InclusiveBetween(0, 1);
        });
        RuleFor(x => x.PrivacySettings).NotNull();
    }
}
