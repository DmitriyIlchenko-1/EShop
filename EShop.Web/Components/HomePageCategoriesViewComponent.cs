using EShop.Core.Catalog.Categories.Extensions;
using EShop.Core.Data;
using EShop.Web.Common.Conponents;
using EShop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Components;

public class HomePageCategoriesViewComponent : BaseViewComponent
{
    private readonly CatalogHelper _catalogHelper;
    private readonly ApplicationDbContext _dbContext;

    public HomePageCategoriesViewComponent(CatalogHelper catalogHelper, ApplicationDbContext dbContext)
    {
        _catalogHelper = catalogHelper;
        _dbContext = dbContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _dbContext
            .Categories.AsNoTracking()
            .ApplyStandardFilters(true)
            .Where(x => x.ShowOnHomePage)
            .ToListAsync();

        //TEMP: Has been checked.
        var model = await _catalogHelper.PrepareCategorySummaryModelAsync(categories);
        if (!model.Any())
        {
            return NoContent();
        }

        return View(model);
    }
}