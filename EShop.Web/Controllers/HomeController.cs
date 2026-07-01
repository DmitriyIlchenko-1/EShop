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
        private readonly ILogger _logger;
        private readonly IWidgetInstanceService _widgetInstanceService;
        private readonly UserManager<User> _userManager;

        public HomeController(ILoggerFactory loggerFactory, IWidgetInstanceService widgetInstanceService, UserManager<User> userManager)
        {
            _logger = loggerFactory.CreateLogger("Unhandled Error");
            _widgetInstanceService = widgetInstanceService;
            _userManager = userManager;
        }

        [HttpGet("/", Name = "Homepage")]
        public async Task<IActionResult> Index(ProductVariantQuery query)
        {
            //TODO: add home-page settings.

            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("secure")]
        public string Secure()
        {
            return "secure data";
        }
        
        
        [HttpGet("diactivate")]
        public async Task DiactivateUser()
        {
            var user = await _userManager.FindByEmailAsync("indicator18@gmail.com");
            user.IsActive = false;
            await _userManager.UpdateSecurityStampAsync(user);
        }
        [HttpGet("activate-back")]
        public async Task ActivateUser()
        {
            var user = await _userManager.FindByEmailAsync("indicator18@gmail.com");
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
        }
    }
}