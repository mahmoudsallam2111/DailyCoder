namespace DailyCoder.Api.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ConnectionStringNameAttribute : Attribute
{
    public string Name { get; }

    public ConnectionStringNameAttribute(string name)
    {
        Name = name;
    }
}
