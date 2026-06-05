using EShop.Core.Catalog.Attributes.Services;
using EShop.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Catalog.Products.Price;

public class CombinationPriceCalculator : IPriceCalculator
{
    private readonly ApplicationDbContext _db;

    public CombinationPriceCalculator(ApplicationDbContext db)
    {
        _db = db;
    }

    public int Order { get; }

    public async Task CalculateAsync(CalculatorPriceContext context, CalculatorDelegate next)
    {
        // var product = context.Product;
        // if (product.BasePriceAmount == 0)
        // {
        //     var comb = (from p in _db.Products
        //         where p.Id == product.Id
        //         join c in _db.ProductVariantAttributeCombinations
        //             on p.Id equals c.ProductId
        //         where c.Price
        //               == _db.ProductVariantAttributeCombinations
        //                   .Where(x => x.ProductId == product.Id)
        //                   .Min(x => x.Price)
        //         select c).FirstOrDefault();
        //     
        //     if (comb != null)
        //     {
        //         context.CalculatedProductPrice.FinalPrice = comb.Price;
        //     }
        // }

        await next(context);
    }
}