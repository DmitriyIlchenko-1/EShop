using Microsoft.AspNetCore.Http;

namespace EShop.Infrastructure.Common;

public class DefaultViewHelper : IViewHelper
{
    public HttpContext HttpContext { get; init; }

    public DefaultViewHelper(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor.HttpContext;
    }
}