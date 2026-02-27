using EShop.Core.Data.DbHandlers;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Data.DbHandlers;
using EShop.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EShop.Tests.Data.DbHandlers;

internal class HandleEntityContextMock : IHandleEntityContext
{
    public HandleEntityContextMock(BaseEntity entity, EntityState entityState)
    {
        Entity = entity;
        EntityState = entityState;
        InitialEntityState = EntityState;
    }

    public BaseEntity Entity { get; }
    public Type EntityType => Entity.GetType();
    public EntityState InitialEntityState { get; set; }
    public EntityState EntityState { get; set; }
    public EntityEntry Entry => null;
    public bool HasStateChanged => InitialEntityState != EntityState;
}