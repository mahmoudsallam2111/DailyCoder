namespace DailyCoder.Api.Database.Abstractions;

public interface IConnectionStringResolver
{
    string Resolve(string name);
}
