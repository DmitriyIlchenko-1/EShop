// using EShop.Core.Catalog.Brands.Extensions;
// using EShop.Core.Catalog.Configuration;
// using EShop.Core.Data;
// using EShop.Web.Common.Conponents;
// using EShop.Web.Controllers;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
//
// namespace EShop.Web.Components;
//
// public class HomePageBrandsViewComponent : BaseViewComponent
// {
//     private readonly CatalogHelper _catalogHelper;
//     private readonly CatalogSettings _catalogSettings;
//     private readonly ApplicationDbContext _dbContext;
//
//     public HomePageBrandsViewComponent(CatalogHelper catalogHelper, ApplicationDbContext dbContext)
//     {
//         _catalogHelper = catalogHelper;
//         _dbContext = dbContext;
//     }
//
//     public async Task<IViewComponentResult> InvokeAsync()
//     {
//         //TEMP: checked;
//         if (_catalogSettings.BrandCountOnHomePage > 0)
//         {
//             var homePageBrands = await _dbContext
//                 .Brands.AsNoTracking()
//                 .ApplyStandardFilters()
//                 .Take(_catalogSettings.BrandCountOnHomePage)
//                 .ToListAsync();
//
//             var models = await _catalogHelper.PrepareBrandModelAsync(homePageBrands);
//
//             if (models.Any())
//             {
//                 return View(models);
//             }
//         }
//
//
//         return NoContent();
//     }
// }