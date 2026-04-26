using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Configuration;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Data;
using EShop.Web.Common.Conponents;
using EShop.Web.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Components;

public class HomePageProductsViewComponent : BaseViewComponent
{
    private readonly CatalogHelper _catalogHelper;
    private readonly ApplicationDbContext _dbContext;

    public HomePageProductsViewComponent(CatalogHelper catalogHelper, ApplicationDbContext dbContext)
    {
        _catalogHelper = catalogHelper;
        _dbContext = dbContext;
    }


    public async Task<IViewComponentResult> InvokeAsync()
    {
        var products = await _dbContext
            .Products.AsNoTracking()
            .Where(x => x.Published)
            .Where(x => x.ShowOnHomePage)
            .OrderBy(x => x.HomePageDisplayOrder)
            .SelectSummaryOnly()
            .Take(30)
            .ToListAsync();


        
        var modelSettings = _catalogHelper.GetProductSummaryMappingSettings();
        var model = await _catalogHelper.PrepareProductSummaryModelAsync(products, modelSettings, 
            new ProductVariantQuery()
            {
                
            });
        if (!model.Items.Any())
        {
            return NoContent();
        }

        return View(model);
    }
}