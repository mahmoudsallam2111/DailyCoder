using DailyCoder.Api.DTOs.Auth;
using DailyCoder.Api.Entities;

namespace DailyCoder.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto)
    {
        return new()
        {
            Id = User.CreateNewId(),
            Name = dto.Name,
            Email = dto.Email,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public static void UpdateFromDto(this User user, UpdateProfileDto dto)
    {
        user.Name = dto.Name;
        user.UpdatedAtUtc = DateTime.UtcNow;
    }
}
