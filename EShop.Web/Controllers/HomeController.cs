using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Content.Widgets.Services;
using EShop.Core.Data.Extensions;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Logging.Services;
using EShop.Web.Common.Controllers;
using EShop.Web.Models.Home;
using EShop.Web.Models.Widgets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers
{
    public class HomeController : EShopBaseController
    {
        

        [HttpGet("/", Name = "Homepage")]
        public IActionResult Index()
        {
            
            //TODO: add home-page settings.
            return View();
        }
    }
}