namespace EShop.Infrastructure.Domain;

public interface IAuditableEntity
{
    DateTime CreatedOnUtc { get; set; }
    DateTime ModifiedOnUtc { get; set; }
}