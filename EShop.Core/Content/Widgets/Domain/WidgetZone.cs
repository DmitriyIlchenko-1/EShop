using System.ComponentModel.DataAnnotations;
using EShop.Infrastructure.Domain;

namespace EShop.Core.Content.Widgets.Domain
{
    public class WidgetZone : BaseEntity
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
    }
}