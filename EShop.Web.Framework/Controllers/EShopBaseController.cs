
using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Platform.Logging.Filters;
using EShop.Core.Platform.Logging.Services;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using EShop.Web.Common.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;
using Controller = Microsoft.AspNetCore.Mvc.Controller;


namespace EShop.Web.Common.Controllers;

[NotificationFilter(Order = 1000)]
[SaveChanges<ApplicationDbContext>(Order = int.MaxValue)]
public abstract class EShopBaseController : Controller
{
    public ILogger Logger { get; set; } = NullLogger.Instance;

    protected virtual IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    protected ActionResult ForbidOrChallenge()
    {
       
        return User?.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
    }

    protected async Task<string> RenderPartialViewToStringAsync<T>(string viewName, T model, ViewDataDictionary viewData = null)
    {
        Guard.NotEmpty(viewName);
        Guard.NotNull(model);
        var factory = HttpContext.RequestServices.GetRequiredService<IViewRendererFactory>();
        var viewRenderer = factory.GetViewRenderer<PartialViewRendererDescriptor>();
        var desciptor = new PartialViewRendererDescriptor()
        {
            IsPartial = true,
            ViewName = viewName,
        };
        var result = await viewRenderer.RenderViewAsync(
            new ViewRendererContext(ControllerContext) { TempData = TempData, ViewData = (viewData ?? ViewData), Model = model },
            desciptor);
      
        return result.ToHtmlString().ToString();
    }

    protected async Task<string> RenderComponentToStringAsync(string componentName, object? arguments = null, ViewDataDictionary viewData = null)
    {
        Guard.NotEmpty(componentName);
       
        var factory = HttpContext.RequestServices.GetRequiredService<IViewRendererFactory>();
        var viewRenderer = factory.GetViewRenderer<ComponentViewRendererDescriptor>();
        var desciptor = new ComponentViewRendererDescriptor()
        {
            ComponentName = componentName,
            Arguments = arguments,
        };
        var result = await viewRenderer.RenderViewAsync(
            new ViewRendererContext(ControllerContext) { TempData = TempData, ViewData = (viewData ?? ViewData), },
            desciptor);
      
        return result.ToHtmlString().ToString();
        
    }
}