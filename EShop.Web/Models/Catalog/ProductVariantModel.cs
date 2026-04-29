namespace EShop.Web.Models.Catalog;

public class ProductVariantModel
{
    public int Id { get; set; }
    public string Alias { get; set; }
    public string Name { get; set; }
    public List<ProductVariantValueModel> Values { get; set; }
}

public class ProductVariantValueModel
{
    public string Alias { get; set; }

    public string Color { get; set; }

    public string Name { get; set; }
}