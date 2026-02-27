using EShop.Core.Data;
using EShop.Core.Data.DbHandlers;
using EShop.Core.Platform.Logging.Filters;
using EShop.Core.Platform.Logging.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace EShop.Web.Common.Controllers;

[NotificationFilter(Order = 1000)]
[SaveChanges<ApplicationDbContext>(Order = int.MaxValue)]
public abstract class BaseController : Controller
{
    protected virtual IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
           
            return RedirectToAction("Index", "Home");
        }
    }
}