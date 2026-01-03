using DailyCoder.Api.Database.Abstractions;

namespace DailyCoder.Api.Database.Implemetaions;

public class DefaultConnectionStringResolver : IConnectionStringResolver
{
    private readonly IConfiguration _configuration;

    public DefaultConnectionStringResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Resolve(string name)
    {
        var connectionString = _configuration.GetConnectionString(name);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' was not found.");
        }

        return connectionString;
    }
}
