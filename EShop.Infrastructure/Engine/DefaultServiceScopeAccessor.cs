using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Infrastructure.Engine;

public interface IScopedProviderAccessor
{
    /// <summary>
    /// EShopEngine resolves dependencies using a service provider returned by this property. If <see cref="HttpContext"/> is available, we prefer resolving services using HttpContext's <see cref="IServiceProvider"/>.
    /// If it's not available you first should make a call to <see cref="CreateManualScopedProvider"/> to create a scope rather than directly using this property.
    /// After you've done this, you can call this property to retrieve the scope to resolve services with any lifetime and when you're done using the services, the scope and all the services resolved this way will be disposed automatically (though 'try/finally').
    /// This lets you resolve services through the engine so that either way all of them are eventually disposed and garbage-collected no matter if <see cref="HttpContext"/> is there.
    /// So that if you use the engine to resolve services where <see cref="HttpContext"/> isn't available, you still don't have memory leaks.
    /// Do not dispose the scope manually because it can also be used in other places you aren't aware of.
    /// </summary>
    IServiceProvider GetScopedProvider { get; }

    /// <summary>
    /// Used to create a scope unless <see cref="HttpContext"/> is present in which case HttpContext's <see cref="IServiceProvider"/>'s Scope is returned.
    /// Any call to <see cref="GetScopedProvider"/> after calling this method to resolve dependencies is safe and any dependencies resolved this way will be automatically disposed as opposed to resolving services against the root service container. 
    /// </summary>  
    /// <returns>Returns the <see cref="IDisposable"/> of a newly created <see cref="IServiceScope"/></returns>
    IDisposable CreateManualScopedProvider(out IServiceProvider scope);
}

public class DefaultScopedProviderAccessor : IScopedProviderAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IServiceScope _customScope;

    public DefaultScopedProviderAccessor(IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceScopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }


    public IServiceProvider GetScopedProvider
    {
        get
        {
            var provider = _customScope?.ServiceProvider;
            if (provider == null)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    return httpContext.RequestServices;
                }

                _customScope = CreateCustomScopeInternal();
                return _customScope.ServiceProvider;

            }
            return provider;
        }
    }


    public IDisposable CreateManualScopedProvider(out IServiceProvider provider)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            provider = httpContext.RequestServices;
            return ActionDisposable.Empty;
        }
        else
        {
            provider = _customScope?.ServiceProvider;
            if (provider == null)
            {
                _customScope = CreateCustomScopeInternal();
                provider = _customScope.ServiceProvider;
                return new ActionDisposable(() => _customScope.Dispose());
            }

            return ActionDisposable.Empty;
        }
    }

    private IServiceScope CreateCustomScopeInternal()
    {
        var customScope = _serviceScopeFactory.CreateScope();
        return new ServiceScopeDisposeWrapper(customScope, OnScopeDisposed);
    }

    private void OnScopeDisposed()
    {
        //For GC
        _customScope = null;
    }
}

public class ServiceScopeDisposeWrapper : IServiceScope
{
    private readonly IServiceScope _underlyingScope;
    private readonly Action _disposeAction;

    public ServiceScopeDisposeWrapper(IServiceScope underlyingScope, Action disposeAction = null)
    {
        _underlyingScope = underlyingScope;
        _disposeAction = disposeAction;
    }

    public IServiceProvider ServiceProvider => _underlyingScope.ServiceProvider;


    public void Dispose()
    {
        try
        {
            _disposeAction?.Invoke();
        }
        finally
        {
            _underlyingScope.Dispose();
        }
    }
}