using EShop.Core.Catalog.Products.Domain;
using EShop.Core.Catalog.Products.Services;
using EShop.Core.Common.Domain;
using EShop.Core.Common.Services;
using EShop.Core.Data;

namespace EShop.Core.Catalog.Products.Price;

public class DiscountPriceCalculator : IPriceCalculator
{
    private readonly IDiscountService _discountService;
    private readonly ILabelManager _labelManager;
    private readonly ApplicationDbContext _db;

    public DiscountPriceCalculator(IDiscountService discountService, ILabelManager labelManager, ApplicationDbContext db)
    {
        _discountService = discountService;
        _labelManager = labelManager;
        _db = db;
    }

    public int Order { get; }

    public async Task CalculateAsync(CalculatorPriceContext context, CalculatorDelegate next)
    {
        if (!context.Options.ApplyDiscounts)
        {
            await next(context);
            return;
        }

        var priceCtx = context.CalculatedProductPrice;

        var (discountAmount, appliedDiscount) = await ApplyDiscountAsync(context.CalculatedProductPrice.FinalPrice, context);
        if (appliedDiscount != null)
        {
            priceCtx.FinalPrice -= discountAmount;
            priceCtx.AppliedDiscount = appliedDiscount;
            priceCtx.DiscountAmount = discountAmount;
        }

        await next(context);
    }

    protected virtual async Task<(decimal DiscountAmount, Discount AppliedDiscount)> ApplyDiscountAsync(decimal price, CalculatorPriceContext ctx)
    {
        decimal discountAmount = 0;
        Discount bestCandidateDiscount = null;
        var applicableDiscounts = await FindApplicableDiscounts(ctx);
        if (applicableDiscounts.Any())
        {
            bestCandidateDiscount = applicableDiscounts.GetBestCandidateDiscount(price);
            if (bestCandidateDiscount != null)
            {
                discountAmount = bestCandidateDiscount.GetDiscountAmount(price);
            }
        }

        return (discountAmount, bestCandidateDiscount);

    }

    protected virtual async Task<ICollection<Discount>> FindApplicableDiscounts(CalculatorPriceContext ctx)
    {
        var batchContext = ctx.BatchContext;
        var product = ctx.Product;
        HashSet<Discount> result = new HashSet<Discount>();
       
        if (product.HasDiscountsApplied)
        {
            var discountsLoaded = _db
                .Entry(product)
                .Collection(x => x.AppliedDiscounts)
                .IsLoaded;
            var appliedDiscounts = discountsLoaded
                ? product.AppliedDiscounts
                : await batchContext.ProductDiscounts.GetOrLoadAsync(product.Id);
            
            if (appliedDiscounts != null && appliedDiscounts.Any())
            {
                await ValidateAddDiscounts(appliedDiscounts, result, DiscountType.ProductDiscount, ctx);
            }
             
        }

        var categoryDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.CategoryDiscount);
        if (categoryDiscounts.Any())
        {
            //TODO: should I do the same check with Entry.Collection first before calling GetOrLoadAsync()? 
            var productCategories = await batchContext.ProductCategories.GetOrLoadAsync(product.Id);
            foreach (var pc in productCategories)
            {
                var category = pc.Category;
                if (category.HasDiscountsApplied)
                {
                    var loaded = _db
                        .Entry(category)
                        .Collection(x => x.AppliedDiscounts)
                        .IsLoaded;
                    var appliedDiscounts = loaded
                        ? category.AppliedDiscounts
                        : categoryDiscounts.Where(x => x.AppliedToCategories.Any(x => x.Id == category.Id));
                     await ValidateAddDiscounts(appliedDiscounts, result, DiscountType.CategoryDiscount, ctx);
                }
            }
        }

        return result;

    }


    protected virtual async Task ValidateAddDiscounts(IEnumerable<Discount> source, HashSet<Discount> resultSet, DiscountType discountType, CalculatorPriceContext ctx)
    {
        var set = resultSet;
        foreach (var discount in source)
        {
            if (discount.DiscountType == discountType && !set.Contains(discount)
                && await _discountService.IsDiscountValidAsync(discount, ctx.User))
            {
                set.Add(discount);
            }
        }
    }
    
}