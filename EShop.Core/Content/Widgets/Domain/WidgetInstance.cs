using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Content.Widgets.Domain
{
    public class WidgetInstance : BaseEntity
    {
        public WidgetInstance()
        {
            CreateOnUtc = DateTime.UtcNow;
        }

        [StringLength(100)] 
        public string Name { get; set; }
        public string? Data { get; set; }
        public string? HtmlData { get; set; }
        public byte DisplayOrder { get; set; }

        public bool IsPublished => PublishStartUtc.HasValue && PublishStartUtc.Value < DateTimeOffset.UtcNow &&
                                   (!PublishEndUtc.HasValue || PublishEndUtc.Value > DateTimeOffset.UtcNow);

        public DateTime CreateOnUtc { get; set; }

        public DateTime? LatestUpdatedOnUtc { get; set; }
        public DateTime? PublishEndUtc { get; set; }

        public DateTime? PublishStartUtc { get; set; }

        public string WidgetId { get; set; }
        public Widget Widget { get; set; }


        public int WidgetZoneId { get; set; }
        public WidgetZone WidgetZone { get; set; }

       
    }
}