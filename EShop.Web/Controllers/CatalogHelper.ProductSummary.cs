using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Web.Models.Catalog;

namespace EShop.Web.Controllers;

public partial class CatalogHelper
{
    public async Task<IList<ProductSummaryModel>> PrepareProductSummaryModelAsync(IList<Product> products,
        bool preparePriceModel = true, bool prepareImageModel = true, bool prepareReviewModel = true,
        bool prepareSpecificationAttributes = false)
    {
        ArgumentNullException.ThrowIfNull(products);
        if (!(products.Count > 0))
            return [];

        var modelCollection = new List<ProductSummaryModel>();
        foreach (var product in products)
        {
            var model = new ProductSummaryModel
            {
                Id = product.Id,
                Name = product.Name,
                ShortDescription = product.ShortDescription,
                SeName = await _urlService.GetActiveSlugAsync(product.Id, product.Name),
                Sku = product.Sku,
            };

            if (preparePriceModel)
            {
                var price = _productPricingService.CalculateProductPrice(product);
                MapPriceModel(price, model.PriceModel);
            }

            if (prepareImageModel)
            {
                model.Images = await PrepareProductSummaryImageModelAsync(product);
            }

            if (prepareReviewModel)
            {
                ProductSummaryReviewModel reviewModel = new()
                {
                    ProductId = product.Id,
                    TotalReviews = product.ApprovedReviewCount,
                    RatingSum = product.ApprovedRatingSum,
                };

                model.ReviewModel = reviewModel;
            }

            modelCollection.Add(model);
        }

        return modelCollection;
    }


    public async Task<IList<CategorySummaryModel>> PrepareCategorySummaryModelAsync(IList<Category> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        var fileIds = categories
            .Select(x => x.MediaFileId ?? 0)
            .Where(x => x != 0)
            .Distinct()
            .ToArray();
        var allImages = (await _mediaService
                .GetMediaFilesByIdsAsync(fileIds, false))
            .ToDictionary(x => x.Id);
        
        //TODO: add caching;
        return await categories
            .SelectAsync(async category =>
            {
                allImages.TryGetValue(category.MediaFileId ?? 0, out var image);
                var model = new CategorySummaryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Url = await _urlService.GetActiveSlugAsync(category.Id, category.Name),
                    Image = new MediaModel
                    {
                        Url = await _mediaService.GetMediaUrlAsync(image),
                        Alt = image?.Alt,
                        Width = image?.Width,
                        Height = image?.Height,
                    }
                };
                
                return model;
            })
            .ToListAsync();
    }

    protected virtual async Task<IList<MediaModel>> PrepareProductSummaryImageModelAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        //TODO: add caching.

        async Task<MediaModel> MapToMediaModelAsync(MediaFile mediaFile)
        {
            var mediaUrl = await _mediaService.GetMediaUrlAsync(mediaFile);
            return new MediaModel()
            {
                Url = mediaUrl,
                Alt = mediaFile.Alt,
                Width = mediaFile.Width,
                Height = mediaFile.Height,
            };
        }

        var images =
            await _mediaService.GetFilesByProductIdAsync(product.Id,
                _catalogSettings.ShowTwoImagesOnHomePage ? 2 : 1,
                false);
        var imageModels = await images
            .SelectAsync(async image => await MapToMediaModelAsync(image))
            .ToListAsync();
        return imageModels;
    }


    protected void MapPriceModel(CalculatedProductPrice price, ProductSummaryPriceModel model)
    {
        model.Price = price.Price;
        model.OldPrice = price.OldPrice;
        model.PercentOfSaving = price.PercentOfSaving;
    }
}