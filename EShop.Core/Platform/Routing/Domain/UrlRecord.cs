using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Routing.Domain
{
    public class UrlRecord : BaseEntity
    {
        public string Name { get; set; }

        public string Slug { get; set; }
        public int EntityId { get; set; }

        public EntityType EntityType { get; set; }

        public string EntityTypeId { get; set; }
    }
}