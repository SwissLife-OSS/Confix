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
        return (TFeature) _features[typeof(TFeature)];
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
