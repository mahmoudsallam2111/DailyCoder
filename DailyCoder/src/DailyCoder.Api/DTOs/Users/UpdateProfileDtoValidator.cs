using FluentValidation;

namespace DailyCoder.Api.DTOs.Users;

public sealed class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name must not be empty")
            .MaximumLength(100)
            .WithMessage("Name can not exceed 100 characters");
    }
}
