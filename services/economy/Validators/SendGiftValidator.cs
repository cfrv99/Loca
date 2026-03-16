using FluentValidation;
using Loca.Services.Economy.Commands;

namespace Loca.Services.Economy.Validators;

public class SendGiftValidator : AbstractValidator<SendGiftCommand>
{
    public SendGiftValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.GiftId).NotEmpty();
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Context).NotEmpty()
            .Must(c => c is "public_chat" or "private_chat")
            .WithMessage("Context 'public_chat' və ya 'private_chat' olmalıdır");
        RuleFor(x => x.VenueId).NotEmpty().When(x => x.Context == "public_chat")
            .WithMessage("Ictimai chat üçün VenueId tələb olunur");
        RuleFor(x => x.ConversationId).NotEmpty().When(x => x.Context == "private_chat")
            .WithMessage("Şəxsi chat üçün ConversationId tələb olunur");
    }
}
