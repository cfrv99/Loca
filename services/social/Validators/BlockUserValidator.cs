using FluentValidation;
using Loca.Services.Social.Commands;

namespace Loca.Services.Social.Validators;

public class BlockUserValidator : AbstractValidator<BlockUserCommand>
{
    public BlockUserValidator()
    {
        RuleFor(x => x.BlockerId).NotEmpty();
        RuleFor(x => x.BlockedId).NotEmpty();
    }
}
