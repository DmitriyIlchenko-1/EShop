using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Data;
using EShop.Infrastructure.Extensions;
using EShop.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Components;

public class CategoryBreadcrumbViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public CategoryBreadcrumbViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }


    public async Task<IViewComponentResult> InvokeAsync(int? categoryId, IEnumerable<int> categoryIds)
    {
        List<BreadcrumModel> breadcrumModels;
        if (categoryId.HasValue)
        {
            breadcrumModels = await PrepareRoutesAsync(categoryId.Value);
        }
        else
        {
            var collection = await categoryIds
                .SelectAsync(async id => await PrepareRoutesAsync(id))
                .ToListAsync();
            breadcrumModels = collection
                .OrderByDescending(x => x.Count)
                .First();
        }
        
        return View(breadcrumModels);
    }

    private async Task<List<BreadcrumModel>> PrepareRoutesAsync(int categoryId)
    {
        List<BreadcrumModel> breadcrumModels = [];
        var category = await _db
            .Categories.AsNoTracking()
            .Include(category => category.Parent)
            .FirstOrDefaultAsync(x => x.Id == categoryId);

        breadcrumModels.Add(new BreadcrumModel()
        {
            RouteName = category.Name,
            Url = category.Slug
        });


        var parent = category.Parent;
        while (parent != null)
        {
        }

        return breadcrumModels;
    }
}