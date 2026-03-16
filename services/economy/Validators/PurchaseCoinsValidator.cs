using FluentValidation;
using Loca.Services.Economy.Commands;

namespace Loca.Services.Economy.Validators;

public class PurchaseCoinsValidator : AbstractValidator<PurchaseCoinsCommand>
{
    public PurchaseCoinsValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Platform).NotEmpty()
            .Must(p => p.ToLowerInvariant() is "ios" or "android")
            .WithMessage("Platform 'ios' və ya 'android' olmalıdır");
        RuleFor(x => x.ReceiptData).NotEmpty().WithMessage("Qəbz məlumatı tələb olunur");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Məhsul ID tələb olunur");
    }
}
