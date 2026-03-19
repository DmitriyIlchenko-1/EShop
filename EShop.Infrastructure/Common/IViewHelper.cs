using Microsoft.AspNetCore.Http;

namespace EShop.Infrastructure.Common;

public interface IViewHelper
{
    public HttpContext HttpContext { get; set; }
}