using FluentValidation;
using Loca.Services.Social.Commands;

namespace Loca.Services.Social.Validators;

public class ReportUserValidator : AbstractValidator<ReportUserCommand>
{
    public ReportUserValidator()
    {
        RuleFor(x => x.ReporterId).NotEmpty();
        RuleFor(x => x.ReportedId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty()
            .Must(r => r.ToLowerInvariant() is "harassment" or "spam" or "fakeprofile" or "inappropriatecontent" or "other")
            .WithMessage("Yanlış şikayət səbəbi");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
