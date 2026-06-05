using System.Net;
using Microsoft.AspNetCore.Http;

namespace EShop.Core.Platform.Web;

public interface IWebHelper
{
    HttpContext HttpContext { get; }
    string GetClientIdentity();
    IPAddress GetClientIpAddress();
    bool IsCurrentConnectionSecured();

    string GetCurrentPageUrl(bool includeQueryString = false);

}