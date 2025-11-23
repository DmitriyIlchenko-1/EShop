namespace EShop.Infrastructure.Domain;

public interface ISoftDeletedEntity
{
    bool Deleted { get; set; }
}