using FluentValidation;
using Loca.Services.Venue.Commands;

namespace Loca.Services.Venue.Validators;

public class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.VenueId).NotEmpty();
        RuleFor(x => x.Content).MaximumLength(2000).When(x => x.Content is not null);
        RuleForEach(x => x.MediaUrls).NotEmpty().MaximumLength(500).When(x => x.MediaUrls is not null);
        RuleFor(x => x.MediaUrls).Must(m => m == null || m.Count <= 3)
            .WithMessage("Ən çox 3 media fayl əlavə edə bilərsiniz");
    }
}
