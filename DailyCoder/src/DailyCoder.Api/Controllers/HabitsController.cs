using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using DailyCoder.Api.Database;
using DailyCoder.Api.DTOs.Common;
using DailyCoder.Api.DTOs.Habits;
using DailyCoder.Api.Entities;
using DailyCoder.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DailyCoder.Api.Enums.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DailyCoder.Api.Controllers;

[ApiController]
[Route("habits")]
public class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginationResult<HabitDto>>> GetHabits(
        [FromQuery] HabitQueryParamters queryParamters,
        SortMappingProvider sortMappingProvider)
    {
        // sort support
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(queryParamters.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{queryParamters.Sort}'");
        }

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<Habit, HabitDto>();


        // search support
        queryParamters.Search ??= queryParamters.Search?.Trim().ToLower();

        IQueryable<HabitDto> habitsQuery = dbContext
             .Habits
             .Where(h => queryParamters.Search == null ||
                         h.Name.ToLower().Contains(queryParamters.Search) ||
                         h.Description != null && h.Description.ToLower().Contains(queryParamters.Search))
             .Where(h => queryParamters.Type == null || h.Type == queryParamters.Type)
             .Where(h => queryParamters.Status == null || h.Status == queryParamters.Status)
             .ApplySort(queryParamters.Sort, sortMappings)
             .Select(HabitQueries.ProjectToDto());

        PaginationResult<HabitDto> habits = await PaginationResult<HabitDto>.CreateAsync(habitsQuery , page: queryParamters.Page, pageSize: queryParamters.PageSize);

        return Ok(habits);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit(string id)
    {
        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto , IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        Habit habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();

        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
