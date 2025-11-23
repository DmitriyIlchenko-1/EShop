namespace EShop.Infrastructure.Domain;

public interface IEntityWithTypedId<TId>
{
    TId Id { get; }
}