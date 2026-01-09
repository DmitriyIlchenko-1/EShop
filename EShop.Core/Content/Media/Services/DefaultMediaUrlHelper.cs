using EShop.Core.Content.Media.Configuration;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;

// namespace EShop.Core.Content.Media.Services;
//
// public class DefaultMediaUrlHelper : IMediaUrlHelper
// {
//     private readonly MediaSettings _mediaSettings;
//     private readonly string _host;
//
//     public DefaultMediaUrlHelper(MediaSettings mediaSettings, IHttpContextAccessor httpContextAccessor)
//     {
//         _mediaSettings = mediaSettings;
//         var httpContext = httpContextAccessor.HttpContext;
//         var cdnUrl = _mediaSettings.ContentDeliveryNetwork;
//         string basePath = "/";
//         if (httpContext != null)
//         {
//             if (cdnUrl.HasValue())
//             {
//                 _host = cdnUrl;
//             }
//             else
//             {
//                 basePath = httpContext.Request.PathBase.Value;
//                 _host = basePath;
//             }
//         }
//         
//         _host = _host.EmptyIfNull().EndsWith('/') ? _host : _host + "/";
//     }
//
//     public string GetUrl(MediaFile file)
//     {
//         
//     }
// }