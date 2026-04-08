namespace EShop.Core.Catalog.Brands.Domain;

public interface IBrandService
{
    Task<ICollection<ProductBrand>> GetBrandsByProductIdsAsync(int[] productIds, bool includeUnpublished = false);
}