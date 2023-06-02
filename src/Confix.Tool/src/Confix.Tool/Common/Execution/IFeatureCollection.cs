namespace Confix.Tool.Common.Pipelines;

public interface IFeatureCollection
{
    public void Set<TFeature>(TFeature instance);

    public TFeature Get<TFeature>();

    public bool TryGet<TFeature>(out TFeature instance);
}
