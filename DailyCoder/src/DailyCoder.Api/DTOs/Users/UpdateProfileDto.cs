using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DailyCoder.Api.DTOs.Users;

[ValidateNever]
public sealed record UpdateProfileDto
{
    public required string Name { get; set; }
}
