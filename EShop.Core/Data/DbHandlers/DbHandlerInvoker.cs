namespace EShop.Core.Data.DbHandlers;


public interface IDbHandlerInvoker
{
    
}
public class DbHandlerInvoker
{
    private IEnumerable<IDbHandleEntity> _entities;
    private DbEntityHandlerContext _entityHandlerContext;
    private readonly IDbHandler[] _handlers;
    private DbHandlerPointer _pointer;


    public DbHandlerInvoker(IDbHandler[] handlers)
    {
        _handlers = handlers;
        _pointer = new DbHandlerPointer(handlers);
    }

    // private Task BeginHandlerExecutionAsync()
    // {
    // }
    //
    // private Task InvokeNextHandlerAsync()
    // {
    //     try
    //     {
    //     }
    //     catch (Exception exception)
    //     {
    //     }
    // }
    //
    // private Task Next(ref DbState status, ref bool isCompleted)
    // {
    //     switch (status)
    //     {
    //         case DbState.BeforeSaving:
    //         {
    //         }
    //     }
    // }
}