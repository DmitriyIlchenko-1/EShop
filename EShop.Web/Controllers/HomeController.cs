using EShop.Web.Common.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Web.Controllers;

public class HomeController : EShopBaseController
{
    
    [HttpGet("/", Name = "Homepage")]
    public IActionResult Index()
    {
        //TODO: add home-page settings.
        return View();
    }
}