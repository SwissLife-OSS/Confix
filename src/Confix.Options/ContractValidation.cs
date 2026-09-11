using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix;

public static class ContractValidation
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _properties = new();

    private static readonly Type[] _scalarTypes =
    [
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(Uri),
        typeof(byte[])
    ];

    public static IReadOnlyList<string> Validate(
        IServiceProvider services,
        IConfiguration configuration,
        bool strict = true)
    {
        var contracts = services.GetServices<IConfixContract>().ToArray();

        if (contracts.Length == 0)
        {
            return ["No active Confix contracts were registered."];
        }

        var errors = new List<string>();

        CheckContractConflicts(contracts, errors);

        var hasRootContract = contracts.Any(contract => contract.Section.Length == 0);

        if (strict && !hasRootContract)
        {
            CheckCoverage(configuration, string.Empty, contracts, errors);
        }

        foreach (var contract in contracts)
        {
            ValidateContract(contract, services, configuration, errors);
        }

        return errors;
    }

    private static void CheckContractConflicts(IConfixContract[] contracts, List<string> errors)
    {
        for (var i = 0; i < contracts.Length; i++)
        {
            for (var j = i + 1; j < contracts.Length; j++)
            {
                var first = contracts[i];
                var second = contracts[j];

                if (first.Section.Equals(second.Section, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"{Display(first.Section)}: multiple contracts " +
                        $"({first.OptionsType.Name}, {second.OptionsType.Name}) claim this section.");

                    continue;
                }

                var (parent, child) = IsNestedIn(first.Section, second.Section)
                    ? (second, first)
                    : IsNestedIn(second.Section, first.Section)
                        ? (first, second)
                        : (null, null);

                if (parent is not null &&
                    NestingConflict(parent.Section, parent.OptionsType, child!.Section, child.OptionsType) is { } conflict)
                {
                    errors.Add(conflict);
                }
            }
        }
    }

    /// <summary>
    /// A contract may live inside another contract's section only when the parent type cannot
    /// bind the delegated key, so the same data can never have two owners.
    /// </summary>
    internal static string? NestingConflict(
        string parentSection,
        Type parentType,
        string childSection,
        Type childType)
    {
        var relative = parentSection.Length == 0
            ? childSection
            : childSection[(parentSection.Length + 1)..];
        var key = relative.Split(':')[0];

        return ConsumesKey(parentType, key)
            ? $"Section '{Display(childSection)}' of {childType.Name} conflicts with " +
                $"'{Display(parentSection)}' of {parentType.Name}: '{key}' is bound by {parentType.Name}."
            : null;
    }

    internal static bool ConsumesKey(Type type, string key)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        // Scalars, dictionaries and collections bind every child key.
        if (Scalar(type) || ItemType(type) is not null)
        {
            return true;
        }

        return Properties(type).Any(p => Key(p).Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    internal static string Display(string section)
    {
        return section.Length == 0 ? "(root)" : section;
    }

    private static void ValidateContract(
        IConfixContract contract,
        IServiceProvider services,
        IConfiguration configuration,
        List<string> errors)
    {
        if (!contract.Required &&
            !ConfixOptionsExtensions.HasSection(configuration, contract.Section))
        {
            return;
        }

        try
        {
            contract.Validate(services);
        }
        catch (ConfixValidationException ex)
        {
            errors.AddRange(ex.Failures);
        }
        catch (OptionsValidationException)
        {
            // Only Confix's own errors are exposed by the object validator; user-supplied
            // IValidateOptions messages may contain values, so the runner uses safe paths.
            errors.Add($"{contract.Section}: options validation failed.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            errors.Add($"{contract.Section}: validation could not complete.");
        }
    }

    internal static void CheckCoverage(
        IConfiguration configuration,
        string path,
        IConfixContract[] contracts,
        List<string> errors)
    {
        foreach (var child in configuration.GetChildren())
        {
            var current = path.Length == 0 ? child.Key : path + ":" + child.Key;

            if (contracts.Any(c => c.Section.Equals(current, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (!contracts.Any(c => IsNestedIn(c.Section, current)))
            {
                errors.Add($"{current}: no active Confix contract owns this section.");
            }
            else if (child.Value is not null)
            {
                errors.Add($"{current}: expected a configuration section container.");
            }
            else
            {
                CheckCoverage(child, current, contracts, errors);
            }
        }
    }

    internal static void CheckKeys(
        IConfiguration section,
        Type type,
        string path,
        List<string> errors)
    {
        CheckKeys(section, type, path, errors, []);
    }

    internal static void CheckKeys(
        IConfiguration section,
        Type type,
        string path,
        List<string> errors,
        IReadOnlyCollection<string> delegated)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (Scalar(type))
        {
            if (section.GetChildren().Any())
            {
                errors.Add($"{path}: expected a scalar configuration value.");
            }

            return;
        }

        var itemType = ItemType(type);

        // An empty JSON array arrives as an empty value rather than as a childless container.
        if (section is IConfigurationSection { Value: { } value } &&
            !(value.Length == 0 && itemType is not null))
        {
            errors.Add($"{path}: expected structured configuration.");

            return;
        }

        if (itemType is not null)
        {
            foreach (var child in section.GetChildren())
            {
                CheckKeys(child, itemType, path + ":" + child.Key, errors);
            }

            return;
        }

        var properties = Properties(type);

        var required = properties.Where(p => p.IsDefined(typeof(ConfixRequiredKeyAttribute)));

        foreach (var property in required)
        {
            var key = Key(property);

            if (!section.GetChildren().Any(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"{path}:{key}: required key is missing.");
            }
        }

        foreach (var child in section.GetChildren())
        {
            if (IsDelegated(child.Key, delegated))
            {
                continue;
            }

            var property = properties
                .FirstOrDefault(p => Key(p).Equals(child.Key, StringComparison.OrdinalIgnoreCase));

            if (property is not null)
            {
                // Registration guarantees delegation never passes through a bound key.
                CheckKeys(child, property.PropertyType, path + ":" + child.Key, errors);
            }
            else if (Nested(child.Key, delegated) is { Count: > 0 } nested)
            {
                CheckDelegatedContainer(child, nested, path + ":" + child.Key, errors);
            }
            else
            {
                errors.Add($"{path}:{child.Key}: unknown configuration key.");
            }
        }
    }

    /// <summary>Walks an unbound structural key that only exists to host nested contracts.</summary>
    private static void CheckDelegatedContainer(
        IConfigurationSection section,
        IReadOnlyCollection<string> delegated,
        string path,
        List<string> errors)
    {
        if (section.Value is { Length: > 0 })
        {
            errors.Add($"{path}: expected a configuration section container.");

            return;
        }

        foreach (var child in section.GetChildren())
        {
            if (IsDelegated(child.Key, delegated))
            {
                continue;
            }

            if (Nested(child.Key, delegated) is { Count: > 0 } nested)
            {
                CheckDelegatedContainer(child, nested, path + ":" + child.Key, errors);
            }
            else
            {
                errors.Add($"{path}:{child.Key}: unknown configuration key.");
            }
        }
    }

    private static bool IsDelegated(string key, IReadOnlyCollection<string> delegated)
    {
        return delegated.Any(d => d.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyCollection<string> Nested(
        string key,
        IReadOnlyCollection<string> delegated)
    {
        return delegated
            .Where(d => d.StartsWith(key + ":", StringComparison.OrdinalIgnoreCase))
            .Select(d => d[(key.Length + 1)..])
            .ToArray();
    }

    internal static void ValidateObject(
        object value,
        string path,
        IServiceProvider services,
        List<string> errors)
    {
        var ancestors = new HashSet<object>(ReferenceEqualityComparer.Instance);

        Walk(value, path, services, errors, ancestors);
    }

    private static void Walk(
        object? value,
        string path,
        IServiceProvider services,
        List<string> errors,
        HashSet<object> ancestors)
    {
        if (value is null || Scalar(value.GetType()) || !ancestors.Add(value))
        {
            return;
        }

        try
        {
            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    Walk(entry.Value, path + ":" + entry.Key, services, errors, ancestors);
                }

                return;
            }

            if (value is IEnumerable enumerable)
            {
                var index = 0;

                foreach (var item in enumerable)
                {
                    Walk(item, path + ":" + index++, services, errors, ancestors);
                }

                return;
            }

            var properties = Properties(value.GetType());

            CheckDeclaredRules(value, path, services, properties, errors);

            foreach (var property in properties)
            {
                var child = property.GetValue(value);

                if (property.IsDefined(typeof(ConfixRequiredItemsAttribute)))
                {
                    CheckRequiredItems(child, path + ":" + Key(property), errors);
                }

                Walk(child, path + ":" + Key(property), services, errors, ancestors);
            }
        }
        finally
        {
            ancestors.Remove(value);
        }
    }

    private static void CheckDeclaredRules(
        object value,
        string path,
        IServiceProvider services,
        PropertyInfo[] properties,
        List<string> errors)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(value, services, null);

        try
        {
            Validator.TryValidateObject(value, context, results, validateAllProperties: true);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            errors.Add($"{path}: declared validator could not complete.");

            return;
        }

        foreach (var result in results)
        {
            // Custom messages may echo configured values, so only the member path is reported.
            var members = result.MemberNames
                .Where(member => properties.Any(p => p.Name == member))
                .ToArray();

            foreach (var member in members.Length == 0 ? [string.Empty] : members)
            {
                var suffix = member.Length == 0 ? string.Empty : ":" + member;
                errors.Add($"{path}{suffix}: declared validation rule failed.");
            }
        }
    }

    private static void CheckRequiredItems(object? value, string path, List<string> errors)
    {
        if (value is not IEnumerable items)
        {
            return;
        }

        var index = 0;

        foreach (var item in value is IDictionary dictionary ? dictionary.Values : items)
        {
            if (item is null)
            {
                errors.Add($"{path}:{index}: null items are not allowed.");
            }

            index++;
        }
    }

    internal static PropertyInfo[] Properties(Type type)
    {
        return _properties.GetOrAdd(type, static t => t
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && p.GetMethod is not null)
            .ToArray());
    }

    internal static string Key(PropertyInfo property)
    {
        return property.GetCustomAttribute<ConfigurationKeyNameAttribute>()?.Name ?? property.Name;
    }

    internal static bool Scalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive || type.IsEnum || _scalarTypes.Contains(type);
    }

    private static Type? ItemType(Type type)
    {
        var candidates = type.GetInterfaces().Append(type).Where(t => t.IsGenericType).ToArray();

        var dictionary = candidates
            .FirstOrDefault(t => t.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        if (dictionary is not null)
        {
            return dictionary.GetGenericArguments()[1];
        }

        return candidates
            .FirstOrDefault(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ?.GetGenericArguments()[0];
    }

    private static bool IsNestedIn(string section, string parent)
    {
        return parent.Length == 0
            ? section.Length > 0
            : section.StartsWith(parent + ":", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Relative paths of contracts mounted inside the given contract's section.</summary>
    internal static IReadOnlyCollection<string> DelegatedPaths(
        IEnumerable<IConfixContract> contracts,
        IConfixContract parent)
    {
        return contracts
            .Where(c => !ReferenceEquals(c, parent) && IsNestedIn(c.Section, parent.Section))
            .Select(c => parent.Section.Length == 0
                ? c.Section
                : c.Section[(parent.Section.Length + 1)..])
            .ToArray();
    }
}
