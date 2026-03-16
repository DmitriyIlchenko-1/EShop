using Microsoft.AspNetCore.Mvc.Razor;

namespace EShop.Web.Common.Razor;

public abstract class EShopRazorPage<TModel> : RazorPage<TModel>
{
    
    
    public override async Task ExecuteAsync()
    {
        
    }
}