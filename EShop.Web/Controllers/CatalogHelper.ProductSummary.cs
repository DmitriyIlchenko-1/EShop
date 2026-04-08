using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Brands.Domain;
using EShop.Core.Catalog.Categories.Domain;
using EShop.Core.Catalog.Products;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Content.Media.Domain;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using EShop.Web.Models.Catalog;

namespace EShop.Web.Controllers;

public partial class CatalogHelper
{
    public virtual async Task<ProductSummaryModel> PrepareProductSummaryModelAsync(IList<Product> products,
        ProductSummaryMappingSettings mappingSettings, ProductVariantQuery? variantQuery)
    {
        Guard.NotNull(products);
        if (!(products.Count > 0))
            return ProductSummaryModel.Empty;
        mappingSettings ??= new ProductSummaryMappingSettings();

        var model = new ProductSummaryModel();
        var lazyProductCtx = _productService.CreateProductBatchContext(products);
        var itemCtx = new ProductSummaryItemContext
        {
            LazyProductContext = lazyProductCtx,
            MappingSettings = mappingSettings,
        };

        if (mappingSettings.MapVariants && mappingSettings.VariantCountInLists > 0)
        {
            await lazyProductCtx.Attributes.LoadAllAsync();
            var selections = new Dictionary<int, ProductVariantAttributeSelection>();
            foreach (var product in products)
            {
                var attributes = await lazyProductCtx.Attributes.GetOrLoadAsync(product.Id);

                //selection (attribute + selected value) of all the attributes displayed. 
                var sltAttrs =
                    _productAttributeMaterializer.CreateAttributeSelectionAsync(variantQuery, attributes, product.Id);
                selections.Add(product.Id, sltAttrs);
                itemCtx.ProductSelectedAttributes.Add(product.Id, sltAttrs);
            }

            itemCtx.ProductSelectedCombinations =
                await _productAttributeMaterializer.FindProductVariantAttributeCombinationsAsync(selections);
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


    private async Task PrepareProductSummaryAttributeCombinationModelAsync((Product Product, ProductLazyContext LazyCtx,
        ICollection<ProductVariantAttributeModel> ProductVariantAttributes, ProductVariantAttributeSelection
        SelectedCombination) ctx)
    {
        var product = ctx.Product;
        var batchContext = ctx.LazyCtx;
        var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);

        var selectedValues =
            _productAttributeMaterializer.MaterializeProductVariantAttributeValues(ctx.SelectedCombination,
                attributes);
        var selectedValueIds = selectedValues
            .Select(x => x.Id)
            .ToArray();


        //more out to a mapper
        // if (model.SelectedCombination != null && !model.SelectedCombination.IsActive && model.SelectedCombination.StockQuantity == 0)
        // {
        //     model.IsAvailable = false;
        // }

        //TODO: Come up with an idea about how to merge the data between the product and the selection. 
        // MergeWithCombination();

        foreach (var attribute in ctx.ProductVariantAttributes.Where(x => x.IsActive))
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
                    // model.Weight += value.ProductVariantAttributeValue.WeightAdjustment;
                }
            }
        }
    }


    private async Task<ProductSummaryItemModel> MapProductSummaryItem(Product product, ProductSummaryItemContext ctx)
    {
        var lazyContext = ctx.LazyProductContext;
        var settings = ctx.MappingSettings;
        var item = new ProductSummaryItemModel
        {
            Id = product.Id,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            SeName = await _urlService.GetActiveSlugAsync(product.Id, product.Name),
            Sku = product.Sku,
        };


        if (settings.MapShortDescription)
        {
            item.ShortDescription = product.ShortDescription;
        }

        if (settings.MapPrices)
        {
            var price = _productPricingService.CalculateProductPrice(product);
            MapPriceModel(price, item.PriceModel);
        }

        if (settings.MapPictures)
        {
            item.Images = await PrepareProductSummaryImageModelAsync(product);
        }

        if (settings.MapVariants && settings.VariantCountInLists > 0)
        {
            item.Selection = ctx.ProductSelectedAttributes
                .FirstOrDefault(x => x.Key == product.Id)
                .Value;
            await PrepareProductSummaryAttributeCombinationModelAsync((product, lazyContext, item.Variants,
                item.Selection));
        }

        if (settings.MapBrands)
        {
            item.Brand = (await MapBrandSummaryModelAsync(await lazyContext.ProductBrands.GetOrLoadAsync(product.Id)))
                .FirstOrDefault();
        }


        return item;
    }

    private async Task MapProductSummaryModelCore(ProductSummaryModel model, ProductSummaryItemContext ctx)
    {
        var items = model.Items;
        foreach (var item in items)
        {
            
        }
    }


    public async Task<List<BrandSummaryModel>> MapBrandSummaryModelAsync(IEnumerable<ProductBrand> productBrands,
        bool withPictures = false)
    {
        var model = new List<BrandSummaryModel>();
        if (withPictures)
        {
            //TODO
            await Task.CompletedTask;
        }

        foreach (var pb in productBrands)
        {
            var brand = pb.Brand;
            var item = new BrandSummaryModel()
            {
                Id = brand.Id,
                Name = brand.Name,
            };
            model.Add(item);
        }

        return model;
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
                999999,
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

    public virtual ProductSummaryMappingSettings GetProductSummaryMappingSettings(
        Action<ProductSummaryMappingSettings> conf = null)
    {
        var settings = new ProductSummaryMappingSettings()
        {
            MapPictures = true,
            MapPrices = true
        };
        settings.MapShortDescription = _catalogSettings.ShowDescriptionProductList;
        settings.MapAttributes = _catalogSettings.ShowVariantsProductList;
        settings.MapReviews = _catalogSettings.ShowReviewsProductList;
        settings.MapDimensions = _catalogSettings.ShowDescriptionProductList;
        settings.MapBrands = _catalogSettings.ShowBrandProductList;
        settings.MapVariants = _catalogSettings.ShowVariantsProductList;

        conf?.Invoke(settings);
        return settings;
    }

    public class ProductSummaryMappingSettings
    {
        public bool MapPrices { get; set; }
        public bool MapPictures { get; set; }
        public bool MapDimensions { get; set; }
        public bool MapVariants { get; set; }

        public bool MapSpecificationAttributes { get; set; }
        public int VariantCountInLists { get; set; }
        public bool MapAttributes { get; set; }
        public bool MapBrands { get; set; }
        public bool MapShortDescription { get; set; }
        public bool MapReviews { get; set; }
    }
}