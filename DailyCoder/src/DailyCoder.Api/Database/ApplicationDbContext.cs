using DailyCoder.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyCoder.Api.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<HabitTag> HabitTags => Set<HabitTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(schema: Schemas.DailyCoder);

        modelBuilder.ApplyConfigurationsFromAssembly(assembly: typeof(ApplicationDbContext).Assembly);
    }
}
