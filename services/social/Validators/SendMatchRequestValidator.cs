using FluentValidation;
using Loca.Services.Social.Commands;

namespace Loca.Services.Social.Validators;

public class SendMatchRequestValidator : AbstractValidator<SendMatchRequestCommand>
{
    public SendMatchRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ReceiverId).NotEmpty();
        RuleFor(x => x.IntroMessage).MaximumLength(200).When(x => x.IntroMessage is not null);
    }
}
