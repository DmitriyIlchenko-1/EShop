using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extensions.DependencyInjection;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Infrastructure.Engine;

public interface IChildLifetimeScopeAccessor
{
    /// <summary>
    /// EShopEngine resolves dependencies using the <see cref="ILifetimeScope"/> returned by this property. If <see cref="HttpContext"/> is available, we prefer resolving services using HttpContext's <see cref="ILifetimeScope"/> (wrapper in <see cref="ServiceProvider"/>.
    /// If it's not available you first should make a call to <see cref="CreateManualChildLifetimeScope"/> to create a <see cref="ILifetimeScope"/> rather than directly using this property.
    /// After you've done this, you can call this property at any time (inside the scope created by calling <see cref="CreateManualChildLifetimeScope"/> to retrieve the <see cref="ILifetimeScope"/> to resolve services against with any lifetime and when you're done using the services, the <see cref="ILifetimeScope"/> and all the services resolved this way will be disposed automatically (though 'try/finally').
    /// This lets you resolve services through the engine so that either way all of them are eventually disposed and garbage-collected no matter if <see cref="HttpContext"/> is there.
    /// So that if you use the engine to resolve services where <see cref="HttpContext"/> isn't available, like during the startup, you still don't have memory leaks.
    ///  Don't call Dispose() on the returned <see cref="ILifetimeScope"/> because it might be used somewhere else inside the 'using' scope created by <see cref="CreateManualChildLifetimeScope"/>.
    /// </summary>
    ILifetimeScope GetChildLifetimeScope { get; }

    /// <summary>
    /// Used to create a <see cref="ILifetimeScope"/> unless <see cref="HttpContext"/> is present in which case HttpContext's <see cref="ILifetimeScope"/> is returned.
    /// Any call to <see cref="GetChildLifetimeScope"/> after calling this method to resolve dependencies is safe and any dependencies resolved this way will be automatically disposed as opposed to resolving services against the root <see cref="ILifetimeScope"/>, which will keep referencing them without disposing them and letting GC collect them until the app shutdown. 
    /// </summary>  
    /// <returns>Returns the <see cref="IDisposable"/> of a newly created <see cref="ILifetimeScope"/>  You don't get to dispose if the scope is managed by the framework (if it's retrieved from <see cref="HttpContext"/>) because it can also be used in other places you aren't aware of OR if the <see cref="ILifetimeScope"/> has already been created somewhere in the outer scope.</returns>
    IDisposable CreateManualChildLifetimeScope(out ILifetimeScope scope);
}

public class DefaultChildLifetimeScopeAccessor : IChildLifetimeScopeAccessor
{
    private static readonly object ChildLifetimeScopeTag = "Custom";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ILifetimeScope _customScope;
    private readonly ILifetimeScope _rootScope;

    public DefaultChildLifetimeScopeAccessor(IServiceProvider rootServiceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _rootScope = rootServiceProvider.GetAutofacRoot();
        _httpContextAccessor = httpContextAccessor;
    }


    public ILifetimeScope GetChildLifetimeScope
    {
        get
        {
            if (_customScope == null)
            {
                var scope = _httpContextAccessor.HttpContext?.RequestServices.GetAutofacRoot();
                if (scope != null)
                    return scope;

                return _customScope = CreateCustomLifetimeScope();
            }

            return _customScope;
        }
    }


    public IDisposable CreateManualChildLifetimeScope(out ILifetimeScope scope)
    {
        // We should prefer ILifetimeScope from HttpContext to resolve services more consistently (from the same scope if possible).
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            scope = httpContext.RequestServices.GetAutofacRoot();
            return ActionDisposable.Empty;
        }
        else
        {
            scope = _customScope;
            if (scope == null)
            {
                _customScope = CreateCustomLifetimeScope();
                var scopeRef = _customScope;
                return new ActionDisposable(() => scopeRef.Dispose());
            }

            return ActionDisposable.Empty;
        }
    }

    private ILifetimeScope CreateCustomLifetimeScope()
    {
        var childLifetimeScope = _rootScope.BeginLifetimeScope(ChildLifetimeScopeTag);
        childLifetimeScope.CurrentScopeEnding += OnScopeDisposed;
        return childLifetimeScope;
    }

    private void OnScopeDisposed(object sender, LifetimeScopeEndingEventArgs e)
    {
        //For GC
        _customScope = null;
    }
}