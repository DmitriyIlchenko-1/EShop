using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Content.Widgets.Services;
using EShop.Web.Models.Home;
using EShop.Web.Models.Widgets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IWidgetInstanceService _widgetInstanceService;

        public HomeController(ILoggerFactory loggerFactory, IWidgetInstanceService widgetInstanceService)
        {
            _logger = loggerFactory.CreateLogger("Unhandled Error");
            _widgetInstanceService = widgetInstanceService;
        }

        public async Task<IActionResult> Index(ProductVariantQuery query)
        {
            //TODO: add home-page settings.

            return View();
        }


        [Authorize]
        public string Secure()
        {
            return "secure data";
        }
    }
}