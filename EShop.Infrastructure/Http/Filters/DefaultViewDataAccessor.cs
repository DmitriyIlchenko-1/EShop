using EShop.Infrastructure.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EShop.Web.Common.Filters;

public interface IViewDataAccessor : IActionFilter
{
    ViewDataDictionary? ViewData { get; }
}

public class DefaultViewDataAccessor : IViewDataAccessor
{
    private const string ViewDataAccessKey = "ViewData";
    private readonly IRequestCache _requestCache;

    public DefaultViewDataAccessor(IRequestCache requestCache)
    {
        _requestCache = requestCache;
    }

    public ViewDataDictionary? ViewData { get; private set; }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controller = context.Controller as Controller;
        if (controller != null)
        {
            _requestCache.Put(ViewDataAccessKey, controller.ViewData);
            ViewData = controller.ViewData;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}