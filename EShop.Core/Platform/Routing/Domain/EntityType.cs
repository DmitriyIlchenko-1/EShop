using EShop.Infrastructure.Domain;

namespace EShop.Core.Platform.Routing.Domain
{
    public class EntityType : EntityWithTypedId<string>
    {
        public string Name => Id;

        public string TargetActionName { get; set; }

        public string TargetAreaName { get; set; }

        public string TargetControllerName { get; set; }
    }
}