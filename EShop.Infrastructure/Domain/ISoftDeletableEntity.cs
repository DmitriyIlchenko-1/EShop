namespace EShop.Infrastructure.Domain;

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; set; }
}