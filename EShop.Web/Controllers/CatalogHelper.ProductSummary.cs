using System.Globalization;
using System.Security.Policy;
using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Extensions;
using EShop.Core.Catalog.Products.Price;
using EShop.Core.Common.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using EShop.Web.Common.Models.Choices;
using EShop.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EShop.Web.Controllers;

public partial class CatalogHelper
{
    public virtual async Task<ProductSummaryModel> PrepareProductSummaryModelAsync(IList<Product> products,
        ProductVariantQuery? variantQuery, ProductSummaryMappingSettings mappingSettings)
    {
        Guard.NotNull(products);
        if (!(products.Count > 0))
            return ProductSummaryModel.Empty;


        mappingSettings ??= new ProductSummaryMappingSettings();
        var model = new ProductSummaryModel()
        {
            ShowBrand = mappingSettings.MapBrands,
            ShowRating = mappingSettings.MapReviews
        };
        var lazyProductCtx = _productService.CreateProductBatchContext(products);
        var itemCtx = new ProductSummaryItemContext(model)
        {
            BatchProductContext = lazyProductCtx,
            MappingSettings = mappingSettings,
            ProductVariantQuery = variantQuery,
        };

        //TODO: continue await lazyProductCtx.ProductLabels.LoadAllAsync();

        var prefetchSlugs = mappingSettings.PrefetchUrlSlugs.HasValue
            ? mappingSettings.PrefetchUrlSlugs.Value
            : _performanceSettings.AlwaysPrefetchUrlSlugs;

        int[] productIds = prefetchSlugs
            ? products
                .Select(p => p.Id)
                .ToArray()
            : Array.Empty<int>();

        if (prefetchSlugs)
        {
            await _urlService.PrefetchUrlRecordsAsync(nameof(Product), productIds, false);
        }

        if (mappingSettings.MapColorAttributes)
        {
            await lazyProductCtx.Attributes.LoadAllAsync();
        }

        if (mappingSettings.MapPictures)
        {
            await lazyProductCtx.ProductMedia.LoadAllAsync();
        }

        if (mappingSettings.MapBrands)
        {
            await lazyProductCtx.ProductBrands.LoadAllAsync();
        }


        foreach (var product in products)
        {
            model.Items.Add(await MapProductSummaryItem(product, itemCtx));
        }

        lazyProductCtx.Clear();
        return model;
    }

    private async Task<ProductSummaryItemModel> MapProductSummaryItem(Product product, ProductSummaryItemContext ctx)
    {
        var batchContext = ctx.BatchProductContext;
        var settings = ctx.MappingSettings;
        var seName = await _urlService.GetActiveSlugAsync(product.Id, product.GetEntityName());
        var item = new ProductSummaryItemModel(ctx.Model)
        {
            Id = product.Id,
            Name = product.Name,
            SeName = seName,
            DetailUrl = _urlHelper.RouteUrl("Product", new { SeName = seName })
        };


        // var labels = await lazyContext.ProductLabels.GetOrLoadAsync(product.Id);
        // foreach (var productLabel in labels)
        // {
        //     var label = productLabel.Label;
        //     var labelModel = new ProductLabelModel()
        //     {
        //         Name = label.Name,
        //         Color = label.Color,
        //         SvgName = label.SvgPath
        //     };
        //     
        //     item.ProductLabels.Add(labelModel);
        // }

        //ready
        if (settings.MapShortDescription)
        {
            item.ShortDescription = product.ShortDescription;
        }

        //ready
        if (settings.MapColorAttributes)
        {
            var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);
            if (attributes.Any())
            {
                var colorAttributes = attributes
                    .Where(x => x.IsListTypeAttribute())
                    .SelectMany(x => x.ProductVariantAttributeValues)
                    .Where(x => x.Color.HasValue() &&
                                x.Color.Equals("transparent", StringComparison.OrdinalIgnoreCase))
                    .Take(20)
                    .Select(x => new ProductSummaryItemModel.ColorAttributeValue
                    {
                        Id = x.Id,
                        AttributeId = x.ProductVariantAttributeId,
                        Color = x.Color,
                        Name = x.Name
                    })
                    .ToList();
                item.ColorAttributeValues = colorAttributes;
            }
        }

        if (settings.MapPrices)
        {
            await MapPriceModel(product, item, ctx);
        }


        if (settings.MapBrands)
        {
            var productBrand =
                (await ctx.BatchProductContext.ProductBrands.GetOrLoadAsync(product.Id)).FirstOrDefault();
            if (productBrand != null)
            {
                item.Brand = await PrepareBrandSummaryModelAsync(productBrand.Brand);
            }
        }

        if (settings.MapPictures)
        {
            var productMedia = await batchContext.ProductMedia.GetOrLoadAsync(product.Id);
            item.Images = await PrepareProductImageModelAsync(productMedia);
        }


        if (settings.MapDimensions && (product.Width != 0 || product.Height != 0 || product.Length != 0))
        {
            item.Dimensions = string.Format(CultureInfo.InvariantCulture,
                CatalogMessages.Product.DimensionValues,
                product.Width.ToString("G29"),
                product.Height.ToString("G29"),
                product.Length.ToString("G29"));
        }

        item.DeliveryTimeModel = await PrepareDeliveryTimeModelAsync(product, settings);

        if (product.IsNew(_catalogSettings))
        {
            item.ProductLabels.Add(new ProductLabelModel()
            {
                Name = SystemLabelNames.NewArrival,
                Content = SystemLabelNames.NewArrivalTemplate
            });
        }

        item.Sku = product.Sku;
        item.IsAvailable = product.IsAvailable;
        item.TotalReviews = product.ApprovedReviewCount;
        item.RatingSum = product.ApprovedRatingSum;
        item.ShortDescription = product.ShortDescription;


        return item;
    }

    private async Task<DeliveryTimeModel> PrepareDeliveryTimeModelAsync(Product product,
        ProductSummaryMappingSettings settings)
    {
        var model = new DeliveryTimeModel
        {
            Id = product.DeliveryTimeId ?? 0,
            ShowDeliveryTime = settings.ShowDeliveryTime,
        };

        if (model.ShowDeliveryTime)
        {
            var deliveryTime = await _deliveryTimeService.GetDeliveryTimeAsync(model.Id);
            if (deliveryTime != null)
            {
                model.DeliveryTimeName = deliveryTime.Name;
                model.DeliveryTimeDate = _deliveryTimeService.GetFormattedDeliveryDate(deliveryTime);
            }

            model.StatusLabel = model.DeliveryTimeName;
            if (model.StatusLabel.IsEmpty())
            {
                model.ShowDeliveryTime = false;
            }
        }

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


        return model;
    }

    //not working at the moment cuz in the middle of refactoring
    // private async Task MapProductSummaryAttributesAsync(Product product, ProductSummaryItemModel model,
    //     ProductSummaryItemContext ctx)
    // {
    //     var batchContext = ctx.LazyProductContext;
    //     var query = ctx.ProductVariantQuery;
    //     var attributes = await batchContext.EssentialAttributes.GetOrLoadAsync(model.Id);
    //     var weightAdjustment = 0m;
    //     var valueCountDict = await _productAttributeMaterializer.GetEssentialVariantAttributeValueCountsAsync(false);
    //     foreach (var attribute in attributes)
    //     {
    //         var attributeVm = new ProductVariantAttributeModel()
    //         {
    //             Id = attribute.Id,
    //             ProductId = attribute.ProductId,
    //             ProductAttributeId = attribute.ProductAttributeId,
    //             ProductVariantAttribute = attribute,
    //             Alias = attribute.ProductAttribute.Alias,
    //             Name = attribute.ProductAttribute.Name,
    //             Description = attribute.ProductAttribute.Description,
    //             TextPrompt = attribute.ProductAttribute.TextPrompt,
    //             IsRequired = attribute.IsRequired,
    //             AttributeControlType = attribute.AttributeControlType,
    //             IsActive = attribute.IsActive,
    //             TotalAttributeCount = valueCountDict.Get(attribute.Id)
    //         };
    //         if (query.Variants.Count != 0)
    //         {
    //             // we work on the 'variant' attribute level in here, not just attributes.
    //             var selectedAttribute = query.Variants.FirstOrDefault(x =>
    //                 x.AttributeId == attribute.Id
    //                 && x.ProductId == attribute.ProductId
    //                 && x.VariantAttributeId == attribute.Id
    //                 && x.AttributeId == attribute.ProductAttributeId);
    //
    //             if (selectedAttribute != null)
    //             {
    //             }
    //         }
    //
    //         if (attribute.IsListTypeAttribute())
    //         {
    //             List<ProductVariantAttributeValueModel> attrValueVms = attribute
    //                 .ProductVariantAttributeValues
    //                 .Select(value =>
    //                 {
    //                     var valueVm = new ProductVariantAttributeValueModel()
    //                     {
    //                         Id = value.Id,
    //                         ProductVariantAttributeValue = value,
    //                         PriceAdjustment = string.Empty,
    //                         Name = value.Name,
    //                         Alias = value.Alias,
    //                         Color = value.Color,
    //                         IsPreSelected = value.IsPreSelected,
    //                         DisplayOrder = value.DisplayOrder,
    //                         QuantityInfo = value.Quantity,
    //                         IsEssential = value.IsEssential
    //                     };
    //
    //                     // If the attribute value is pre-selected, it means that it's the default value chosen even if the user hasn't selected any value for this attribute.
    //                     // That means we have to find the preselected values, which is what we do in here, and then we call AddVariant() for the so-called default aka preselected attribute values,
    //                     // so that later, if all the preselected attribute values are part of a specific attribute selection, we can then retrieve the combination by using this selection made of all the preselected values.
    //                     // If you don't try to determine all the preselected values, then it'd be impossible on the first load to determine if any product variant combination matches the default selected attribute values,
    //                     // so that we can adjust price of the product and other properties accordingly.
    //                     if (value.IsPreSelected)
    //                     {
    //                         weightAdjustment += value.WeightAdjustment;
    //                     }
    //
    //                     return valueVm;
    //                 })
    //                 .OrderBy(x => x.DisplayOrder)
    //                 .ThenBy(x => x.Name)
    //                 .ToList();
    //
    //             attributeVm.Values = attrValueVms
    //                 .Select(x => (ChoiceItemModel)x)
    //                 .ToList();
    //         }
    //
    //         // model.ProductVariantAttributes.Add(attributeVm);
    //     }
    //
    //     if (query != null && (query.Variants.Count > 0))
    //     {
    //         // await PrepareProductSummaryAttributeCombinationModelAsync(product, model, ctx);
    //     }
    //     else
    //     {
    //         model.Weight += weightAdjustment;
    //     }
    // }


    // private async Task PrepareProductSummaryAttributeCombinationModelAsync(Product product, ProductCombinationMap model,
    //     ProductSummaryItemContext ctx)
    // {
    //     var batchContext = ctx.LazyProductContext;
    //     var parent = ctx.Model;
    //     var showAvailabilityInfo = product.CombinationDisplayBehaviour ==
    //                                CombinationDisplayBehaviour.HighlightUnavailableWithGrey;
    //     var attributes = await batchContext.EssentialAttributes.GetOrLoadAsync(product.Id);
    //     ctx.ProductAttributeSelections.TryGetValue(product.Id, out var selection);
    //     _productAttributeMaterializer.TryGetPrefetchedCombination(product.Id, selection, out var combination);
    //
    //     var selectedValues =
    //         _productAttributeMaterializer.MaterializeProductVariantAttributeValues(selection,
    //             attributes);
    //     var selectedValueIds = selectedValues
    //         .Select(x => x.Id)
    //         .ToArray();
    //     model.SelectedCombination = combination;
    //
    //     if (combination != null && !combination.IsActive)
    //     {
    //         model.AdjustForCombinationActive(isActive: false);
    //     }
    //
    //     product.MergeDataWithCombination(combination);
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
    //                 model.AdjustForAttributeValue(value);
    //             }
    //
    //             if (showAvailabilityInfo)
    //             {
    //                 var availabilityInfo = await _productAttributeMaterializer.IsCombinationAvailableAsync(
    //                     product,
    //                     attributes,
    //                     selectedValues,
    //                     value.ProductVariantAttributeValue);
    //                 if (availabilityInfo != null)
    //                 {
    //                     value.IsUnavailable = true;
    //                     if (availabilityInfo.IsOutOfStock && availabilityInfo.IsActive)
    //                     {
    //                         value.Title = "Out of Stock";
    //                     }
    //                     else
    //                     {
    //                         value.Title = "Not Available";
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }


    public async Task<BrandSummaryModel> PrepareBrandSummaryModelAsync(Brand productBrand,
        bool withPictures = false)
    {
        var model = new BrandSummaryModel();
        if (withPictures)
        {
            //TODO
            await Task.CompletedTask;
        }

        model.Id = productBrand.Id;
        model.Name = productBrand.Name;
        model.SeName = await _urlService.GetActiveSlugAsync(productBrand.Id, productBrand.GetEntityName());

        return model;
    }

    protected async Task MapPriceModel(Product product, ProductSummaryItemModel item, ProductSummaryItemContext ctx)
    {
        var calculatedPrice = await _productPriceService.CalculatePriceAsync(new PriceCalculationContext()
        {
            Product = product,
            BatchContext = ctx.BatchProductContext
        });
        var priceModel = item.PriceModel;
        var labelModel = item.ProductLabels;
        priceModel.FinalPrice = calculatedPrice.FinalPrice;
        priceModel.RegularPrice = calculatedPrice.PriceSaving.SavingPrice;
        priceModel.Saving = calculatedPrice.PriceSaving;
        if (calculatedPrice.PriceSaving.HasSaving)
        {
            labelModel.Add(new ProductLabelModel()
            {
                Name = SystemLabelNames.Sale,
                Content = string.Format(CultureInfo.InvariantCulture,
                    SystemLabelNames.SaleTemplate,
                    calculatedPrice.PriceSaving.SavingPercent.ToString("N0"))
            });
        }
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


        return await categories
            .SelectAsync(async category =>
            {
                allImages.TryGetValue(category.MediaFileId ?? 0, out var image);
                var model = new CategorySummaryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    // Url = await _urlService.GetActiveSlugAsync(category.Id, category.Name),
                    Image = new ImageModel
                    {
                        // Url = await _mediaService.GetMediaUrlAsync(image),
                        Alt = image?.Alt,
                        Width = image.Width,
                        Height = image.Height,
                    }
                };

                return model;
            })
            .ToListAsync();
    }

    public virtual async Task<IList<ImageModel>> PrepareProductImageModelAsync(
        IEnumerable<ProductMedia> mediaFiles)
    {
        if (!mediaFiles.Any())
        {
            return [];
        }
        //TODO: add caching.


        async Task<ImageModel> MapToMediaModelAsync(ProductMedia productMedia)
        {
            var mediaFile = productMedia.MediaFile;
            string url = await _mediaService.GetMediaUrlAsync(mediaFile);
            return new ImageModel()
            {
                Id = mediaFile.Id,
                FileName = mediaFile.FileName,
                MimeType = mediaFile.MimeType,
                MediaType = mediaFile.MediaType,
                Size = mediaFile.Size,
                Alt = mediaFile.Alt,
                Width = mediaFile.Width,
                Height = mediaFile.Height,
                Url = url,
                Subpath = url
            };
        }

        var imageModels = await mediaFiles
            .SelectAsync(async image => await MapToMediaModelAsync(image))
            .ToListAsync();
        return imageModels;
    }


    public virtual ProductSummaryMappingSettings GetProductSummaryMappingSettings(
        Action<ProductSummaryMappingSettings> conf = null)
    {
        var settings = new ProductSummaryMappingSettings()
        {
            MapPictures = true,
            MapPrices = true,
            MapBrands = true
        };
        settings.MapShortDescription = _catalogSettings.ShowDescriptionProductList;
        settings.MapReviews = _catalogSettings.ShowReviewsProductList;
        settings.MapDimensions = _catalogSettings.ShowDimensionsInLists;
        settings.MapColorAttributes = _catalogSettings.ShowColorAttributesInLists;

        conf?.Invoke(settings);
        return settings;
    }

    public class ProductSummaryMappingSettings
    {
        public bool MapPrices { get; set; }
        public bool MapPictures { get; set; }
        public bool MapDimensions { get; set; }
        public bool MapColorAttributes { get; set; }

        public bool MapSpecificationAttributes { get; set; }
        public bool MapBrands { get; set; }

        public bool MapShortDescription { get; set; }
        public bool MapReviews { get; set; }
        public bool? PrefetchUrlSlugs { get; set; }
        public bool ShowDeliveryTime { get; set; }
    }
}