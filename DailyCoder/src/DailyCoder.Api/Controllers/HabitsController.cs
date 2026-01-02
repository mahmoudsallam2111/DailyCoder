using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Reflection.Metadata.Ecma335;
using Asp.Versioning;
using DailyCoder.Api.Database;
using DailyCoder.Api.DTOs.Common;
using DailyCoder.Api.DTOs.Habits;
using DailyCoder.Api.Entities;
using DailyCoder.Api.Services;
using DailyCoder.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DailyCoder.Api.Controllers;

[ApiController]
[Route("habits")]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
public class HabitsController(ApplicationDbContext dbContext, LinkService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetHabits(
        [FromQuery] HabitQueryParamters queryParamters,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService)
    {
        // sort support
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(queryParamters.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{queryParamters.Sort}'");
        }

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();


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

        int totalCount = await habitsQuery.CountAsync();

        List<HabitDto> habits = await habitsQuery
            .Skip((queryParamters.Page - 1) * queryParamters.PageSize)
            .Take(queryParamters.PageSize)
            .ToListAsync();

        bool includeLinks = queryParamters.Accept == CustomMediaTypeNames.Application.HateoasJson;

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(habits, 
            queryParamters.Fields , 
            h => CreateHyperLinksForHabit(h.Id , queryParamters.Fields)),

            Page = queryParamters.Page,
            PageSize = queryParamters.PageSize,
            TotalCount = totalCount
        };

        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(
                queryParamters,
                paginationResult.HasNextPage,
                paginationResult.HasPreviousPage);
        }


        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetHabit(string id,
        string? fields,
        DataShapingService dataShapingService,
        [FromHeader(Name ="Accept")]
        string? accept)
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

        var shapedHabit = dataShapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJson)
        {
            var linkDtos = CreateHyperLinksForHabit(id, fields);

            shapedHabit.TryAdd("links", linkDtos);
        }


        return Ok(shapedHabit);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetHabitV2(string id,
        string? fields,
        DataShapingService dataShapingService,
        [FromHeader(Name ="Accept")]
        string? accept)
    {
        HabitWithTagsDtoV2? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTagsV2())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        var shapedHabit = dataShapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJson)
        {
            var linkDtos = CreateHyperLinksForHabit(id, fields);

            shapedHabit.TryAdd("links", linkDtos);
        }


        return Ok(shapedHabit);
    }


    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        Habit habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();

        habitDto.Links = CreateHyperLinksForHabit(habit.Id, null);


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


    private List<LinkDto> CreateLinksForHabits(
        HabitQueryParamters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        return links;
    }

    private List<LinkDto> CreateHyperLinksForHabit(string id, string? fields)
    {
        return [

             linkService.Create(
                nameof(GetHabits),
                rel: "self",
                method: HttpMethods.Get,
                new { id , fields}),

            linkService.Create(
                nameof(UpdateHabit),
                rel: "update",
                method: HttpMethods.Put,
                new { id}),

            linkService.Create(
                nameof(PatchHabit),
                rel: "patch",
                method: HttpMethods.Patch,
                new { id}),


           linkService.Create(
                nameof(DeleteHabit),
                rel: "delete",
                method: HttpMethods.Delete,
                new { id}),

           linkService.Create(
                nameof(HabitTagsController.UpsertHabitTags),
                "upsert-tags",
                HttpMethods.Put,
                new { habitId = id },
                "HabitTags")

         ];
    }
}
