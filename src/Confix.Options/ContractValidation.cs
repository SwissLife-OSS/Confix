using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confix;

public static class ContractValidation
{
    public static IReadOnlyList<string> Validate(IServiceProvider services, IConfiguration configuration, bool strict = true)
    {
        var contracts = services.GetServices<IConfixContract>().ToArray();
        var errors = new List<string>();
        if (contracts.Length == 0) return ["No active Confix contracts were registered."];
        for (var i = 0; i < contracts.Length; i++)
            for (var j = i + 1; j < contracts.Length; j++)
                if (contracts[i].Section.StartsWith(contracts[j].Section + ":", StringComparison.OrdinalIgnoreCase) ||
                    contracts[j].Section.StartsWith(contracts[i].Section + ":", StringComparison.OrdinalIgnoreCase))
                    errors.Add($"{contracts[i].Section} and {contracts[j].Section}: overlapping Confix sections are not supported.");
        if (strict && !contracts.Any(c => c.Section.Length == 0)) CheckCoverage(configuration, "", contracts, errors);
        foreach (var contract in contracts)
        {
            if (!contract.Required && !ConfixOptionsExtensions.HasSection(configuration, contract.Section)) continue;
            try { contract.Validate(services); }
            catch (ConfixValidationException ex) { errors.AddRange(ex.Failures); }
            catch (Microsoft.Extensions.Options.OptionsValidationException)
            {
                // Only Confix's own errors are exposed by the object validator; user-supplied
                // IValidateOptions messages may contain values, so the runner uses safe paths.
                errors.Add($"{contract.Section}: options validation failed.");
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            { errors.Add($"{contract.Section}: validation could not complete."); }
        }
        return errors;
    }

    internal static void CheckCoverage(IConfiguration configuration, string path, IConfixContract[] contracts, List<string> errors)
    {
        foreach (var child in configuration.GetChildren())
        {
            var current = path.Length == 0 ? child.Key : path + ":" + child.Key;
            if (contracts.Any(c => c.Section.Equals(current, StringComparison.OrdinalIgnoreCase))) continue;
            if (contracts.Any(c => c.Section.StartsWith(current + ":", StringComparison.OrdinalIgnoreCase)))
            {
                if (child.Value is not null) errors.Add($"{current}: expected a configuration section container.");
                else CheckCoverage(child, current, contracts, errors);
            }
            else errors.Add($"{current}: no active Confix contract owns this section.");
        }
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, PropertyInfo[]> _properties = new();

    internal static PropertyInfo[] Properties(Type type) => _properties.GetOrAdd(type,
        static t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && p.GetMethod is not null).ToArray());
    internal static string Key(PropertyInfo property) => property.GetCustomAttribute<ConfigurationKeyNameAttribute>()?.Name ?? property.Name;
    internal static bool Scalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) || type == typeof(Guid) || type == typeof(Uri) || type == typeof(byte[]);
    }

    internal static void CheckKeys(IConfiguration section, Type type, string path, List<string> errors)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (Scalar(type))
        {
            if (section.GetChildren().Any()) errors.Add($"{path}: expected a scalar configuration value.");
            return;
        }
        if (section is IConfigurationSection scalar && scalar.Value is not null)
        {
            errors.Add($"{path}: expected structured configuration.");
            return;
        }
        var dictionary = type.GetInterfaces().Append(type).FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        var enumerable = type.GetInterfaces().Append(type).FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (dictionary is not null || enumerable is not null)
        {
            var itemType = dictionary?.GetGenericArguments()[1] ?? enumerable!.GetGenericArguments()[0];
            foreach (var child in section.GetChildren()) CheckKeys(child, itemType, path + ":" + child.Key, errors);
            return;
        }
        var properties = Properties(type);
        foreach (var property in properties.Where(p => p.IsDefined(typeof(ConfixRequiredKeyAttribute))))
            if (!section.GetChildren().Any(c => c.Key.Equals(Key(property), StringComparison.OrdinalIgnoreCase)))
                errors.Add($"{path}:{Key(property)}: required key is missing.");
        foreach (var child in section.GetChildren())
        {
            var property = properties.FirstOrDefault(p => Key(p).Equals(child.Key, StringComparison.OrdinalIgnoreCase));
            if (property is null) errors.Add($"{path}:{child.Key}: unknown configuration key.");
            else CheckKeys(child, property.PropertyType, path + ":" + child.Key, errors);
        }
    }

    internal static void ValidateObject(object value, string path, IServiceProvider services, List<string> errors)
        => Walk(value, path, services, errors, new HashSet<object>(ReferenceEqualityComparer.Instance));

    private static void Walk(object? value, string path, IServiceProvider services, List<string> errors, HashSet<object> ancestors)
    {
        if (value is null || Scalar(value.GetType()) || !ancestors.Add(value)) return;
        try
        {
            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary) Walk(entry.Value, path + ":" + entry.Key, services, errors, ancestors);
                return;
            }
            if (value is IEnumerable enumerable)
            {
                var index = 0;
                foreach (var item in enumerable) Walk(item, path + ":" + index++, services, errors, ancestors);
                return;
            }
            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, services, null);
            var properties = Properties(value.GetType());
            try
            {
                Validator.TryValidateObject(value, context, results, validateAllProperties: true);
                foreach (var result in results)
                {
                    var members = result.MemberNames.Where(m => properties.Any(p => p.Name == m)).ToArray();
                    foreach (var member in members.Length == 0 ? [""] : members)
                        errors.Add($"{path}{(member.Length == 0 ? "" : ":" + member)}: declared validation rule failed.");
                }
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            { errors.Add($"{path}: declared validator could not complete."); }
            foreach (var property in properties)
            {
                var child = property.GetValue(value);
                if (property.IsDefined(typeof(ConfixRequiredItemsAttribute)) && child is IEnumerable items)
                {
                    var index = 0;
                    foreach (var item in child is IDictionary dictionaryItems ? dictionaryItems.Values : items)
                    {
                        if (item is null) errors.Add($"{path}:{Key(property)}:{index}: null items are not allowed.");
                        index++;
                    }
                }
                Walk(child, path + ":" + Key(property), services, errors, ancestors);
            }
        }
        finally { ancestors.Remove(value); }
    }
}
