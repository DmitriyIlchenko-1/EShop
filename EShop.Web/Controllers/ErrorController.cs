using System.Net;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.IO;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Error;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;


namespace EShop.Web.Controllers;

public class ErrorController : Controller
{
    [Route("error/{status?}")]
    public IActionResult Error(int? status)
    {
        Enum.TryParse((status ?? HttpContext.Response.StatusCode).ToString(),
            true,
            out HttpStatusCode statusCode);
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        var model = new ErrorModel
        {
            Status = statusCode,
            Exception = exceptionHandlerPathFeature?.Error,
            Path = exceptionHandlerPathFeature?.Path ?? statusCodeReExecuteFeature?.OriginalPath
        };

        if (Request.IsRequestFetch())
        {
            return Json(model);
        }

        if (IsFilePathError(model) && MimeTypes.TryGetContentType(model.Path, out var contentType))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Content("Resource not found", contentType);
        }

        switch (statusCode)
        {
            case HttpStatusCode.NotFound:
                return View("NotFound");
            default:
                return View("Error");
        }
    }

    private static bool IsFilePathError(ErrorModel model)
    {
        return model.Status == HttpStatusCode.NotFound ||
               model.Exception is FileNotFoundException ||
               model.Exception is DirectoryNotFoundException ||
               model.Exception is PathTooLongException ||
               model.Exception is NotSupportedException ||
               model.Exception is IOException;
    }
}