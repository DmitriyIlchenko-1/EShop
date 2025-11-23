namespace EShop.Web.Models.Catalog;

public class ProductDetailCategoryModel
{
    public ProductDetailCategoryModel(long id, string name, string slug)
    {
        Id = id;
        Name = name;
        Slug = slug;
    }

    public long Id { get; set; }

    public string Name { get; set; }

    public string Slug { get; set; }
}