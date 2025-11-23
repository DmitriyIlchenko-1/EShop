// using EShop.Core.Catalog.Configuration;
// using EShop.Core.Catalog.Products.Domain;
// using EShop.Core.Data;
// using EShop.Web.Controllers;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
//
// namespace EShop.Web.Components;
//
// public class HomePageProductsViewComponents : ViewComponent
// {
//     private readonly CatalogHelper _catalogHelper;
//     private readonly ApplicationDbContext _dbContext;
//     private readonly CatalogSettings _catalogSettings;
//
//     public HomePageProductsViewComponents(CatalogHelper catalogHelper, ApplicationDbContext dbContext, CatalogSettings catalogSettings)
//     {
//         _catalogHelper = catalogHelper;
//         _dbContext = dbContext;
//         _catalogSettings = catalogSettings;
//     }
//
//
//     public async Task<IViewComponentResult> InvokeAsync()
//     {
//         var products = await _dbContext
//             .Products.AsNoTracking()
//             .Where(x => x.Published)
//             .Where(x => x.ShowOnHomePage)
//             .OrderBy(x => x.HomePageDisplayOrder)
//             .Select(x =>  new Product()
//             {
//                Id = x.Id,
//             })
//             .ToListAsync();
//     }
// }