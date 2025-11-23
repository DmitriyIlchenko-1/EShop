using EShop.Core.Catalog.Attributes.Domain;
using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Common.Services;
using EShop.Core.Data;
using EShop.Web.Common.Models.Choices;
using EShop.Web.Models.Catalog;
using Microsoft.EntityFrameworkCore;

namespace EShop.Web.Controllers;

public partial class CatalogHelper
{
    private readonly IMediaService _mediaService;
    private readonly IProductService _productService;
    private readonly IProductPricingService _productPricingService;
    private readonly IProductAttributeMaterializer _productAttributeMaterializer;
    private readonly IDeliveryTimeService _deliveryTimeService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ApplicationDbContext _db;

    public CatalogHelper(IMediaService mediaService, IProductPricingService productPricingService,
        IProductService productService, IProductAttributeMaterializer productAttributeMaterializer,
        IDeliveryTimeService deliveryTimeService, ApplicationDbContext db, IDateTimeService dateTimeService)
    {
        _mediaService = mediaService;
        _productPricingService = productPricingService;
        _productService = productService;
        _productAttributeMaterializer = productAttributeMaterializer;
        _deliveryTimeService = deliveryTimeService;
        _db = db;
        _dateTimeService = dateTimeService;
    }

    public ProductDetailsModelContext CreateModelContext(Product product, ProductVariantQuery query)
    {
        return new ProductDetailsModelContext(
            product,
            query,
            _productService.CreateProductBatchContext(new[] { product }));
    }


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
        model.WeightValue = product.Weight;
        await PrepareProductPropertiesModelAsync(context, model);
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
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;
        var batchContext = context.LazyContext;
        var query = context.ProductVariantQuery;
        var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);

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
                        };

                        return valueVm;
                    })
                    .ToList();

                attributeVm.Values = attrValueVms
                    .Select(x => (ChoiceItemModel)x)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();
            }

            model.ProductVariantAttributes.Add(attributeVm);
        }

        if (query != null && (query.Variants.Count > 0))
        {
            await PrepareProductAttributeCombinationsModelAsync(context, model);
        }
    }


    private async Task PrepareProductAttributeCombinationsModelAsync(ProductDetailsModelContext context,
        ProductDetailVm model)
    {
        ArgumentNullException.ThrowIfNull(context);
        var product = context.Product;
        var batchContext = context.LazyContext;
        var query = context.ProductVariantQuery;
        var attributes = await batchContext.Attributes.GetOrLoadAsync(product.Id);

        var selection = _productAttributeMaterializer.CreateAttributeSelectionAsync(query, attributes, product.Id);
        context.SelectedAttributes = selection;
        var selectedValues =
            _productAttributeMaterializer.MaterializeProductVariantAttributeValues(selection, attributes);
        var selectedValueIds = selectedValues
            .Select(x => x.Id)
            .ToArray();

        model.SelectedCombination =
            await _productAttributeMaterializer.FindProductVariantAttributeCombinationAsync(product.Id,
                context.SelectedAttributes);

        if (model.SelectedCombination != null && !model.SelectedCombination.IsActive)
        {
            model.IsAllowToOrder = false;
        }

        //TODO: Come up with an idea about how to merge the data between the product and the selection. 
        // MergeWithCombination();

        foreach (var attribute in model.ProductVariantAttributes.Where(x => x.IsActive))
        {
            var updatePreselection = selectedValueIds.Length > 0 && selectedValueIds
                .Intersect(attribute.Values.Select(x => x.Id))
                .Any();


            foreach (ProductVariantAttributeValueModel value in
                     attribute.Values.Cast<ProductVariantAttributeValueModel>())
            {
                var isSelected = selectedValueIds.Contains(value.Id);
                if (updatePreselection)
                {
                    value.IsPreSelected = isSelected;
                }

                if (isSelected)
                {
                    model.WidthValue += value.ProductVariantAttributeValue.WeightAdjustment;
                }
            }
        }
    }

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
        model.IsAllowToOrder = product.IsAllowToOrder;
        model.StockQuantity = product.StockQuantity;
        model.RatingAverage = product.RatingAverage;
        model.ReviewsCount = product.ReviewsCount;
        model.WeightValue = product.Weight;


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

        model.HeightValue = combination is { Height: not null } ? combination.Height.Value : product.Height;
        model.LengthValue = combination is { Length: not null } ? combination.Length.Value : product.Length;
        model.WidthValue = combination is { Width: not null } ? combination.Width.Value : product.Width;

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
            relProductVm.ThumbnailUrl = _mediaService.GetMediaUrl(relProduct.ThumbnailImage);
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