using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Configuration;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Common.Services;
using EShop.Core.Content.Media.Services;
using EShop.Core.Data;
using EShop.Core.Platform.Routing;
using EShop.Infrastructure.Extensions;
using EShop.Web.Common.Models.Choices;
using EShop.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EShop.Web.Controllers;

public partial class CatalogHelper
{
    private readonly IMediaService _mediaService;
    private readonly IProductService _productService;
    private readonly IProductPricingService _productPricingService;
    private readonly IProductAttributeMaterializer _productAttributeMaterializer;
    private readonly IDeliveryTimeService _deliveryTimeService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IBrandService _brandService;
    private readonly IUrlService _urlService;
    private readonly ApplicationDbContext _db;
    private readonly CatalogSettings _catalogSettings;

    public CatalogHelper(IMediaService mediaService, IProductPricingService productPricingService,
        IProductService productService, IProductAttributeMaterializer productAttributeMaterializer,
        IDeliveryTimeService deliveryTimeService, ApplicationDbContext db, IDateTimeService dateTimeService,
        IUrlService urlService, CatalogSettings catalogSettings, IBrandService brandService)
    {
        _mediaService = mediaService;
        _productPricingService = productPricingService;
        _productService = productService;
        _productAttributeMaterializer = productAttributeMaterializer;
        _deliveryTimeService = deliveryTimeService;
        _db = db;
        _dateTimeService = dateTimeService;
        _urlService = urlService;
        _catalogSettings = catalogSettings;
        _brandService = brandService;
    }

    public ProductDetailsModelContext CreateModelContext(Product product, ProductVariantQuery query)
    {
        return new ProductDetailsModelContext(
            product,
            query,
            _productService.CreateProductBatchContext(new[] { product }));
    }

    // public ProductSummaryItemContext CreateSummaryItemContext(Product product, ProductVariantQuery query)
    // {
    //     
    // }


    public async Task<ProductDetailVm> MapProductDetailsPageModelAsync(Product product,
        ProductVariantQuery? variantQuery)
    {
        ArgumentNullException.ThrowIfNull(product);
        var context = CreateModelContext(product, variantQuery);

        var model = new ProductDetailVm();

        await PrepareProductSpecificationModelAsync(context, model);
        await PrepareProductAttributeModelAsync(context, model);
        await PrepareRelatedProductModelAsync(context, model);

        //Should be called after these three methods. 
        await PrepareProductPropertiesModelAsync(context, model);


        model.ProductReviews = new ProductReviewsModel();
        await PrepareProductReviewModelAsync(model.ProductReviews, product);

        return model;
    }

    public async Task PrepareProductDetailModelAsync(ProductDetailVm model, ProductDetailsModelContext context,
        int selectedQuantity = 1)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;

        await PrepareProductAttributeModelAsync(context, model);

        await PrepareProductPropertiesModelAsync(context, model);
    }


    public async Task<IList<BrandSummaryModel>> PrepareBrandModelAsync(IList<Brand> brands)
    {
        ArgumentNullException.ThrowIfNull(brands);
        var fileIds = brands
            .Select(x => x.MediaFileId ?? 0)
            .Where(x => x != 0)
            .Distinct()
            .ToArray();
        var allImages = (await _mediaService
                .GetMediaFilesByIdsAsync(fileIds, false))
            .ToDictionary(x => x.Id);

        //TODO: add caching.
        var brandModels = await brands
            .SelectAsync(async b =>
            {
                allImages.TryGetValue(b.MediaFileId ?? 0, out var image);
                return new BrandSummaryModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Image = new MediaModel()
                    {
                        Id = b.Id,
                        Alt = image?.Alt,
                        Height = image?.Height,
                        Width = image?.Width,
                        Url = await _urlService.GetActiveSlugAsync(b.Id, b.Name)
                    }
                };
            })
            .ToListAsync();

        return brandModels;
    }

    protected virtual async Task<MediaModel> PrepareBrandImageModelAsync(Brand brand)
    {
        throw new NotImplementedException();
    }


    private async Task PrepareProductReviewModelAsync(ProductReviewsModel model, Product product, int take = 10)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(product);

        model.ProductId = product.Id;
        model.ProductName = product.Name;

        model.ReviewItems = await _db
            .ProductReviews
            .Where(x => x.ProductId == product.Id && x.ReviewStatus == ReviewStatus.Approved)
            .OrderByDescending(x => x.CreatedOnUtc)
            .Select(x => new ProductReviewItemModel()
            {
                Id = x.Id,
                Title = x.Title,
                ReviewerName = x.ReviewerName,
                CreatedOn = x.CreatedOnUtc,
                EditedOn = x.ModifiedOnUtc,
                CommentText = x.CommentText,
                Rating = x.Rating,
                Replies = x
                    .Replies.Where(
                        x => x.ReplyStatus == ReplyStatus.Approved)
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .Select(x => new ReplyModel()
                    {
                        ReplyText = x.ReplyText,
                        ReplierName = x.ReplierName,
                        // This line will get executed by EF in the server, not in the database.
                        CreatedOn = _dateTimeService.ConvertToLocalTimeZoneFromUtc(x.CreatedOnUtc)
                    })
                    .ToList()
            })
            .Take(take)
            .ToListAsync();


        model.TotalReviewsCount = model.ReviewItems.Count;
        model.Rating1Count = model.ReviewItems.Count(x => x.Rating == 1);
        model.Rating2Count = model.ReviewItems.Count(x => x.Rating == 2);
        model.Rating3Count = model.ReviewItems.Count(x => x.Rating == 3);
        model.Rating4Count = model.ReviewItems.Count(x => x.Rating == 4);
        model.Rating5Count = model.ReviewItems.Count(x => x.Rating == 5);
    }

    private async Task PrepareProductAttributeModelAsync(ProductDetailsModelContext context, ProductDetailVm model)
    {
        var product = context.Product;
        var batchContext = context.LazyContext;
        var query = context.ProductVariantQuery;
        var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);
        var weightAdjustment = 0m;
        foreach (var attribute in attributes)
        {
            var attributeVm = new ProductVariantAttributeModel()
            {
                Id = attribute.Id,
                ProductId = attribute.ProductId,
                ProductAttributeId = attribute.ProductAttributeId,
                ProductVariantAttribute = attribute,
                Alias = attribute.ProductAttribute.Alias,
                Name = attribute.ProductAttribute.Name,
                Description = attribute.ProductAttribute.Description,
                TextPrompt = attribute.ProductAttribute.TextPrompt,
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType,
            };

            // if (query.Variants.Count > 0)
            // {
            //     var selectedAttribute = query.Variants.FirstOrDefault(x =>
            //         x.ProductId == attribute.ProductId && x.AttributeId == attribute.ProductAttributeId &&
            //             x.VariantAttributeId == attribute.Id);
            //     
            // }


            if (attribute.IsListTypeAttribute())
            {
                List<ProductVariantAttributeValueModel> attrValueVms = attribute
                    .ProductVariantAttributeValues
                    .Select(value =>
                    {
                        var valueVm = new ProductVariantAttributeValueModel()
                        {
                            Id = value.Id,
                            ProductVariantAttributeValue = value,
                            PriceAdjustment = string.Empty,
                            Name = value.Name,
                            Alias = value.Alias,
                            Color = value.Color,
                            IsPreSelected = value.IsPreSelected,
                            DisplayOrder = value.DisplayOrder,
                            QuantityInfo = value.Quantity
                        };
                        if (value.IsPreSelected)
                        {
                            weightAdjustment += value.WeightAdjustment;
                        }

                        return valueVm;
                    })
                    .ToList();

                attributeVm.Values = attrValueVms
                    .Select(x => (ChoiceItemModel)x)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();
            }

            foreach (var val in attributeVm.Values.Where(x => x.IsPreSelected))
            {
                // query.AddVariant();
            }

            model.ProductVariantAttributes.Add(attributeVm);
        }

        if (query != null && (query.Variants.Count > 0))
        {
            var sltAttrs =
                _productAttributeMaterializer.CreateAttributeSelectionAsync(query, attributes, product.Id);
            // model.SelectedCombination =
            //     (await _productAttributeMaterializer.FindProductVariantAttributeCombinationsAsync(
            //         new Dictionary<int, ProductVariantAttributeSelection>() { { product.Id, sltAttrs } }))
            //     .Select(x => x.Value)
            //     .FirstOrDefault();
            
            // (Product Product, ProductCombinationMap Model, ProductLazyContext LazyCtx,
            //     ICollection<ProductVariantAttributeModel> ProductVariantAttributes, ProductVariantAttributeSelection
            //     Selection, ProductVariantAttributeCombination SelectedCombination) attributeMappingCtx =
            //         (product, model, batchContext, model.ProductVariantAttributes, sltAttrs, model.SelectedCombination);
            // await PrepareProductSummaryAttributeCombinationModelAsync(attributeMappingCtx);
        }
    }

//     private async Task PrepareProductAttributeCombinationsModelAsync(ProductDetailsModelContext context,
//     ProductDetailVm model)
// {
//     var product = context.Product;
//     var batchContext = context.LazyContext;
//     var query = context.ProductVariantQuery;
//     var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);
//
//     //selection (attribute + selected value) of all the attributes displayed. 
//     context.SelectedAttributes =
//         _productAttributeMaterializer.CreateAttributeSelectionAsync(query, attributes, product.Id);
//
//     var selectedValues =
//         _productAttributeMaterializer.MaterializeProductVariantAttributeValues(context.SelectedAttributes, attributes);
//     var selectedValueIds = selectedValues
//         .Select(x => x.Id)
//         .ToArray();
//
//
//     model.SelectedCombination =
//         (await _productAttributeMaterializer.FindProductVariantAttributeCombinationsAsync(
//             new Dictionary<int, ProductVariantAttributeSelection>() { { product.Id, context.SelectedAttributes } }))
//         .FirstOrDefault();
//
//     //more out to a mapper
//     // if (model.SelectedCombination != null && !model.SelectedCombination.IsActive && model.SelectedCombination.StockQuantity == 0)
//     // {
//     //     model.IsAvailable = false;
//     // }
//
//     //TODO: Come up with an idea about how to merge the data between the product and the selection. 
//     // MergeWithCombination();
//
//     foreach (var attribute in model.ProductVariantAttributes.Where(x => x.IsActive))
//     {
//         //any value of the attribute intersects with any user chosen value for this attribute.
//         // In other words, has the user selected a particular value for this attribute? 
//         var updatePreselection = selectedValueIds.Length > 0 && selectedValueIds
//             .Intersect(attribute.Values.Select(x => x.Id))
//             .Any();
//
//
//         foreach (ProductVariantAttributeValueModel value in
//                  attribute.Values.Cast<ProductVariantAttributeValueModel>())
//         {
//             var isSelected = selectedValueIds.Contains(value.Id);
//             if (updatePreselection)
//             {
//                 //set to false or true depending on which value the user has chosen. 
//                 value.IsPreSelected = isSelected;
//             }
//
//             if (isSelected)
//             {
//                 // model.Weight += value.ProductVariantAttributeValue.WeightAdjustment;
//             }
//         }
//     }
// }


    private async Task PrepareProductPropertiesModelAsync(ProductDetailsModelContext context, ProductDetailVm model)
    {
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;
        var combination = model.SelectedCombination;

        model.Id = product.Id;
        model.Name = product.Name;
        model.Brand = product.Brand;
        model.MetaTitle = product.MetaTitle;
        model.MetaDescriptions = product.MetaTitle;
        model.Description = product.Description;
        model.ShortDescription = product.ShortDescription;
        model.IsAvailable = product.IsAvailable;
        model.StockQuantity = product.StockQuantity;
        model.RatingAverage = product.ApprovedRatingSum;
        model.ReviewsCount = product.ApprovedReviewCount;


        model.CalculatedProductPrice = combination is { Price: not null }
            ? _productPricingService.CalculateProductPrice(price: combination.Price.Value,
                oldPrice: combination.OldPrice,
                specialPrice: combination.SpecialPrice,
                specialPriceEnd: combination.SpecialPriceEnd,
                specialPriceStart: combination.SpecialPriceStarts)
            : _productPricingService.CalculateProductPrice(product);

        model.Sku = combination is { Sku: not null } ? combination.Sku : product.Sku;
        model.Gtin = combination is { Sku: not null } ? combination.Sku : product.Sku;

        model.Weight = model.WidthValue > 0 ? $"{combination.Height:G29}" : string.Empty;
        model.Height = combination?.Height > 0 ? $"{combination.Height:G29}" :
            product.Height > 0 ? $"{product.Height:G29}" : string.Empty;
        model.Length = combination?.Length > 0 ? $"{combination.Length:G29}" :
            product.Length > 0 ? $"{product.Length:G29}" : string.Empty;
        model.Width = combination?.Width > 0 ? $"{combination.Width:G29}" :
            product.Width > 0 ? $"{product.Width:G29}" : string.Empty;


        // Delivery time 
        if (combination?.DeliveryTimeId is > 0 && model.IsAvailable)
        {
            var deliveryTime = await _deliveryTimeService.GetDeliveryTimeAsync(combination.DeliveryTimeId);
            if (deliveryTime != null)
            {
                model.DeliveryTimeDate = _deliveryTimeService.GetFormattedDeliveryDate(deliveryTime);
            }
        }
    }


    private async Task PrepareRelatedProductModelAsync(ProductDetailsModelContext context, ProductDetailVm model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;
        var batchContext = context.LazyContext;
        var relatedProducts = await batchContext.RelatedProducts.GetOrLoadAsync(product.Id);

        foreach (ProductLink productLink in relatedProducts)
        {
            var relProduct = productLink.LinkedProduct;
            var relProductVm = ProductThumbnail.FromProduct(relProduct);
            //relProductVm.ThumbnailUrl = _mediaService.GetMediaUrl(relProduct.ThumbnailImage);
            relProductVm.CalculatedProductPrice = _productPricingService.CalculateProductPrice(relProduct);
            model.RelatedProducts.Add(relProductVm);
        }
    }

    private async Task PrepareProductSpecificationModelAsync(ProductDetailsModelContext context,
        ProductDetailVm model)
    {
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;
        var batchContext = context.LazyContext;
        var specs = await batchContext.ProductSpecification.GetOrLoadAsync(product.Id);
        model.ProductSpecifications = specs
            .Select(x => new ProductSpecificationModel()
            {
                SpecificationAttributeId = x.SpecificationAttributeOption.SpecificationAttributeId,
                SpecificationAttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                Essential = x.SpecificationAttributeOption.SpecificationAttribute.IsEssential,
                SpecificationAttributeOption = x.SpecificationAttributeOption.Name
            })
            .ToList();
    }
}