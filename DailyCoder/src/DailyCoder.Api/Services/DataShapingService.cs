using System.Dynamic;
using DailyCoder.Api.DTOs.Common;

namespace DailyCoder.Api.Services;

public sealed class DataShapingService
{

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        var propertyInfoList = new List<System.Reflection.PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            propertyInfoList.AddRange(
                typeof(T).GetProperties(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance));
        }
        else
        {
            var fieldsAfterSplit = fields.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var field in fieldsAfterSplit)
            {
                var propertyInfo = typeof(T).GetProperty(
                    field,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new ArgumentException(
                        $"Property '{field}' not found on type '{typeof(T).Name}'");
                }

                propertyInfoList.Add(propertyInfo);
            }
        }

        var expandoObject = new ExpandoObject();
        var expandoDict = (IDictionary<string, object?>)expandoObject;

        foreach (var propertyInfo in propertyInfoList)
        {
            expandoDict[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }

        return expandoObject;
    }

    public List<ExpandoObject> ShapeCollectionData<T>(IEnumerable<T> data, string? fields , Func<T , List<LinkDto>>? linkDtosFactory = null)
    {
        // prepare the list of properties to return
        var expandoObjectList = new List<ExpandoObject>();
        var propertyInfoList = new List<System.Reflection.PropertyInfo>();
        if (string.IsNullOrWhiteSpace(fields))
        {
            var propertyInfos = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
            var fieldsAfterSplit = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var field in fieldsAfterSplit)
            {
                var propertyInfo = typeof(T).GetProperty(field, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (propertyInfo is null)
                {
                    throw new ArgumentException($"Property '{field}' not found on type '{typeof(T)}'");
                }
                propertyInfoList.Add(propertyInfo);
            }
        }

        // extract the properties values
        foreach (var item in data)
        {
            var expandoObject = new ExpandoObject();
            foreach (var propertyInfo in propertyInfoList)
            {
                var propertyValue = propertyInfo.GetValue(item);
                ((IDictionary<string, object?>)expandoObject).Add(propertyInfo.Name, propertyValue);
            }

            if (linkDtosFactory is not null)
            {
                ((IDictionary<string, object?>)expandoObject)["links"] = linkDtosFactory(item);
            }

            expandoObjectList.Add(expandoObject);
        }
        return expandoObjectList;
    }
}
