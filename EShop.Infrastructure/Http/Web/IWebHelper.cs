using System.Net;
using Microsoft.AspNetCore.Http;

namespace EShop.Core.Platform.Web;

public interface IWebHelper
{
    HttpContext HttpContext { get; }
    string GetClientIdentity();
    IPAddress GetClientIpAddress();

    string GetCurrentPageUrl(bool includeQueryString = false);

}