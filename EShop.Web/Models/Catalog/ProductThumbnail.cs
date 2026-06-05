using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Content.Media.Domain;

namespace EShop.Web.Models.Catalog;

public class ProductThumbnail
{
    public CalculatedProductPrice CalculatedProductPrice { get; set; }

    public bool HasOptions { get; set; }

    public long Id { get; set; }

    public bool IsAllowToOrder { get; set; }

    public bool IsVisibleIndividually { get; set; }

    public string Name { get; set; }

    public decimal? OldPrice { get; set; }

    public decimal Price { get; set; }

    public double? RatingAverage { get; set; }

    public int? ReviewsCount { get; set; }

    public string Slug { get; set; }

    public decimal? SpecialPrice { get; set; }

    public DateTime? SpecialPriceEnds { get; set; }

    public DateTime? SpecialPriceStarts { get; set; }

    public int StockQuantity { get; set; }

    public MediaFile ThumbnailImage { get; set; }

    public string ThumbnailUrl { get; set; }

    public static ProductThumbnail FromProduct(Product product)
    {
        return new ProductThumbnail
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            HasOptions = product.HasOptions,
            IsVisibleIndividually = product.IsVisibleIndividually,
            StockQuantity = product.StockQuantity,
        };
    }
}