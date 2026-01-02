using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DailyCoder.Api.DTOs.Auth;


[ValidateNever]
public sealed record RegisterUserDto
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string ConfirmationPassword { get; init; }
}
