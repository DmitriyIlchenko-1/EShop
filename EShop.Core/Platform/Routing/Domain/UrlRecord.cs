using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Routing.Domain
{
    public class UrlRecord : BaseEntity
    {
        public string Slug { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public bool IsActive { get; set; }
    }
}