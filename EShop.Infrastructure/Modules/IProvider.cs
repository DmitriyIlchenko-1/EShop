namespace EShop.Infrastructure.Modules;

public interface IProvider
{
    
}


/// <summary>
/// Holds metadata describing a provider. Differs from IModuleDescriptor in that, ModuleDescriptor hold information about a module with one or more provider in it.
/// </summary>
public class ProviderMetadata
{
    public string SystemName { get; set; }
    public string FriendlyName { get; set; }
    
    
    /// <returns>
    /// Returns null if the provider is part of the Core project and not shipped in a separate module.
    /// </returns>
    public IModuleDescriptor ModuleDescriptor { get; set; }
}


public class Provider<TProvider> where TProvider : IProvider
{
    public Provider(Lazy<TProvider, ProviderMetadata> lazy)
    {
        _lazy = lazy;
    }

    private readonly Lazy<TProvider, ProviderMetadata> _lazy;

    public TProvider Proviver => _lazy.Value;
    public ProviderMetadata Metadata => _lazy.Metadata;
}