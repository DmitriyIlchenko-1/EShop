namespace EShop.Core.Catalog.Brands.Domain;

public interface IBrandService
{
    Task<ICollection<ProductBrand>> GetBrandsByProductIdsAsync(int[] brandIds,
        bool includeUnpublished = false, bool track = false);
}