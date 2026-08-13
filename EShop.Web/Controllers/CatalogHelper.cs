using System.Globalization;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Configuration;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Extensions;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Content.Media.Services;
using EShop.Core.Data;
using EShop.Core.Platform.Common;
using EShop.Core.Platform.Routing;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Extensions;
using EShop.Web.Common.Models.Choices;
using EShop.Web.Infrastructure.DbHandlers;
using EShop.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EShop.Web.Controllers;

public partial class CatalogHelper
{
    private readonly IMediaService _mediaService;
    private readonly IProductService _productService;
    private readonly IProductPriceService _productPriceService;
    private readonly IProductAttributeMaterializer _productAttributeMaterializer;
    private readonly IDeliveryTimeService _deliveryTimeService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IBrandService _brandService;
    private readonly IUrlService _urlService;
    private readonly IUrlHelper _urlHelper;
    private readonly ICacheManagerFactory _cacheFactory;
    private readonly ApplicationDbContext _db;
    private readonly CatalogSettings _catalogSettings;
    private readonly PerformanceSettings _performanceSettings;
    readonly InventorySettings _inventorySettings;

    public CatalogHelper(IMediaService mediaService,
        IProductService productService, IProductAttributeMaterializer productAttributeMaterializer,
        IDeliveryTimeService deliveryTimeService, ApplicationDbContext db, IDateTimeService dateTimeService,
        IUrlService urlService, CatalogSettings catalogSettings, IBrandService brandService,
        IProductPriceService productPriceService, PerformanceSettings performanceSettings, IUrlHelper urlHelper,
        ICacheManagerFactory cacheFactory, InventorySettings inventorySettings)
    {
        _mediaService = mediaService;
        _productService = productService;
        _productAttributeMaterializer = productAttributeMaterializer;
        _deliveryTimeService = deliveryTimeService;
        _db = db;
        _dateTimeService = dateTimeService;
        _urlService = urlService;
        _catalogSettings = catalogSettings;
        _brandService = brandService;
        _productPriceService = productPriceService;
        _performanceSettings = performanceSettings;
        _urlHelper = urlHelper;
        _cacheFactory = cacheFactory;
        _inventorySettings = inventorySettings;
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


    public async Task<ProductDetailModel> MapProductDetailsPageModelAsync(Product product,
        ProductVariantQuery? variantQuery, ProductBrand productBrand, int selectedQuantity = 1)
    {
        ArgumentNullException.ThrowIfNull(product);
        var context = CreateModelContext(product, variantQuery);

        var model = new ProductDetailModel()
        {
            Id = product.Id,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            MetaTitle = product.MetaTitle,
            MetaDescriptions = product.MetaDescription,
            IsAvailable = product.IsAvailable,
            Sku = product.Sku,
            Gtin = product.Gtin,
            UpdateUrl = _urlHelper.Action(nameof(ProductController.UpdateProductDetails), "Product", new
            {
                productId = product.Id,
            })
        };

        #region Brand

        if (productBrand != null)
        {
            model.Brand = await PrepareBrandSummaryModelAsync(productBrand.Brand);
        }
       

        #endregion

        #region Specifications

        await PrepareProductSpecificationModelAsync(context, model);

        #endregion

        #region RelatedProducts

        //TODO: not finished, first you have to write HTML markup to display this
        await PrepareRelatedProductModelAsync(context, model);

        #endregion

        var productMedia = await context.BatchContext.ProductMedia.GetOrLoadAsync(product.Id);
        model.Images = await PrepareProductImageModelAsync(productMedia);
        

        #region CORE: Product Attributes, Price, Property Mapping

        await PrepareProductDetailModelAsync(model, context);

        #endregion


        return model;
    }

    protected virtual async Task PrepareProductPriceModelAsync(ProductDetailsModelContext ctx, ProductDetailModel model,
        int selectedQuantity)
    {
        // Don't calculate product's price if it's unavailable to order.
        if (!model.IsAvailable)
        {
            return;
        }
        var priceCalculationContext = new PriceCalculationContext()
        {
            Quantity = selectedQuantity,
            Product = ctx.Product,
            BatchContext = ctx.BatchContext
        };
        var calculatedPrice = await _productPriceService.CalculatePriceAsync(priceCalculationContext);
        model.FinalPrice = calculatedPrice.FinalPrice;
        model.RegularPrice = calculatedPrice.RegularPrice;
        model.Saving = calculatedPrice.PriceSaving;

        if (calculatedPrice.FinalPrice == 0)
        {
            model.FinalPrice = model.FinalPrice.WithPostFormat("Free");
        }

        if (model.Saving.HasSaving)
        {
            model.Labels.Add(new ProductLabelModel()
            {
                Name = SystemLabelNames.Sale,
                Content = string.Format(CultureInfo.InvariantCulture,
                    SystemLabelNames.SaleTemplate,
                    model.Saving.SavingPercent.ToString("F1"))
            });
        }
    }


    public async Task PrepareProductDetailModelAsync(ProductDetailModel model, ProductDetailsModelContext context,
        int selectedQuantity = 1)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;

        #region Attributes

        await PrepareProductAttributeModelAsync(context, model);

        #endregion

        #region Price

        await PrepareProductPriceModelAsync(context, model, selectedQuantity);

        #endregion

        #region Property Mapping

        //Should be called after these three methods. 
        await PrepareProductPropertiesModelAsync(context, model);

        #endregion
        
        #region Labels

        // Allow to display only 5 custom labels for a product
        var customLabels = await _db
            .ProductLabels.AsNoTracking()
            .Include(x => x.Label)
            .Where(x => x.ProductId == product.Id)
            .Take(5)
            .OrderBy(x => x.Order)
            .Select(x => new ProductLabelModel
            {
                Name = x.Label.Name,
                Content = x.Label.Content
            })
            .ToListAsync();
        foreach (var label in customLabels)
        {
            model.Labels.Add(label);
        }

        #endregion
    }
    
 


    // public async Task<IList<BrandSummaryModel>> PrepareBrandModelAsync(IList<Brand> brands)
    // {
    //     ArgumentNullException.ThrowIfNull(brands);
    //     var fileIds = brands
    //         .Select(x => x.MediaFileId ?? 0)
    //         .Where(x => x != 0)
    //         .Distinct()
    //         .ToArray();
    //     var allImages = (await _mediaService
    //             .GetMediaFilesByIdsAsync(fileIds, false))
    //         .ToDictionary(x => x.Id);
    //
    //     //TODO: add caching.
    //     var brandModels = await brands
    //         .SelectAsync(async b =>
    //         {
    //             allImages.TryGetValue(b.MediaFileId ?? 0, out var image);
    //             return new BrandSummaryModel
    //             {
    //                 Id = b.Id,
    //                 Name = b.Name,
    //                 Image = new ImageModel()
    //                 {
    //                     Id = b.Id,
    //                     Alt = image?.Alt,
    //                     Height = image.Height,
    //                     Width = image.Width,
    //                     Url = await _urlService.GetActiveSlugAsync(b.Id, b.Name)
    //                 }
    //             };
    //         })
    //         .ToListAsync();
    //
    //     return brandModels;
    // }

   

    private async Task PrepareProductAttributeModelAsync(ProductDetailsModelContext context, ProductDetailModel model)
    {
        var product = context.Product;
        var batchContext = context.BatchContext;
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
                IsActive = attribute.IsActive,
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType,
            };


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
                            IsEssential = value.IsEssential,
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
                Console.WriteLine();
            }

            foreach (var value in attributeVm.Values.Where(x => x.IsPreSelected))
            {
                // When this runs the first time, we've got to select attributes that are pre-selected cuz none are selected by the user yet.
                // Query will contain both the user-selected values and the values that are 'IsPreSelected'. 
                // The user-chosen values will override the 'IsPreSelected' ones since they appear before
                // the pre-selected ones on the list and since we retrieve the values by calling FirstOrDefault(), we'll get the first added-value (a user-selected one if exists)
                query.AddVariant(new ProductVariantQueryItem()
                {
                    Value = value.Id.ToString(),
                    ProductId = product.Id,
                    AttributeId = attribute.ProductAttributeId,
                    VariantAttributeId = attribute.Id
                });
            }

            model.ProductVariantAttributes.Add(attributeVm);
        }

        if (query.Variants.Count > 0)
        {
            await PrepareProductAttributeCombinationsModelAsync(context, model);
        }
        
    }

    private async Task PrepareProductAttributeCombinationsModelAsync(ProductDetailsModelContext context,
        ProductDetailModel model)
    {
        var product = context.Product;
        var batchContext = context.BatchContext;
        var query = context.ProductVariantQuery;
        var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);

        //selection (attribute + selected value) of all the attributes displayed. 
        var selection = _productAttributeMaterializer.CreateAttributeSelection(query, attributes, product.Id);
        context.Selection = selection;
        var selectedValues =
            _productAttributeMaterializer.MaterializeProductVariantAttributeValues(selection, attributes);
        var selectedValueIds = selectedValues
            .Select(x => x.Id)
            .ToArray();

        var selectedCombination = model.SelectedCombination =
            await _productAttributeMaterializer.FindAttributeCombinationAsync(product.Id, selection);

        if ((model.SelectedCombination == null || (!model.SelectedCombination.IsActive ||
                                                   model.SelectedCombination.StockQuantity == 0)) 
            && product.AttributeCombinationRequired)
        {
            model.IsAvailable = false;
        }
        else
        {
            model.IsAvailable = true;
        }

        int stockQuantity = selectedCombination?.StockQuantity ?? product.StockQuantity;
        model.MaxAddToCartNumber =
            (stockQuantity != 0 &&
             stockQuantity > product.MaxAddToCartNumber)
                ? product.MaxAddToCartNumber
                : stockQuantity;
        model.MinAddToCartNumber = product.MinAddToCartNumber;
        model.MinAddToCartNumber = Math.Min(model.MinAddToCartNumber, model.MaxAddToCartNumber);

        product.MergeDataWithCombination(model.SelectedCombination);


        //TODO: if none is active or even if one is not active, how to we convey that to the user? "foreach (var attribute in model.ProductVariantAttributes.Where(x => x.IsActive))"
        foreach (var attribute in model.ProductVariantAttributes.Where(x => x.IsActive))
        {
            //any value of the attribute intersects with any user chosen value for this attribute.
            // In other words, has the user selected a particular value for this attribute? 
            var updatePreselection = selectedValueIds.Length > 0 && selectedValueIds
                .Intersect(attribute.Values.Select(x => x.Id))
                .Any();


            foreach (ProductVariantAttributeValueModel value in
                     attribute.Values.Cast<ProductVariantAttributeValueModel>())
            {
                var isSelected = selectedValueIds.Contains(value.Id);
                if (updatePreselection)
                {
                    //set to false or true depending on which value the user has chosen. 
                    value.IsPreSelected = isSelected;
                }

                if (isSelected)
                {
                    model.Weight += value.ProductVariantAttributeValue.WeightAdjustment;
                }

                var availabilityInfo = await _productAttributeMaterializer.IsCombinationAvailableAsync(
                    product,
                    attributes,
                    selectedValues,
                    value.ProductVariantAttributeValue);
                if (availabilityInfo != null)
                {
                    
                    value.IsUnavailable = true;
                    if (availabilityInfo.IsOutOfStock && availabilityInfo.IsActive)
                    {
                        value.Title = "Out of Stock";
                    }
                    else
                    {
                        value.Title = "Not Available";
                    }
                }
            }
        }
    }


    private async Task PrepareProductPropertiesModelAsync(ProductDetailsModelContext context, ProductDetailModel model)
    {
        //TODO: Start using Meta properties and display dimension properties (including making sure dimension value like Length, Weight, Width etc are in sync with combination values e.g. Weight += comb.Weight).
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;
        var combination = model.SelectedCombination;

        model.Id = product.Id;
        model.Name = product.Name;
        model.MetaTitle = product.MetaTitle;
        model.MetaDescriptions = product.MetaTitle;
        model.Description = product.Description;
        model.ShortDescription = product.ShortDescription;
        // Don't map this in here because it'll override the set value base off the combination availability.
        //model.IsAvailable = product.IsAvailable;
        model.StockQuantity = product.StockQuantity;
        model.RatingAverage = product.ApprovedRatingSum;
        model.ProductReviewOverview = new ProductReviewOverviewModel()
        {
            TotalReviews = product.ApprovedReviewCount,
            RatingSum = product.ApprovedRatingSum,
        };
        model.Weight = product.Width > 0 ? $"{product.Width:G29}" : string.Empty;
        model.Height = product.Height > 0 ? $"{product.Height:G29}" : string.Empty;
        model.Length = product.Length > 0 ? $"{product.Length:G29}" : string.Empty;
        model.Width = product.Width > 0 ? $"{product.Width:G29}" : string.Empty;


        // Stock info
        if (product.DisplayStockQuantity)
        {
            if (product.StockQuantity > 0)
            {
                model.StockAvailability =
                    string.Format(CatalogMessages.Product.StockAvailability, product.StockQuantity);
                
            }
            else
            {
                model.StockAvailability = CatalogMessages.Product.OutOfStock;
            }
        }

        // TODO: Display delivery times.
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


    private async Task PrepareRelatedProductModelAsync(ProductDetailsModelContext context, ProductDetailModel model)
    {
        // ArgumentNullException.ThrowIfNull(model);
        // ArgumentNullException.ThrowIfNull(context);
        // var product = context.Product;
        // var batchContext = context.LazyContext;
        // var relatedProducts = await batchContext.RelatedProducts.GetOrLoadAsync(product.Id);
        //
        // foreach (ProductLink productLink in relatedProducts)
        // {
        //     var relProduct = productLink.LinkedProduct;
        //     var relProductVm = ProductThumbnail.FromProduct(relProduct);
        //     //relProductVm.ThumbnailUrl = _mediaService.GetMediaUrl(relProduct.ThumbnailImage);
        //     relProductVm.CalculatedProductPrice = _productPricingService.CalculateProductPrice(relProduct);
        //     model.RelatedProducts.Add(relProductVm);
        // }
    }

    private async Task PrepareProductSpecificationModelAsync(ProductDetailsModelContext context,
        ProductDetailModel model)
    {
        var cacheKey = string.Format(ModelCacheInvalidator.ProductSpecsModelKey, context.Product.Id);
        var specs = await _cacheFactory
            .GetMemoryCache()
            .GetOrCreateAsync(cacheKey,
                async () =>
                {
                    var product = context.Product;
                    return await _db
                        .ProductSpecificationAttributes
                        .AsNoTracking()
                        .Where(x => x.ProductId == product.Id)
                        .Include(x => x.SpecificationAttributeOption)
                        .ThenInclude(x => x.SpecificationAttribute)
                        .OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.DisplayOrder)
                        .ThenBy(x => x.SpecificationAttributeOption.SpecificationAttribute.Name)
                        .Select(x => new ProductSpecificationModel()
                        {
                            SpecificationAttributeId = x.SpecificationAttributeOption.SpecificationAttributeId,
                            SpecificationAttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                            SpecificationAttributeOption = x.SpecificationAttributeOption.Name,
                            DisplayOrder = x.SpecificationAttributeOption.DisplayOrder,
                        })
                        .ToListAsync();
                });
        model.ProductSpecifications = specs;
    }
}