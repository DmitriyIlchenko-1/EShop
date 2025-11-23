namespace EShop.Infrastructure.Domain;

public abstract class EntityWithTypedId<TId> : IEntityWithTypedId<TId>
{
    public virtual TId Id { get; set; }
}