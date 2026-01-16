using EShop.Infrastructure.Engine;

namespace EShop.Core.Data.DbHandlers;

public interface IDbHandlerProvider
{
    public void ExecuteProvider(DbHandlerProviderContext handlerProviderContext);
}


//We need a way to know about all the dbHandle types so we can request them from DI.
public class DefaultDbHandlerProvider : IDbHandlerProvider
{
   private readonly IServiceProvider _serviceProvider;

   public DefaultDbHandlerProvider(IServiceProvider serviceProvider)
   {
       _serviceProvider = serviceProvider;
   }

   public void ExecuteProvider(DbHandlerProviderContext context)
    {
         ArgumentNullException.ThrowIfNull(context);

         if (context.HandlerMetadata != null)
         {
             var results = context.Results;
             var resultCount = results.Count;
             for (int i = 0; i < resultCount; i++)
             {
                 ProvideHandler(results[i]);
             }
         }
    }

    public void ProvideHandler(DbHandlerItem item)
    {
        if (item.DbHandler != null)
        {
            return;
        }

        var handlerType = item.DbHandlerMetadata.HandlerType;
        item.DbHandler = (IDbHandler) _serviceProvider.GetService(handlerType);

        if (item.DbHandler == null)
        {
            throw new InvalidOperationException($"Handler type {handlerType.Name} can't be resolved.");
        }
    }
}

public class DbHandlerProviderContext
{
    public IList<DbHandlerMetadata> HandlerMetadata { get; set; }
    public IList<DbHandlerItem> Results { get; set; }
}

public class DbHandlerItem
{
    public DbHandlerMetadata DbHandlerMetadata { get; set; }
    public IDbHandler DbHandler { get; set; }
}