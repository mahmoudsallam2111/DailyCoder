using Microsoft.AspNetCore.Mvc;
using static DailyCoder.Api.Enums.Enums;

namespace DailyCoder.Api.DTOs.Habits;

public sealed record HabitQueryParamters
{

    [FromQuery(Name ="q")]
    public string? Search { get; set; }
    public HabitType? Type { get; init; }
    public HabitStatus? Status { get; init; }
    public string? Sort { get; set; }
    public string? Fields { get; set; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    [FromHeader(Name = "Accept")]
    public string? Accept { get; set; }
}
