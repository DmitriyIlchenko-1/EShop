using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Core.Data;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class SaveChangesAttribute<TContext>(bool saveChanges = true) : ActionFilterAttribute where TContext : DbContext
{ 
    public bool SaveChanges => saveChanges;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionExecuted = await next();

        if (actionExecuted.Exception != null)
            return;

        var actionScopedFilter = context
            .ActionDescriptor.FilterDescriptors
            .Where(x => x.Scope == FilterScope.Action)
            .Select(x => x.Filter)
            .OfType<SaveChangesAttribute<TContext>>()
            .FirstOrDefault();

        if (actionScopedFilter?.SaveChanges == false)
            return;

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<TContext>();
        await dbContext.SaveChangesAsync();
    }
}