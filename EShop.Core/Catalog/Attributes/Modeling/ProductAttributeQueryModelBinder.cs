using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Core.Catalog.Attributes.Modeling;

public class ProductAttributeQueryModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext, nameof(bindingContext));

        var factory = bindingContext.HttpContext.RequestServices.GetService<IProductVariantQueryFactory>();

        if (factory.Current != null)
        {
            bindingContext.Result = ModelBindingResult.Success(factory.Current);
        }
        else
        {
            var query = factory.CreateFromQuery();
            bindingContext.Result = ModelBindingResult.Success(query);
        }

        return Task.CompletedTask;
    }
}