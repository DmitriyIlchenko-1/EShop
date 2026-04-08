using EShop.Core.Platform.Themes.Extensions;
using EShop.Core.Platform.Themes.Services;
using EShop.Infrastructure.Common;
using EShop.Web.Common.TagHelpers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Web.Common.Razor;

public abstract class EShopRazorPage<TModel> : RazorPage<TModel>
{
    private IViewHelper _viewHelper;
    
    // lazy service resolution so we don't resolve services we don't end up using. 
    protected IViewHelper ViewHelper 
        => _viewHelper ??= ViewContext.HttpContext.RequestServices.GetRequiredService<IViewHelper>();
    protected dynamic Config => ViewHelper.GetThemeVariables();
    
    //TEMP:
    protected IThemeVariableService ServiceVariable 
        => ViewContext.HttpContext.RequestServices.GetRequiredService<IThemeVariableService>();
}