using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Content.Widgets.Services;
using EShop.Core.Platform.Identity.Domain;
using EShop.Web.Models.Home;
using EShop.Web.Models.Widgets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/", Name = "Homepage")]
        public IActionResult Index()
        {
            //TODO: add home-page settings.
            return View();
        }
    }
}