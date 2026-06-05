using EShop.Core.Catalog.Categories.Domain;

namespace EShop.Core.Data.Categories.Services;

public interface ICategoryService
{
    Task<ICollection<ProductCategory>> GetAllCategoriesByProductIds(int[] productIds,
        bool tracking = false,
        bool includeHidden = false);
}