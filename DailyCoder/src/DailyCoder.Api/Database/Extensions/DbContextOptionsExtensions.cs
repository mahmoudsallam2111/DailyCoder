using DailyCoder.Api.Database.Abstractions;
using DailyCoder.Api.Database.Implemetaions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace DailyCoder.Api.Database.Extensions;

public static class DbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseNpgsqlLoader(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider,
        Type dbContextType,
        string? schema)
    {
        var resolver = serviceProvider
            .GetRequiredService<IConnectionStringResolver>();

        var connectionStringName =
            DbContextConnectionStringHelper.GetConnectionStringName(dbContextType);

        var connectionString = resolver.Resolve(connectionStringName);

        return optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
                                  npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema))
                                 .UseSnakeCaseNamingConvention();
    }
}

