using EShop.Infrastructure.Domain;

namespace EShop.Core.Data.DbHandlers;

public interface IDbHandleEntity
{
    public BaseEntity Entity { get; set; }
    public EntityState State { get; set; }
}



public enum EntityState
{
    Added,
    Modified,
    Deleted,
}