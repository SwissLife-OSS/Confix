using System;
using System.Collections.Generic;

namespace Confix.Tool.Common.Pipelines;

public sealed class FeatureCollection : IFeatureCollection
{
    private readonly Dictionary<Type, object> _features = new();

    public void Set<TFeature>(TFeature instance)
    {
        _features[typeof(TFeature)] = instance!;
    }

    public TFeature Get<TFeature>()
    {
        if (_features.TryGetValue(typeof(TFeature), out var value))
        {
            return (TFeature) value;
        }

        throw new KeyNotFoundException($"The feature of type '{typeof(TFeature)}' was not found.");
    }

    public bool TryGet<TFeature>(out TFeature instance)
    {
        if (_features.TryGetValue(typeof(TFeature), out var value))
        {
            instance = (TFeature) value;
            return true;
        }

        instance = default!;
        return false;
    }
}
