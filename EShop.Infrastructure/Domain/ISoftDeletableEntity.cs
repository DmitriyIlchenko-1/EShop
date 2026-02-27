namespace EShop.Infrastructure.Domain;

public interface ISoftDeletableEntity
{
    bool Deleted { get; set; }
}