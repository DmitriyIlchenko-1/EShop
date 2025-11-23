using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Content.Widgets.Domain
{
    public class Widget : EntityWithTypedId<string>
    {
        public Widget(string id)
        {
            Id = id;
            CreatedOnUtc = DateTime.UtcNow;
        }

        [Required(ErrorMessage = "Please enter a widget name")]
        [StringLength(100)]
        public string Name { get; set; }

        public string CreateUrl { get; set; }

        [StringLength(100)] 
        public string EditUrl { get; set; }

        public bool IsPublished { get; set; }
        [StringLength(100)] 
        public string ViewComponentName { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}