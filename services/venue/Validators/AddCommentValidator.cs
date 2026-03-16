using FluentValidation;
using Loca.Services.Venue.Commands;

namespace Loca.Services.Venue.Validators;

public class AddCommentValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(500)
            .WithMessage("Şərh 500 simvoldan çox ola bilməz");
    }
}
