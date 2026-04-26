namespace EShop.Core.Catalog.Brands.Domain;

public interface IBrandService
{
    Task<ICollection<Brand>> GetBrandsByIdsAsync(int[] brandIds,
        bool includeUnpublished = false, bool track = false);
}