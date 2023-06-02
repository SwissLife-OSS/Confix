namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Represents a collection of features added and consumed by the pipeline. 
/// The feature collection is an mechanism for passing data and shared resources across different
/// stages of a processing pipeline. Each middleware can add or replace features in this
/// collection as well as consume existing features. This pattern decouples different middleware
/// components while promoting information sharing and reuse. A feature can represent any data or
/// service required by the middleware, such as configuration data, state information,
/// service references, and more.
/// </summary>
public interface IFeatureCollection
{
    /// <summary>
    /// Adds or replaces a feature in the collection. 
    /// </summary>
    /// <typeparam name="TFeature">The type of the feature.</typeparam>
    /// <param name="instance">The instance of the feature to add or replace.</param>
    public void Set<TFeature>(TFeature instance);

    /// <summary>
    /// Retrieves a feature from the collection. Throws an exception if a feature of the specified
    /// type is not present.
    /// </summary>
    /// <typeparam name="TFeature">The type of the feature to retrieve.</typeparam>
    /// <returns>An instance of the feature if present in the collection.</returns>
    public TFeature Get<TFeature>();

    /// <summary>
    /// Attempts to retrieve a feature from the collection. If the feature is not present, it
    /// doesn't throw an exception but returns false.
    /// </summary>
    /// <typeparam name="TFeature">The type of the feature to retrieve.</typeparam>
    /// <param name="instance">An out parameter to hold the retrieved feature instance if it exists.</param>
    /// <returns>True if the feature exists in the collection, false otherwise.</returns>
    public bool TryGet<TFeature>(out TFeature instance);
}
