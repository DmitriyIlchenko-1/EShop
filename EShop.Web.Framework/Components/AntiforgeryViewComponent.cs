using EShop.Web.Common.Conponents;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Common.Components;

public class AntiforgeryViewComponent : BaseViewComponent
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryViewComponent(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public IViewComponentResult Invoke()
    {
        var hasStarted = HttpContext.Response.HasStarted;
        var tokenSet = hasStarted ? _antiforgery.GetTokens(HttpContext) 
            : _antiforgery.GetAndStoreTokens(HttpContext);
        return HtmlContent($"<meta name='csrf-token' content='{tokenSet.RequestToken}'");
    }
}