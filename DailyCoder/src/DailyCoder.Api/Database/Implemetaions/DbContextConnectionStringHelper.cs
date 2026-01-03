using System.Reflection;
using DailyCoder.Api.Attributes;

namespace DailyCoder.Api.Database.Implemetaions;

public static class DbContextConnectionStringHelper
{
    public static string GetConnectionStringName(Type dbContextType)
    {
        var attribute = dbContextType
            .GetCustomAttribute<ConnectionStringNameAttribute>();

        return attribute?.Name ?? "Default";
    }
}
