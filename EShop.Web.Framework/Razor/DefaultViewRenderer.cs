using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ControllerContext = Microsoft.AspNetCore.Mvc.ControllerContext;
using ViewDataDictionary = Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary;

namespace EShop.Web.Common.Razor;

public interface IViewRenderer<T> where T : IViewRendererDescriptor
{
    Task<IHtmlContent> RenderViewAsync(ViewRendererContext context, T viewDescriptor);
}

public abstract class ViewRenderer<T> : IViewRenderer<T> where T : IViewRendererDescriptor
{
    public abstract Task<IHtmlContent> RenderViewAsync(ViewRendererContext context, T viewDescriptor);
    
    protected virtual ViewContext CreateViewContext(ViewRendererContext context, TextWriter writer)
    {
        var services = context.HttpContext.RequestServices;
        var tempData = context.TempData ?? services
            .GetRequiredService<ITempDataDictionaryFactory>()
            .GetTempData(context.HttpContext);
        var viewData = context.Model != null
            ? new ViewDataDictionary<object>(context.ViewData, context.Model)
            : context.ViewData;
        return new ViewContext(context.ActionContext,
            NullView.Instance,
            viewData,
            tempData,
            writer,
            services.GetRequiredService<IOptions<MvcViewOptions>>()
                .Value.HtmlHelperOptions);
    }
    
     
}

public class PartialViewRenderer : ViewRenderer<PartialViewRendererDescriptor>
{
    private readonly ICompositeViewEngine _viewEngine;

    public PartialViewRenderer(ICompositeViewEngine viewEngine)
    {
        _viewEngine = viewEngine;
       
    }

    public override async Task<IHtmlContent> RenderViewAsync(ViewRendererContext context,
        PartialViewRendererDescriptor descriptor)
    {
        Guard.NotNull(context);
        Guard.NotNull(descriptor);
        using var _ = StringBuilderPool.Pool.Get(out var sb);
        await using var writer = new StringWriter(sb);

        var viewContext = CreateViewContext(context, writer);
        var result = _viewEngine.FindView(context.ActionContext, descriptor.ViewName, !descriptor.IsPartial);
        result.EnsureSuccessful(originalLocations: null);
        var view = viewContext.View = result.View;
        using (view as IDisposable)
        {
            await view.RenderAsync(viewContext);
        }

        return new HtmlString(viewContext.Writer.ToString());
    }

     
}

public class ComponentViewRenderer : ViewRenderer<ComponentViewRendererDescriptor>
{
    public override async Task<IHtmlContent> RenderViewAsync(ViewRendererContext context, ComponentViewRendererDescriptor descriptor)
    {
        Guard.NotNull(context);
        Guard.NotNull(descriptor);
        if (descriptor.ComponentName.IsEmpty())
        {
            throw new ArgumentException("The component name is empty.", nameof(descriptor.ComponentName));
        }
        
        using var _ = StringBuilderPool.Pool.Get(out var sb);
        await using var writer = new StringWriter(sb);
        var viewContext = CreateViewContext(context, writer);

        var viewComponentHelper = context.HttpContext.RequestServices.GetRequiredService<IViewComponentHelper>();
        (viewComponentHelper as IViewContextAware)?.Contextualize(viewContext);
        return await viewComponentHelper.InvokeAsync(descriptor.ComponentName, descriptor.Arguments);
    }

    
}

public class ViewRendererContext
{
    public ViewRendererContext(ActionContext actionContext)
    {
        ActionContext = actionContext;
        HttpContext = actionContext.HttpContext;
    }

    public HttpContext HttpContext { get; set; }
    public ActionContext ActionContext { get; set; }
    public ViewDataDictionary ViewData { get; set; }
    public ITempDataDictionary TempData { get; set; }
    public object? Model { get; set; }
}

public  interface IViewRendererDescriptor
{
    
}

public class PartialViewRendererDescriptor : IViewRendererDescriptor
{
    public string ViewName { get; set; }

    public bool IsPartial { get; set; }
    
}
public class ComponentViewRendererDescriptor : IViewRendererDescriptor
{
    public string ComponentName { get; set; }

    public object? Arguments { get; set; }
    
}

public interface IViewRendererFactory
{
    IViewRenderer<TViewDescriptor> GetViewRenderer<TViewDescriptor>() where TViewDescriptor : IViewRendererDescriptor;
}
public class DefaultViewRendererFactory : IViewRendererFactory
{
    private readonly HttpContext _httpContext;

    public DefaultViewRendererFactory(IHttpContextAccessor acc)
    {
        _httpContext = acc?.HttpContext;
    }

    public IViewRenderer<TViewDescriptor> GetViewRenderer<TViewDescriptor>() where TViewDescriptor : IViewRendererDescriptor 
    {
        return _httpContext?.RequestServices.GetRequiredService<IViewRenderer<TViewDescriptor>>();
    }
}

public sealed class NullView : IView
{
    public static readonly NullView Instance = new NullView();
    public string Path => string.Empty;
    public Task RenderAsync(ViewContext context)
    {
        if (context == null) { throw new ArgumentNullException(nameof(context)); }
        return Task.CompletedTask;
    }
}