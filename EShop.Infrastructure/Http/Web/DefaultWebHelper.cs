using System.Net;
using System.Net.Sockets;
using System.Text;
using EShop.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace EShop.Core.Platform.Web;

public class DefaultWebHelper : IWebHelper
{
    private IPAddress? _ipAddress;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public DefaultWebHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public string GetClientIdentity()
    {
        var ipAddress = GetClientIpAddress();
        HttpContext?.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent);

        if (ipAddress != IPAddress.None && !string.IsNullOrWhiteSpace(userAgent))
        {
            var hashCode = System.IO.Hashing.XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(ipAddress + userAgent));
            return $"{hashCode:X}";
        }

        return null;
    }

    public HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public IPAddress GetClientIpAddress()
    {
        if (_ipAddress != null)
        {
            return _ipAddress;
        }

        var request = HttpContext?.Request;
        if (request == null)
        {
            return _ipAddress = IPAddress.None;
        }

        if (HttpContext?.Connection?.RemoteIpAddress is IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                ip = ip == IPAddress.IPv6Loopback ? IPAddress.Loopback : ip.MapToIPv4();
            }

            _ipAddress = ip;
        }

        return _ipAddress ?? IPAddress.None;
    }

    public string GetCurrentPageUrl(bool includeQueryString = false)
    {
        var r = HttpContext?.Request;
        if (r == null)
        {
            return string.Empty;
        }

        var page = r.Scheme + Uri.SchemeDelimiter + r.Host + r.Path;

        return includeQueryString ? page + r.QueryString.Value : page;
    }
}